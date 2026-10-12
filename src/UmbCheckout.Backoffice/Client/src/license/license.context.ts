import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { LicenseService } from "../api/licensing/sdk.gen.js";
import { client as licensingClient } from "../api/licensing/client.gen.js";
import { whenUmbCheckoutClientReady } from "../core/api-client.js";
import { UMBCHECKOUT_LICENSE_CONTEXT } from "./license.context-token.js";

export type UmbCheckoutLicenseStatus = { status: string; isValid: boolean; isDevelopmentLicense: boolean; expiryDateTime: string };

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
        expiryDateTime: data.expiryDateTime,
      });
    }
  }

  async requestCheck() {
    await tryExecute(this, LicenseService.checkLicence());
    await this.#load();
  }
}

export { UmbCheckoutLicenseContext as api };
