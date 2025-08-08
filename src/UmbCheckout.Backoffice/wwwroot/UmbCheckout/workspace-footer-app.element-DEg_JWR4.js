import { LitElement as l, html as u, css as h, customElement as _ } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as v } from "@umbraco-cms/backoffice/element-api";
import { UmbLocalizationController as d } from "@umbraco-cms/backoffice/localization-api";
import { UmbTextStyles as f } from "@umbraco-cms/backoffice/style";
var k = Object.getOwnPropertyDescriptor, i = (e) => {
  throw TypeError(e);
}, b = (e, t, r, m) => {
  for (var o = m > 1 ? void 0 : m ? k(t, r) : t, s = e.length - 1, p; s >= 0; s--)
    (p = e[s]) && (o = p(o) || o);
  return o;
}, y = (e, t, r) => t.has(e) || i("Cannot " + r), c = (e, t, r) => (y(e, t, "read from private field"), r ? r.call(e) : t.get(e)), w = (e, t, r) => t.has(e) ? i("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, r), a;
let n = class extends v(l) {
  constructor() {
    super(), w(this, a, new d(this));
  }
  render() {
    return u`
		<span>${c(this, a).term("umbcheckout_made_with")} <umb-icon name="icon-hearts" color="color-red"></umb-icon> ${c(this, a).term("umbcheckout_made_by")} <a href="https://umbcheckout.net" target="_blank">${c(this, a).term("umbcheckout_company_name")}</a></span>`;
  }
};
a = /* @__PURE__ */ new WeakMap();
n.styles = [
  f,
  h`
        span {
            display: flex;
            margin-left: var(--uui-size-layout-1);
            }
            umb-icon, a {
                margin: 0 4px;
            }`
];
n = b([
  _("umbcheckout-workspace-footer-app")
], n);
const U = n;
export {
  n as UmbCheckoutWorkspaceFooterAppElement,
  U as default
};
//# sourceMappingURL=workspace-footer-app.element-DEg_JWR4.js.map
