import { html as c, css as f, state as E, customElement as C } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as P } from "@umbraco-cms/backoffice/lit-element";
import { UMB_WORKSPACE_CONTEXT as U } from "@umbraco-cms/backoffice/workspace";
var x = Object.defineProperty, D = Object.getOwnPropertyDescriptor, b = (t) => {
  throw TypeError(t);
}, y = (t, e, a, r) => {
  for (var i = r > 1 ? void 0 : r ? D(e, a) : e, o = t.length - 1, s; o >= 0; o--)
    (s = t[o]) && (i = (r ? s(e, a, i) : s(i)) || i);
  return r && i && x(e, a, i), i;
}, u = (t, e, a) => e.has(t) || b("Cannot " + a), g = (t, e, a) => (u(t, e, "read from private field"), e.get(t)), _ = (t, e, a) => e.has(t) ? b("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, a), $ = (t, e, a, r) => (u(t, e, "write to private field"), e.set(t, a), a), m = (t, e, a) => (u(t, e, "access private method"), a), n, d, v, k;
const p = [
  { alias: "currencyCode", editor: "Umb.PropertyEditorUi.TextBox", config: [{ alias: "maxChars", value: 3 }] },
  { alias: "successPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "cancelPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "enableShipping", editor: "Umb.PropertyEditorUi.Toggle" },
  { alias: "storeBasketInCookie", editor: "Umb.PropertyEditorUi.Toggle", licensedOnly: !0 },
  { alias: "basketInCookieExpiry", editor: "Umb.PropertyEditorUi.Integer", licensedOnly: !0 },
  { alias: "storeBasketInDatabase", editor: "Umb.PropertyEditorUi.Toggle", licensedOnly: !0 },
  { alias: "basketInDatabaseExpiry", editor: "Umb.PropertyEditorUi.Integer", licensedOnly: !0 }
], h = {
  currencyCode: "currency_code",
  successPageKey: "success_page_url",
  cancelPageKey: "cancel_page_url",
  enableShipping: "enable_shipping",
  storeBasketInCookie: "store_basket_cookie",
  basketInCookieExpiry: "store_basket_cookie_expiry",
  storeBasketInDatabase: "store_basket_database",
  basketInDatabaseExpiry: "store_basket_database_expiry"
};
let l = class extends P {
  constructor() {
    super(), _(this, d), _(this, n), this.consumeContext(U, (t) => {
      $(this, n, t), this.observe(g(this, n)?.model, (e) => this._model = e, "_model");
    });
  }
  render() {
    if (!this._model) return c`<uui-loader></uui-loader>`;
    const t = p.filter((e) => !e.licensedOnly || this._model.isLicensed);
    return c`
      <uui-box>
        <umb-property-dataset .value=${m(this, d, v).call(this, this._model)} @change=${m(this, d, k)}>
          ${t.map(
      (e) => c`<umb-property
              alias=${e.alias}
              label=${this.localize.term(`umbcheckout_${h[e.alias]}_label`)}
              description=${this.localize.term(`umbcheckout_${h[e.alias]}_description`)}
              property-editor-ui-alias=${e.editor}
              .config=${e.config ?? []}></umb-property>`
    )}
        </umb-property-dataset>
      </uui-box>
      ${this._model.isLicensed ? "" : c`<p class="unlicensed">${this.localize.term("umbcheckout_disabled_unlicensed")}</p>`}
    `;
  }
};
n = /* @__PURE__ */ new WeakMap();
d = /* @__PURE__ */ new WeakSet();
v = function(t) {
  return p.map((e) => {
    const a = t[e.alias];
    return { alias: e.alias, value: a ?? (e.editor.endsWith("DocumentPicker") ? "" : a) };
  });
};
k = function(t) {
  const e = t.target;
  for (const { alias: a, value: r } of e.value) {
    const i = p.find((s) => s.alias === a);
    if (!i) continue;
    const o = i.editor.endsWith("DocumentPicker") ? typeof r == "string" && r ? r.split(",")[0] : null : r;
    g(this, n)?.setValue(i.alias, o);
  }
};
l.styles = [f`:host { display: block; padding: var(--uui-size-layout-1); } .unlicensed { color: var(--uui-color-danger); }`];
y([
  E()
], l.prototype, "_model", 2);
l = y([
  C("umbcheckout-configuration-workspace-view")
], l);
const W = l;
export {
  l as UmbCheckoutConfigurationWorkspaceViewElement,
  W as default
};
//# sourceMappingURL=configuration-workspace-view.element-BeT_lNG1.js.map
