import { nothing as c, html as s, css as d, state as h, customElement as v } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as b } from "@umbraco-cms/backoffice/lit-element";
import { tryExecute as f } from "@umbraco-cms/backoffice/resources";
import { C as g } from "./sdk.gen-bktVcCtQ.js";
import { c as C } from "./client.gen-CGf2xBln.js";
import { w as U } from "./api-client-DCzFYg7y.js";
import { U as S } from "./license.context-token-4gXpqREX.js";
var w = Object.defineProperty, E = Object.getOwnPropertyDescriptor, _ = (t) => {
  throw TypeError(t);
}, p = (t, e, i, r) => {
  for (var a = r > 1 ? void 0 : r ? E(e, i) : e, n = t.length - 1, l; n >= 0; n--)
    (l = t[n]) && (a = (r ? l(e, i, a) : l(a)) || a);
  return r && a && w(e, i, a), a;
}, y = (t, e, i) => e.has(t) || _("Cannot " + i), $ = (t, e, i) => e.has(t) ? _("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, i), x = (t, e, i) => (y(t, e, "access private method"), i), u, m;
let o = class extends b {
  constructor() {
    super(), $(this, u), this._applicationUrlSet = !0, this.consumeContext(S, (t) => {
      this.observe(t?.status, (e) => this._license = e, "_license");
    }), x(this, u, m).call(this);
  }
  render() {
    return s`
      ${this._applicationUrlSet ? c : s`<uui-box><p class="warning">${this.localize.term("umbcheckout_application_url_missing")}</p></uui-box>`}
      <uui-box headline=${this.localize.term("umbcheckout_license_status")}>
        ${this._license ? s`<p>
                <uui-tag color=${this._license.isValid ? "positive" : "danger"}>${this._license.status}</uui-tag>
                ${this._license.isDevelopmentLicense ? s`<uui-tag color="warning">${this.localize.term("umbcheckout_license_development")}</uui-tag>` : c}
              </p>
              ${this._license.isValid ? c : s`<p>${this.localize.term("umbcheckout_disabled_unlicensed")}</p>`}` : s`<uui-loader></uui-loader>`}
      </uui-box>
    `;
  }
};
u = /* @__PURE__ */ new WeakSet();
m = async function() {
  await U(C);
  const { data: t } = await f(this, g.getApplicationUrlStatus());
  this._applicationUrlSet = t?.isSet ?? !0;
};
o.styles = [
  d`
      :host { display: grid; gap: var(--uui-size-layout-1); padding: var(--uui-size-layout-1); }
      .warning { color: var(--uui-color-warning-standalone); }
    `
];
p([
  h()
], o.prototype, "_license", 2);
p([
  h()
], o.prototype, "_applicationUrlSet", 2);
o = p([
  v("umbcheckout-status-dashboard")
], o);
const L = o;
export {
  o as UmbCheckoutStatusDashboardElement,
  L as default
};
//# sourceMappingURL=status-dashboard.element-BnUdR4xT.js.map
