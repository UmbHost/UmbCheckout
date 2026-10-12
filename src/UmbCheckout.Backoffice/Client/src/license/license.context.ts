import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { LicenseService } from "../api/licensing/sdk.gen.js";
import { client as licensingClient } from "../api/licensing/client.gen.js";
import { whenUmbCheckoutClientReady } from "../core/api-client.js";
import { UMBCHECKOUT_LICENSE_CONTEXT } from "./license.context-token.js";

export type UmbCheckoutLicenseAddon = { name: string; status: string; nextDueDate: string };

export type UmbCheckoutLicenseStatus = {
  status: string;
  isValid: boolean;
  isDevelopmentLicense: boolean;
  regDate: string;
  expiryDateTime: string;
  validDomains: string;
  validPaths: string;
  addons: Array<UmbCheckoutLicenseAddon>;
};

/** Result of a re-check request: accepted, or refused with the seconds to wait before retrying. */
export type UmbCheckoutLicenseCheckResult = { accepted: boolean; retryAfterSeconds?: number | null };

export class UmbCheckoutLicenseContext extends UmbContextBase {
  #status = new UmbObjectState<UmbCheckoutLicenseStatus | undefined>(undefined);
  readonly status = this.#status.asObservable();
  readonly isLicensed = this.#status.asObservablePart((s) => s?.isValid === true);

  constructor(host: UmbControllerHost) {
    super(host, UMBCHECKOUT_LICENSE_CONTEXT);
    this.#load();
  }

  async #load() {
    // Global contexts can be created before the entry point has configured the client's auth.
    await whenUmbCheckoutClientReady(licensingClient);
    const { data } = await tryExecute(this, LicenseService.getLicenseStatus());
    if (data) {
      this.#status.setValue({
        status: data.status,
        isValid: data.isValid,
        isDevelopmentLicense: data.isDevelopmentLicense,
        regDate: data.regDate,
        expiryDateTime: data.expiryDateTime,
        validDomains: data.validDomains,
        validPaths: data.validPaths,
        addons: (data.addons ?? []).map((a) => ({ name: a.name, status: a.status, nextDueDate: a.nextDueDate })),
      });
    }
  }

  /** Asks the licence server to re-check, then reloads the status when the check was accepted. */
  async requestCheck(): Promise<UmbCheckoutLicenseCheckResult | undefined> {
    const { data } = await tryExecute(this, LicenseService.checkLicence());
    if (data?.accepted) await this.#load();
    return data ? { accepted: data.accepted, retryAfterSeconds: data.retryAfterSeconds } : undefined;
  }
}

export { UmbCheckoutLicenseContext as api };
