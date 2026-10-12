import { UmbContextBase as r } from "@umbraco-cms/backoffice/class-api";
import { UmbObjectState as n } from "@umbraco-cms/backoffice/observable-api";
import { tryExecute as a } from "@umbraco-cms/backoffice/resources";
import { c as s } from "./client.gen-C2rnZBGa.js";
import { w as m } from "./api-client-DCzFYg7y.js";
import { U as o } from "./license.context-token-4gXpqREX.js";
class i {
  static checkLicence(e) {
    return (e?.client ?? s).post({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/umbhost-licensing/licence-check",
      ...e
    });
  }
  static getLicenseStatus(e) {
    return (e?.client ?? s).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/umbhost-licensing/license-status",
      ...e
    });
  }
}
class v extends r {
  constructor(e) {
    super(e, o), this.#e = new n(void 0), this.status = this.#e.asObservable(), this.isLicensed = this.#e.asObservablePart((t) => t?.isValid === !0), this.#t();
  }
  #e;
  async #t() {
    await m(s);
    const { data: e } = await a(this, i.getLicenseStatus());
    e && this.#e.setValue({
      status: e.status,
      isValid: e.isValid,
      isDevelopmentLicense: e.isDevelopmentLicense,
      regDate: e.regDate,
      expiryDateTime: e.expiryDateTime,
      validDomains: e.validDomains,
      validPaths: e.validPaths,
      addons: (e.addons ?? []).map((t) => ({ name: t.name, status: t.status, nextDueDate: t.nextDueDate }))
    });
  }
  /** Asks the licence server to re-check, then reloads the status when the check was accepted. */
  async requestCheck() {
    const { data: e } = await a(this, i.checkLicence());
    return e?.accepted && await this.#t(), e ? { accepted: e.accepted, retryAfterSeconds: e.retryAfterSeconds } : void 0;
  }
}
export {
  v as UmbCheckoutLicenseContext,
  v as api
};
//# sourceMappingURL=license.context-BJZfb2-5.js.map
