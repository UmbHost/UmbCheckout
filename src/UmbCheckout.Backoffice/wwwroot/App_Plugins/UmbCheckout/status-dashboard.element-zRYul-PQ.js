import { nothing as h, html as s, css as b, state as m, customElement as g } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as v } from "@umbraco-cms/backoffice/lit-element";
import { tryExecute as f } from "@umbraco-cms/backoffice/resources";
import { c as n } from "./client.gen-CGf2xBln.js";
import { w as y } from "./api-client-DCzFYg7y.js";
import { U as C } from "./license.context-token-4gXpqREX.js";
class U {
  static getConfiguration(t) {
    return (t?.client ?? n).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/umbcheckout/configuration",
      ...t
    });
  }
  static updateConfiguration(t) {
    return (t?.client ?? n).put({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/umbcheckout/configuration",
      ...t,
      headers: {
        "Content-Type": "application/json",
        ...t?.headers
      }
    });
  }
  static getApplicationUrlStatus(t) {
    return (t?.client ?? n).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/umbcheckout/configuration/application-url",
      ...t
    });
  }
}
var S = Object.defineProperty, E = Object.getOwnPropertyDescriptor, _ = (e) => {
  throw TypeError(e);
}, p = (e, t, a, o) => {
  for (var i = o > 1 ? void 0 : o ? E(t, a) : t, c = e.length - 1, u; c >= 0; c--)
    (u = e[c]) && (i = (o ? u(t, a, i) : u(i)) || i);
  return o && i && S(t, a, i), i;
}, w = (e, t, a) => t.has(e) || _("Cannot " + a), k = (e, t, a) => t.has(e) ? _("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, a), x = (e, t, a) => (w(e, t, "access private method"), a), l, d;
let r = class extends v {
  constructor() {
    super(), k(this, l), this._applicationUrlSet = !0, this.consumeContext(C, (e) => {
      this.observe(e?.status, (t) => this._license = t, "_license");
    }), x(this, l, d).call(this);
  }
  render() {
    return s`
      ${this._applicationUrlSet ? h : s`<uui-box><p class="warning">${this.localize.term("umbcheckout_application_url_missing")}</p></uui-box>`}
      <uui-box headline=${this.localize.term("umbcheckout_license_status")}>
        ${this._license ? s`<p><uui-tag color=${this._license.isValid ? "positive" : "danger"}>${this._license.status}</uui-tag></p>
              ${this._license.isValid ? h : s`<p>${this.localize.term("umbcheckout_disabled_unlicensed")}</p>`}` : s`<uui-loader></uui-loader>`}
      </uui-box>
    `;
  }
};
l = /* @__PURE__ */ new WeakSet();
d = async function() {
  await y(n);
  const { data: e } = await f(this, U.getApplicationUrlStatus());
  this._applicationUrlSet = e?.isSet ?? !0;
};
r.styles = [
  b`
      :host { display: grid; gap: var(--uui-size-layout-1); padding: var(--uui-size-layout-1); }
      .warning { color: var(--uui-color-warning-standalone); }
    `
];
p([
  m()
], r.prototype, "_license", 2);
p([
  m()
], r.prototype, "_applicationUrlSet", 2);
r = p([
  g("umbcheckout-status-dashboard")
], r);
const P = r;
export {
  r as UmbCheckoutStatusDashboardElement,
  P as default
};
//# sourceMappingURL=status-dashboard.element-zRYul-PQ.js.map
