import { css as i, state as c, customElement as d, nothing as h, html as m } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as b } from "@umbraco-cms/backoffice/lit-element";
import { U as _ } from "./license.context-token-4gXpqREX.js";
const p = i`
  .umbcheckout-alert {
    display: block;
    padding: var(--uui-size-space-4) var(--uui-size-space-5);
    margin-bottom: var(--uui-size-space-5);
    border: 1px solid var(--uui-color-danger-standalone);
    border-left-width: 4px;
    border-radius: var(--uui-border-radius);
    background-color: var(--uui-color-danger);
    color: var(--uui-color-danger-contrast);
    font-weight: 700;
  }
  .umbcheckout-alert--info {
    border-color: var(--uui-color-default);
    background-color: var(--uui-color-default);
    color: var(--uui-color-default-contrast);
    font-weight: 400;
  }
  .umbcheckout-alert a {
    color: inherit;
    text-decoration: underline;
  }
`;
var f = Object.defineProperty, v = Object.getOwnPropertyDescriptor, u = (o, e, l, s) => {
  for (var t = s > 1 ? void 0 : s ? v(e, l) : e, a = o.length - 1, n; a >= 0; a--)
    (n = o[a]) && (t = (s ? n(e, l, t) : n(t)) || t);
  return s && t && f(e, l, t), t;
};
let r = class extends b {
  constructor() {
    super(), this._unlicensed = !1, this._addOnsInstalled = !1, this.consumeContext(_, (o) => {
      this.observe(
        o?.status,
        (e) => {
          this._unlicensed = e !== void 0 && !e.isValid, this._addOnsInstalled = e?.addOnsInstalled === !0;
        },
        "_status"
      );
    });
  }
  render() {
    return this._unlicensed ? m`<div class="umbcheckout-alert ${this._addOnsInstalled ? "" : "umbcheckout-alert--info"}" role=${this._addOnsInstalled ? "alert" : "status"}>
      ${this.localize.term("umbcheckout_unlicensed_warning_before")}
      <a href="https://umbcheckout.net" target="_blank" rel="noopener">${this.localize.term("umbcheckout_purchase_license")}</a>
      ${this.localize.term("umbcheckout_unlicensed_warning_after")}
    </div>` : h;
  }
};
r.styles = [p];
u([
  c()
], r.prototype, "_unlicensed", 2);
u([
  c()
], r.prototype, "_addOnsInstalled", 2);
r = u([
  d("umbcheckout-unlicensed-alert")
], r);
export {
  p as U,
  r as a
};
//# sourceMappingURL=unlicensed-alert.element-BFU93TIj.js.map
