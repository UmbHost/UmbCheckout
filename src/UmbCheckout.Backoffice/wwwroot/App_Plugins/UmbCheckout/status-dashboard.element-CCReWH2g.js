import { nothing as p, html as o, css as d, state as h, customElement as v } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as f } from "@umbraco-cms/backoffice/lit-element";
import { tryExecute as b } from "@umbraco-cms/backoffice/resources";
import { C } from "./sdk.gen-bktVcCtQ.js";
import { c as U } from "./client.gen-CGf2xBln.js";
import { w as g } from "./api-client-DCzFYg7y.js";
import { U as S } from "./license.context-token-4gXpqREX.js";
var E = Object.defineProperty, w = Object.getOwnPropertyDescriptor, _ = (t) => {
  throw TypeError(t);
}, u = (t, e, a, r) => {
  for (var i = r > 1 ? void 0 : r ? w(e, a) : e, n = t.length - 1, l; n >= 0; n--)
    (l = t[n]) && (i = (r ? l(e, a, i) : l(i)) || i);
  return r && i && E(e, a, i), i;
}, y = (t, e, a) => e.has(t) || _("Cannot " + a), x = (t, e, a) => e.has(t) ? _("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, a), $ = (t, e, a) => (y(t, e, "access private method"), a), c, m;
let s = class extends f {
  constructor() {
    super(), x(this, c), this._applicationUrlSet = !0, this.consumeContext(S, (t) => {
      this.observe(t?.status, (e) => this._license = e, "_license");
    }), $(this, c, m).call(this);
  }
  render() {
    return o`
      ${this._applicationUrlSet ? p : o`<uui-box><p class="warning">${this.localize.term("umbcheckout_application_url_missing")}</p></uui-box>`}
      <uui-box headline=${this.localize.term("umbcheckout_license_status")}>
        ${this._license ? o`<p><uui-tag color=${this._license.isValid ? "positive" : "danger"}>${this._license.status}</uui-tag></p>
              ${this._license.isValid ? p : o`<p>${this.localize.term("umbcheckout_disabled_unlicensed")}</p>`}` : o`<uui-loader></uui-loader>`}
      </uui-box>
    `;
  }
};
c = /* @__PURE__ */ new WeakSet();
m = async function() {
  await g(U);
  const { data: t } = await b(this, C.getApplicationUrlStatus());
  this._applicationUrlSet = t?.isSet ?? !0;
};
s.styles = [
  d`
      :host { display: grid; gap: var(--uui-size-layout-1); padding: var(--uui-size-layout-1); }
      .warning { color: var(--uui-color-warning-standalone); }
    `
];
u([
  h()
], s.prototype, "_license", 2);
u([
  h()
], s.prototype, "_applicationUrlSet", 2);
s = u([
  v("umbcheckout-status-dashboard")
], s);
const L = s;
export {
  s as UmbCheckoutStatusDashboardElement,
  L as default
};
//# sourceMappingURL=status-dashboard.element-CCReWH2g.js.map
