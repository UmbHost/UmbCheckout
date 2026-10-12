import { css as u, state as l, customElement as d, nothing as m, html as h } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as b } from "@umbraco-cms/backoffice/lit-element";
import { U as p } from "./license.context-token-4gXpqREX.js";
const _ = u`
  .umbcheckout-alert {
    display: block;
    padding: var(--uui-size-space-4) var(--uui-size-space-5);
    margin-bottom: var(--uui-size-space-5);
    border: 1px solid var(--uui-color-danger-standalone);
    border-left-width: 4px;
    border-radius: var(--uui-border-radius);
    background-color: var(--uui-color-danger-emphasis, #fbe6e6);
    color: var(--uui-color-danger-standalone);
    font-weight: 700;
  }
  .umbcheckout-alert a {
    color: inherit;
    text-decoration: underline;
  }
`;
var v = Object.defineProperty, f = Object.getOwnPropertyDescriptor, c = (t, e, i, o) => {
  for (var r = o > 1 ? void 0 : o ? f(e, i) : e, n = t.length - 1, a; n >= 0; n--)
    (a = t[n]) && (r = (o ? a(e, i, r) : a(r)) || r);
  return o && r && v(e, i, r), r;
};
let s = class extends b {
  constructor() {
    super(), this._unlicensed = !1, this.consumeContext(p, (t) => {
      this.observe(t?.status, (e) => this._unlicensed = e !== void 0 && !e.isValid, "_status");
    });
  }
  render() {
    return this._unlicensed ? h`<div class="umbcheckout-alert" role="alert">
      ${this.localize.term("umbcheckout_unlicensed_warning_before")}
      <a href="https://umbcheckout.net" target="_blank" rel="noopener">${this.localize.term("umbcheckout_purchase_license")}</a>
      ${this.localize.term("umbcheckout_unlicensed_warning_after")}
    </div>` : m;
  }
};
s.styles = [_];
c([
  l()
], s.prototype, "_unlicensed", 2);
s = c([
  d("umbcheckout-unlicensed-alert")
], s);
export {
  _ as U,
  s as a
};
//# sourceMappingURL=unlicensed-alert.element-BxElw7Mq.js.map
