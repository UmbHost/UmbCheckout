import { html as s, nothing as u, css as I, state as g, customElement as L } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as W } from "@umbraco-cms/backoffice/lit-element";
import { UMB_NOTIFICATION_CONTEXT as A } from "@umbraco-cms/backoffice/notification";
import { tryExecute as N } from "@umbraco-cms/backoffice/resources";
import { UMB_WORKSPACE_CONTEXT as B } from "@umbraco-cms/backoffice/workspace";
import { C as M } from "./sdk.gen-bktVcCtQ.js";
import { c as K } from "./client.gen-CGf2xBln.js";
import { w as q } from "./api-client-DCzFYg7y.js";
import { U as V } from "./license.context-token-4gXpqREX.js";
import { U as H } from "./unlicensed-alert.element-BxElw7Mq.js";
var X = Object.defineProperty, F = Object.getOwnPropertyDescriptor, w = (e) => {
  throw TypeError(e);
}, k = (e, t, i, c) => {
  for (var r = c > 1 ? void 0 : c ? F(t, i) : t, m = e.length - 1, _; m >= 0; m--)
    (_ = e[m]) && (r = (c ? _(t, i, r) : _(r)) || r);
  return c && r && X(t, i, r), r;
}, $ = (e, t, i) => t.has(e) || w("Cannot " + i), h = (e, t, i) => ($(e, t, "read from private field"), t.get(e)), f = (e, t, i) => t.has(e) ? w("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, i), v = (e, t, i, c) => ($(e, t, "write to private field"), t.set(e, i), i), o = (e, t, i) => ($(e, t, "access private method"), i), p, d, b, a, E, D, z, S, P, O, l, y, T;
const x = [
  { alias: "successPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "cancelPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "currencyCode", editor: "Umb.PropertyEditorUi.TextBox", config: [{ alias: "maxChars", value: 3 }] },
  { alias: "enableShipping", editor: "Umb.PropertyEditorUi.Toggle" },
  { alias: "storeBasketInCookie", editor: "Umb.PropertyEditorUi.Toggle", licensedOnly: !0 },
  { alias: "basketInCookieExpiry", editor: "Umb.PropertyEditorUi.Integer", licensedOnly: !0 },
  { alias: "storeBasketInDatabase", editor: "Umb.PropertyEditorUi.Toggle", licensedOnly: !0 },
  { alias: "basketInDatabaseExpiry", editor: "Umb.PropertyEditorUi.Integer", licensedOnly: !0 }
], C = {
  currencyCode: "currency_code",
  successPageKey: "success_page_url",
  cancelPageKey: "cancel_page_url",
  enableShipping: "enable_shipping",
  storeBasketInCookie: "store_basket_cookie",
  basketInCookieExpiry: "store_basket_cookie_expiry",
  storeBasketInDatabase: "store_basket_database",
  basketInDatabaseExpiry: "store_basket_database_expiry"
}, U = "0000-00-00", R = "Domain not licensed";
let n = class extends W {
  constructor() {
    super(), f(this, a), f(this, p), f(this, d), f(this, b), this._applicationUrlSet = !0, this.consumeContext(B, (e) => {
      v(this, p, e), this.observe(h(this, p)?.model, (t) => this._model = t, "_model");
    }), this.consumeContext(V, (e) => {
      v(this, d, e), this.observe(e?.status, (t) => this._license = t, "_license");
    }), this.consumeContext(A, (e) => v(this, b, e)), o(this, a, E).call(this);
  }
  render() {
    return this._model ? s`<div class="dashboard-overview">
      <div class="left-col">${o(this, a, P).call(this)} ${o(this, a, O).call(this, this._model)}</div>
      <div class="right-col">${o(this, a, T).call(this)}</div>
    </div>` : s`<uui-loader></uui-loader>`;
  }
};
p = /* @__PURE__ */ new WeakMap();
d = /* @__PURE__ */ new WeakMap();
b = /* @__PURE__ */ new WeakMap();
a = /* @__PURE__ */ new WeakSet();
E = async function() {
  await q(K);
  const { data: e } = await N(this, M.getApplicationUrlStatus());
  this._applicationUrlSet = e?.isSet ?? !0;
};
D = function(e) {
  return x.map((t) => {
    const i = e[t.alias];
    return { alias: t.alias, value: i ?? (t.editor.endsWith("DocumentPicker") ? "" : i) };
  });
};
z = function(e) {
  const t = e.target;
  for (const { alias: i, value: c } of t.value) {
    const r = x.find((_) => _.alias === i);
    if (!r) continue;
    const m = r.editor.endsWith("DocumentPicker") ? typeof c == "string" && c ? c.split(",")[0] : null : c;
    h(this, p)?.setValue(r.alias, m);
  }
};
S = async function() {
  if (!h(this, d)) return;
  this._checkState = "waiting";
  const e = await h(this, d).requestCheck();
  e?.accepted ? (this._checkState = "success", h(this, b)?.peek("positive", {
    data: { headline: this.localize.term("umbcheckout_license_check_requested_title"), message: this.localize.term("umbcheckout_license_check_requested_message") }
  })) : e ? (this._checkState = "failed", h(this, b)?.peek("warning", {
    data: {
      headline: this.localize.term("umbcheckout_license_check_request_wait_title"),
      message: `${this.localize.term("umbcheckout_license_check_request_wait_message")} ${e.retryAfterSeconds ?? 0} ${this.localize.term("umbcheckout_seconds")}`
    }
  })) : this._checkState = "failed";
};
P = function() {
  const e = this._license?.isValid === !0;
  return s`<uui-box headline=${this.localize.term("umbcheckout_help_support")}>
      <p class="muted">
        ${this.localize.term("umbcheckout_documentation_description")}
        <a href="https://docs.umbcheckout.net/" target="_blank" rel="noopener">https://docs.umbcheckout.net/</a><br />
        ${this.localize.term("umbcheckout_github_report_issues")}
        <a href="https://github.com/UmbHost/UmbCheckout/issues" target="_blank" rel="noopener">${this.localize.term("umbcheckout_github_repo")}</a><br />
        ${e ? s`${this.localize.term("umbcheckout_support_ticket")}
              <a href="https://www.umbpanel.io" target="_blank" rel="noopener">UmbPanel</a>` : u}
      </p>
      <umbcheckout-unlicensed-alert></umbcheckout-unlicensed-alert>
    </uui-box>`;
};
O = function(e) {
  const t = x.filter((i) => !i.licensedOnly || e.isLicensed);
  return s`<uui-box headline=${this.localize.term("umbcheckout_configuration")}>
      <umb-property-dataset .value=${o(this, a, D).call(this, e)} @change=${o(this, a, z)}>
        ${t.map(
    (i) => s`<umb-property
            alias=${i.alias}
            label=${this.localize.term(`umbcheckout_${C[i.alias]}_label`)}
            description=${this.localize.term(`umbcheckout_${C[i.alias]}_description`)}
            property-editor-ui-alias=${i.editor}
            .config=${i.config ?? []}></umb-property>`
  )}
      </umb-property-dataset>
    </uui-box>`;
};
l = function(e, t) {
  return s`<umb-property-layout label=${this.localize.term(e)} orientation="vertical">
      <div slot="editor">${t}</div>
    </umb-property-layout>`;
};
y = function(e) {
  return (e ?? "").split(",").map((t) => t.trim()).filter((t) => t.length > 0).join(", ");
};
T = function() {
  const e = this._license;
  return s`<uui-box headline=${this.localize.term("umbcheckout_license_details")}>
        ${this._applicationUrlSet ? u : s`<div class="umbcheckout-alert" role="alert">
              ${this.localize.term("umbcheckout_application_url_unset_before")}
              <a href="https://docs.umbraco.com/umbraco-cms/reference/configuration/webroutingsettings#umbraco-application-url" target="_blank" rel="noopener"
                >${this.localize.term("umbcheckout_application_url_unset_link")}</a
              >.<br />${this.localize.term("umbcheckout_application_url_unset_after")}
            </div>`}
        ${e?.status === R ? s`<div class="umbcheckout-alert" role="alert">
              ${this.localize.term("umbcheckout_domain_not_licensed")} <strong>${o(this, a, y).call(this, e.validDomains)}</strong>.<br />${this.localize.term("umbcheckout_domain_not_licensed_after")}
            </div>` : u}
        ${e ? s`${e.isDevelopmentLicense ? o(this, a, l).call(this, "umbcheckout_license_development", this.localize.term("umbcheckout_license_development_true")) : u}
              ${o(this, a, l).call(this, "umbcheckout_license_status", e.status)} ${o(this, a, l).call(this, "umbcheckout_registration_date", e.regDate)}
              ${e.expiryDateTime && e.expiryDateTime !== U ? o(this, a, l).call(this, "umbcheckout_expiry_date", e.expiryDateTime) : u}
              ${o(this, a, l).call(this, "umbcheckout_valid_domains", o(this, a, y).call(this, e.validDomains))} ${o(this, a, l).call(this, "umbcheckout_valid_path", e.validPaths)}` : s`<uui-loader></uui-loader>`}
        <uui-button look="primary" .state=${this._checkState} ?disabled=${this._checkState === "waiting"} @click=${o(this, a, S)}
          >${this.localize.term("umbcheckout_recheck_license")}</uui-button
        >
      </uui-box>
      ${(e?.addons ?? []).map(
    (t) => s`<uui-box headline=${this.localize.term("umbcheckout_license_addon")}>
          ${o(this, a, l).call(this, "umbcheckout_license_addon_name", t.name)} ${o(this, a, l).call(this, "umbcheckout_license_status", t.status)}
          ${t.nextDueDate && t.nextDueDate !== U ? o(this, a, l).call(this, "umbcheckout_expiry_date", t.nextDueDate) : u}
        </uui-box>`
  )}`;
};
n.styles = [
  H,
  I`
      :host {
        display: block;
        padding: var(--uui-size-layout-1);
      }
      /* v13 umbcheckout.css: flexible left column, 350px right column. */
      .dashboard-overview {
        display: grid;
        grid-template-columns: minmax(0, 1fr) 350px;
        gap: var(--uui-size-layout-1);
        align-items: start;
      }
      @media (max-width: 1024px) {
        .dashboard-overview {
          grid-template-columns: minmax(0, 1fr);
        }
      }
      .left-col,
      .right-col {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-layout-1);
      }
      .muted {
        color: var(--uui-color-text-alt);
        margin-top: 0;
      }
      umb-property-layout {
        padding-top: 0;
      }
      /* Licence values such as the valid path can be long, unbroken strings. */
      .right-col [slot="editor"] {
        overflow-wrap: anywhere;
      }
    `
];
k([
  g()
], n.prototype, "_model", 2);
k([
  g()
], n.prototype, "_license", 2);
k([
  g()
], n.prototype, "_applicationUrlSet", 2);
k([
  g()
], n.prototype, "_checkState", 2);
n = k([
  L("umbcheckout-configuration-workspace-view")
], n);
const oe = n;
export {
  n as UmbCheckoutConfigurationWorkspaceViewElement,
  oe as default
};
//# sourceMappingURL=configuration-workspace-view.element-w_j2ZrL1.js.map
