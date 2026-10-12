import { UmbContextBase as r } from "@umbraco-cms/backoffice/class-api";
import { UmbObjectState as n } from "@umbraco-cms/backoffice/observable-api";
import { tryExecute as s } from "@umbraco-cms/backoffice/resources";
import { c as t } from "./client.gen-C2rnZBGa.js";
import { w as m } from "./api-client-DCzFYg7y.js";
import { U as o } from "./license.context-token-4gXpqREX.js";
class i {
  static checkLicence(e) {
    return (e?.client ?? t).post({
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
    return (e?.client ?? t).get({
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
class C extends r {
  constructor(e) {
    super(e, o), this.#e = new n(void 0), this.status = this.#e.asObservable(), this.isLicensed = this.#e.asObservablePart((c) => c?.isValid === !0), this.#t();
  }
  #e;
  async #t() {
    await m(t);
    const { data: e } = await s(this, i.getLicenseStatus());
    e && this.#e.setValue({
      status: e.status,
      isValid: e.isValid,
      isDevelopmentLicense: e.isDevelopmentLicense,
      expiryDateTime: e.expiryDateTime
    });
  }
  async requestCheck() {
    await s(this, i.checkLicence()), await this.#t();
  }
}
export {
  C as UmbCheckoutLicenseContext,
  C as api
};
//# sourceMappingURL=license.context-fIjwuSaP.js.map
