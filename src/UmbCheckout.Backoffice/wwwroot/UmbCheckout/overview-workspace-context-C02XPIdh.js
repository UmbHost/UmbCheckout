var C = (i) => {
  throw TypeError(i);
};
var D = (i, e, a) => e.has(i) || C("Cannot " + a);
var r = (i, e, a) => (D(i, e, "read from private field"), a ? a.call(i) : e.get(i)), n = (i, e, a) => e.has(i) ? C("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(i) : e.set(i, a);
import { UmbContextToken as I } from "@umbraco-cms/backoffice/context-api";
import { UmbContextBase as w } from "@umbraco-cms/backoffice/class-api";
import { UMB_NOTIFICATION_CONTEXT as S } from "@umbraco-cms/backoffice/notification";
import { UmbLocalizationController as O } from "@umbraco-cms/backoffice/localization-api";
import { a as l } from "./entrypoint-BuHY9PxI.js";
import { UmbValidationContext as T, UmbServerModelValidatorContext as P } from "@umbraco-cms/backoffice/validation";
class B {
  static getConfiguration(e) {
    return ((e == null ? void 0 : e.client) ?? l).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/umbcheckout/v2.0/configuration-api/get-configuration",
      ...e
    });
  }
  static hasUmbracoApplicationUrlSet(e) {
    return ((e == null ? void 0 : e.client) ?? l).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/umbcheckout/v2.0/configuration-api/has-umbraco-application-url-set",
      ...e
    });
  }
  static updateConfiguration(e) {
    return ((e == null ? void 0 : e.client) ?? l).patch({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/umbcheckout/v2.0/configuration-api/update-configuration",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
}
var c, u, o;
class y extends w {
  constructor(a) {
    super(a, x);
    n(this, c, new T(this));
    n(this, u, new P(this));
    n(this, o, new O(this));
    this.consumeContext(S, (s) => {
      this._notificationContext = s;
    });
  }
  async save() {
    await r(this, u).validate().then(async (a) => {
      console.log("Server validation passed", a), console.log("Server validation passed");
    }, () => (console.log("Server validation failed"), Promise.reject())), await r(this, c).validate().then(async () => {
      var s, m, d, h, p, f, g, b, _, v, k;
      const a = {
        url: "/umbraco/umbcheckout/v2.0/configuration-api/update-configuration",
        body: {
          basketInCookieExpiry: (s = this.propertyEditorsData.find((t) => t.alias === "basketInCookieExpiry")) == null ? void 0 : s.value,
          basketInDatabaseExpiry: (m = this.propertyEditorsData.find((t) => t.alias === "basketInDatabaseExpiry")) == null ? void 0 : m.value,
          cancelPageUrl: (d = this.propertyEditorsData.find((t) => t.alias === "cancelPageUrl")) == null ? void 0 : d.value,
          successPageUrl: (h = this.propertyEditorsData.find((t) => t.alias === "successPageUrl")) == null ? void 0 : h.value,
          currencyCode: (p = this.propertyEditorsData.find((t) => t.alias === "currencyCode")) == null ? void 0 : p.value,
          enableShipping: (f = this.propertyEditorsData.find((t) => t.alias === "enableShipping")) == null ? void 0 : f.value,
          storeBasketInCookie: (g = this.propertyEditorsData.find((t) => t.alias === "storeBasketInCookie")) == null ? void 0 : g.value,
          storeBasketInDatabase: (b = this.propertyEditorsData.find((t) => t.alias === "storeBasketInDatabase")) == null ? void 0 : b.value
        }
      };
      try {
        await B.updateConfiguration(a);
        const t = { headline: r(this, o).term("umbcheckout_configuration_saved_title"), message: r(this, o).term("umbcheckout_configuration_saved_message") };
        (_ = this._notificationContext) == null || _.peek("positive", { data: t });
      } catch (t) {
        if ((t == null ? void 0 : t.status) === 400) {
          const U = { headline: r(this, o).term("umbcheckout_configuration_failed_save_title"), message: r(this, o).term("umbcheckout_configuration_bad_request_message") };
          return (v = this._notificationContext) == null || v.peek("danger", { data: U }), Promise.reject(t);
        }
        const E = { headline: r(this, o).term("umbcheckout_configuration_failed_save_title"), message: r(this, o).term("umbcheckout_configuration_failed_save_message") };
        (k = this._notificationContext) == null || k.peek("danger", { data: E });
      }
    }, () => {
      var s;
      const a = { headline: r(this, o).term("umbcheckout_configuration_failed_save_title"), message: r(this, o).term("umbcheckout_configuration_failed_save_message") };
      return (s = this._notificationContext) == null || s.peek("danger", { data: a }), Promise.reject();
    });
  }
}
c = new WeakMap(), u = new WeakMap(), o = new WeakMap();
const j = y, x = new I(
  "UmbWorkspaceContext",
  "umbcheckout-overview-workspace-context"
), R = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  UMBCHECKOUT_OVERVIEW_WORKSPACE_CONTEXT: x,
  UmbCheckoutOverviewWorkspaceContextElement: y,
  api: j
}, Symbol.toStringTag, { value: "Module" }));
export {
  B as C,
  x as U,
  R as o
};
//# sourceMappingURL=overview-workspace-context-C02XPIdh.js.map
