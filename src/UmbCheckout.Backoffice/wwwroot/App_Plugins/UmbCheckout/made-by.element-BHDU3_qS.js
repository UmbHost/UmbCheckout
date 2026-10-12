import { css as c, state as a, customElement as m, nothing as h, html as _ } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as p } from "@umbraco-cms/backoffice/lit-element";
import { U as b } from "./license.context-token-4gXpqREX.js";
var f = Object.defineProperty, d = Object.getOwnPropertyDescriptor, l = (s, e, u, o) => {
  for (var t = o > 1 ? void 0 : o ? d(e, u) : e, i = s.length - 1, n; i >= 0; i--)
    (n = s[i]) && (t = (o ? n(e, u, t) : n(t)) || t);
  return o && t && f(e, u, t), t;
};
let r = class extends p {
  constructor() {
    super(), this._unlicensed = !1, this.consumeContext(b, (s) => {
      this.observe(s?.status, (e) => this._unlicensed = e !== void 0 && !e.isValid, "_status");
    });
  }
  render() {
    return this._unlicensed ? _`<uui-button look="outline" href="https://umbhost.net" target="_blank" rel="noopener">
      ${this.localize.term("umbcheckout_made_with")}
      <uui-icon class="heart" name="icon-hearts"></uui-icon>
      ${this.localize.term("umbcheckout_by_umbhost")}
    </uui-button>` : h;
  }
};
r.styles = [c`.heart { color: var(--uui-palette-maroon-flush, #d42054); margin: 0 var(--uui-size-space-1); }`];
l([
  a()
], r.prototype, "_unlicensed", 2);
r = l([
  m("umbcheckout-made-by")
], r);
export {
  r as U
};
//# sourceMappingURL=made-by.element-BHDU3_qS.js.map
