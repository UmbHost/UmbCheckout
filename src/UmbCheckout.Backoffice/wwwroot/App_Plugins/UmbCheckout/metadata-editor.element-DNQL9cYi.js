import { repeat as $, html as s, css as b, property as d, customElement as g } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as E } from "@umbraco-cms/backoffice/lit-element";
import { UmbChangeEvent as k } from "@umbraco-cms/backoffice/event";
var w = Object.defineProperty, C = Object.getOwnPropertyDescriptor, _ = (t) => {
  throw TypeError(t);
}, m = (t, e, a, r) => {
  for (var i = r > 1 ? void 0 : r ? C(e, a) : e, c = t.length - 1, h; c >= 0; c--)
    (h = t[c]) && (i = (r ? h(e, a, i) : h(i)) || i);
  return r && i && w(e, a, i), i;
}, f = (t, e, a) => e.has(t) || _("Cannot " + a), y = (t, e, a) => (f(t, e, "read from private field"), a ? a.call(t) : e.get(t)), z = (t, e, a) => e.has(t) ? _("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, a), o = (t, e, a) => (f(t, e, "access private method"), a), l, v, n, p;
let u = class extends E {
  constructor() {
    super(...arguments), z(this, l), this.readonly = !1;
  }
  render() {
    const t = y(this, l, v);
    return s`
      ${$(
      t,
      (e, a) => a,
      (e, a) => s`<div class="row">
          <uui-input label=${this.localize.term("umbcheckout_metadata_name")} .value=${e.name} ?readonly=${this.readonly}
            @change=${(r) => o(this, l, p).call(this, a, "name", r.target.value)}></uui-input>
          <uui-input label=${this.localize.term("umbcheckout_metadata_value")} .value=${e.value} ?readonly=${this.readonly}
            @change=${(r) => o(this, l, p).call(this, a, "value", r.target.value)}></uui-input>
          ${this.readonly ? "" : s`<uui-button compact label=${this.localize.term("general_remove")} @click=${() => o(this, l, n).call(this, t.filter((r, i) => i !== a))}><uui-icon name="icon-trash"></uui-icon></uui-button>`}
        </div>`
    )}
      ${this.readonly ? "" : s`<uui-button look="placeholder" label=${this.localize.term("general_add")} @click=${() => o(this, l, n).call(this, [...t, { name: "", value: "" }])}></uui-button>`}
    `;
  }
};
l = /* @__PURE__ */ new WeakSet();
v = function() {
  if (typeof this.value == "string")
    try {
      return JSON.parse(this.value);
    } catch {
      return [];
    }
  return this.value ?? [];
};
n = function(t) {
  this.value = t, this.dispatchEvent(new k());
};
p = function(t, e, a) {
  o(this, l, n).call(this, y(this, l, v).map((r, i) => i === t ? { ...r, [e]: a } : r));
};
u.styles = [b`.row { display: grid; grid-template-columns: 1fr 1fr auto; gap: var(--uui-size-space-3); margin-bottom: var(--uui-size-space-3); } uui-button[look="placeholder"] { width: 100%; }`];
m([
  d({ attribute: !1 })
], u.prototype, "value", 2);
m([
  d({ type: Boolean, reflect: !0 })
], u.prototype, "readonly", 2);
u = m([
  g("umbcheckout-metadata-editor")
], u);
const M = u;
export {
  u as UmbCheckoutMetaDataEditorElement,
  M as default
};
//# sourceMappingURL=metadata-editor.element-DNQL9cYi.js.map
