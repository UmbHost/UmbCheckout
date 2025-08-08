var k = (i) => {
  throw TypeError(i);
};
var x = (i, e, a) => e.has(i) || k("Cannot " + a);
var r = (i, e, a) => (x(i, e, "read from private field"), a ? a.call(i) : e.get(i)), c = (i, e, a) => e.has(i) ? k("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(i) : e.set(i, a);
import { UmbContextToken as E } from "@umbraco-cms/backoffice/context-api";
import { UmbContextBase as U } from "@umbraco-cms/backoffice/class-api";
import { UMB_NOTIFICATION_CONTEXT as D } from "@umbraco-cms/backoffice/notification";
import { UmbLocalizationController as I } from "@umbraco-cms/backoffice/localization-api";
import { a as d } from "./entrypoint-C4Z0gcPx.js";
import { UmbValidationContext as w, UmbServerModelValidatorContext as S } from "@umbraco-cms/backoffice/validation";
class O {
  static getConfiguration(e) {
    return ((e == null ? void 0 : e.client) ?? d).get({
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
    return ((e == null ? void 0 : e.client) ?? d).get({
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
    return ((e == null ? void 0 : e.client) ?? d).patch({
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
var u, l, o;
class C extends U {
  constructor(a) {
    super(a, y);
    c(this, u, new w(this));
    c(this, l, new S(this));
    c(this, o, new I(this));
    this.consumeContext(D, (n) => {
      this._notificationContext = n;
    });
  }
  async save() {
    await r(this, l).validate().then(async (a) => {
      console.log("Server validation passed", a), console.log("Server validation passed");
    }, () => (console.log("Server validation failed"), Promise.reject())), await r(this, u).validate().then(async () => {
      var n, h, m, p, f, b, g, v;
      const a = {
        url: "/umbraco/umbcheckout/v2.0/configuration-api/update-configuration",
        body: {
          basketInCookieExpiry: (n = this.propertyEditorsData.find((t) => t.alias === "basketInCookieExpiry")) == null ? void 0 : n.value,
          basketInDatabaseExpiry: (h = this.propertyEditorsData.find((t) => t.alias === "basketInDatabaseExpiry")) == null ? void 0 : h.value,
          cancelPageUrl: (m = this.propertyEditorsData.find((t) => t.alias === "cancelPageUrl")) == null ? void 0 : m.value,
          successPageUrl: (p = this.propertyEditorsData.find((t) => t.alias === "successPageUrl")) == null ? void 0 : p.value,
          currencyCode: (f = this.propertyEditorsData.find((t) => t.alias === "currencyCode")) == null ? void 0 : f.value,
          enableShipping: (b = this.propertyEditorsData.find((t) => t.alias === "enableShipping")) == null ? void 0 : b.value,
          storeBasketInCookie: (g = this.propertyEditorsData.find((t) => t.alias === "storeBasketInCookie")) == null ? void 0 : g.value,
          storeBasketInDatabase: (v = this.propertyEditorsData.find((t) => t.alias === "storeBasketInDatabase")) == null ? void 0 : v.value
        }
      };
      await O.updateConfiguration(a).then(() => {
        var s;
        const t = { headline: r(this, o).term("umbcheckout_configuration_saved_title"), message: r(this, o).term("umbcheckout_configuration_saved_message") };
        (s = this._notificationContext) == null || s.peek("positive", { data: t });
      }).catch((t) => {
        var _;
        const s = { headline: r(this, o).term("umbcheckout_configuration_failed_save_title"), message: r(this, o).term("umbcheckout_configuration_failed_save_message") };
        (_ = this._notificationContext) == null || _.peek("danger", { data: s });
      });
    }, () => {
      var n;
      const a = { headline: r(this, o).term("umbcheckout_configuration_failed_save_title"), message: r(this, o).term("umbcheckout_configuration_failed_save_message") };
      return (n = this._notificationContext) == null || n.peek("danger", { data: a }), Promise.reject();
    });
  }
}
u = new WeakMap(), l = new WeakMap(), o = new WeakMap();
const T = C, y = new E(
  "UmbWorkspaceContext",
  "umbcheckout-overview-workspace-context"
), z = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  UMBCHECKOUT_OVERVIEW_WORKSPACE_CONTEXT: y,
  UmbCheckoutOverviewWorkspaceContextElement: C,
  api: T
}, Symbol.toStringTag, { value: "Module" }));
export {
  O as C,
  y as U,
  z as o
};
//# sourceMappingURL=overview-workspace-context-C797DZyv.js.map
