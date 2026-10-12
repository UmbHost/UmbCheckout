import { html as s, nothing as p, css as O, state as g, customElement as I } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as L } from "@umbraco-cms/backoffice/lit-element";
import { UMB_NOTIFICATION_CONTEXT as W } from "@umbraco-cms/backoffice/notification";
import { tryExecute as A } from "@umbraco-cms/backoffice/resources";
import { UMB_WORKSPACE_CONTEXT as B } from "@umbraco-cms/backoffice/workspace";
import { C as K } from "./sdk.gen-bktVcCtQ.js";
import { c as M } from "./client.gen-CGf2xBln.js";
import { w as N } from "./api-client-DCzFYg7y.js";
import { U as q } from "./license.context-token-4gXpqREX.js";
import { U as V } from "./unlicensed-alert.element-BxElw7Mq.js";
var H = Object.defineProperty, X = Object.getOwnPropertyDescriptor, U = (e) => {
  throw TypeError(e);
}, k = (e, t, i, c) => {
  for (var r = c > 1 ? void 0 : c ? X(t, i) : t, h = e.length - 1, m; h >= 0; h--)
    (m = e[h]) && (r = (c ? m(t, i, r) : m(r)) || r);
  return c && r && H(t, i, r), r;
}, y = (e, t, i) => t.has(e) || U("Cannot " + i), u = (e, t, i) => (y(e, t, "read from private field"), t.get(e)), f = (e, t, i) => t.has(e) ? U("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, i), v = (e, t, i, c) => (y(e, t, "write to private field"), t.set(e, i), i), o = (e, t, i) => (y(e, t, "access private method"), i), _, d, b, a, w, E, D, S, z, P, l, T;
const $ = [
  { alias: "successPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "cancelPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "currencyCode", editor: "Umb.PropertyEditorUi.TextBox", config: [{ alias: "maxChars", value: 3 }] },
  { alias: "enableShipping", editor: "Umb.PropertyEditorUi.Toggle" },
  { alias: "storeBasketInCookie", editor: "Umb.PropertyEditorUi.Toggle", licensedOnly: !0 },
  { alias: "basketInCookieExpiry", editor: "Umb.PropertyEditorUi.Integer", licensedOnly: !0 },
  { alias: "storeBasketInDatabase", editor: "Umb.PropertyEditorUi.Toggle", licensedOnly: !0 },
  { alias: "basketInDatabaseExpiry", editor: "Umb.PropertyEditorUi.Integer", licensedOnly: !0 }
], x = {
  currencyCode: "currency_code",
  successPageKey: "success_page_url",
  cancelPageKey: "cancel_page_url",
  enableShipping: "enable_shipping",
  storeBasketInCookie: "store_basket_cookie",
  basketInCookieExpiry: "store_basket_cookie_expiry",
  storeBasketInDatabase: "store_basket_database",
  basketInDatabaseExpiry: "store_basket_database_expiry"
}, C = "0000-00-00";
let n = class extends L {
  constructor() {
    super(), f(this, a), f(this, _), f(this, d), f(this, b), this._applicationUrlSet = !0, this.consumeContext(B, (e) => {
      v(this, _, e), this.observe(u(this, _)?.model, (t) => this._model = t, "_model");
    }), this.consumeContext(q, (e) => {
      v(this, d, e), this.observe(e?.status, (t) => this._license = t, "_license");
    }), this.consumeContext(W, (e) => v(this, b, e)), o(this, a, w).call(this);
  }
  render() {
    return this._model ? s`<div class="dashboard-overview">
      <div class="left-col">${o(this, a, z).call(this)} ${o(this, a, P).call(this, this._model)}</div>
      <div class="right-col">${o(this, a, T).call(this)}</div>
    </div>` : s`<uui-loader></uui-loader>`;
  }
};
_ = /* @__PURE__ */ new WeakMap();
d = /* @__PURE__ */ new WeakMap();
b = /* @__PURE__ */ new WeakMap();
a = /* @__PURE__ */ new WeakSet();
w = async function() {
  await N(M);
  const { data: e } = await A(this, K.getApplicationUrlStatus());
  this._applicationUrlSet = e?.isSet ?? !0;
};
E = function(e) {
  return $.map((t) => {
    const i = e[t.alias];
    return { alias: t.alias, value: i ?? (t.editor.endsWith("DocumentPicker") ? "" : i) };
  });
};
D = function(e) {
  const t = e.target;
  for (const { alias: i, value: c } of t.value) {
    const r = $.find((m) => m.alias === i);
    if (!r) continue;
    const h = r.editor.endsWith("DocumentPicker") ? typeof c == "string" && c ? c.split(",")[0] : null : c;
    u(this, _)?.setValue(r.alias, h);
  }
};
S = async function() {
  if (!u(this, d)) return;
  this._checkState = "waiting";
  const e = await u(this, d).requestCheck();
  e?.accepted ? (this._checkState = "success", u(this, b)?.peek("positive", {
    data: { headline: this.localize.term("umbcheckout_license_check_requested_title"), message: this.localize.term("umbcheckout_license_check_requested_message") }
  })) : e ? (this._checkState = "failed", u(this, b)?.peek("warning", {
    data: {
      headline: this.localize.term("umbcheckout_license_check_request_wait_title"),
      message: `${this.localize.term("umbcheckout_license_check_request_wait_message")} ${e.retryAfterSeconds ?? 0} ${this.localize.term("umbcheckout_seconds")}`
    }
  })) : this._checkState = "failed";
};
z = function() {
  const e = this._license?.isValid === !0;
  return s`<uui-box headline=${this.localize.term("umbcheckout_help_support")}>
      <p class="muted">
        ${this.localize.term("umbcheckout_documentation_description")}
        <a href="https://docs.umbcheckout.net/" target="_blank" rel="noopener">https://docs.umbcheckout.net/</a><br />
        ${this.localize.term("umbcheckout_github_report_issues")}
        <a href="https://github.com/UmbHost/UmbCheckout/issues" target="_blank" rel="noopener">${this.localize.term("umbcheckout_github_repo")}</a><br />
        ${e ? s`${this.localize.term("umbcheckout_support_ticket")}
              <a href="https://www.umbpanel.io" target="_blank" rel="noopener">UmbPanel</a>` : p}
      </p>
      <umbcheckout-unlicensed-alert></umbcheckout-unlicensed-alert>
    </uui-box>`;
};
P = function(e) {
  const t = $.filter((i) => !i.licensedOnly || e.isLicensed);
  return s`<uui-box headline=${this.localize.term("umbcheckout_configuration")}>
      <umb-property-dataset .value=${o(this, a, E).call(this, e)} @change=${o(this, a, D)}>
        ${t.map(
    (i) => s`<umb-property
            alias=${i.alias}
            label=${this.localize.term(`umbcheckout_${x[i.alias]}_label`)}
            description=${this.localize.term(`umbcheckout_${x[i.alias]}_description`)}
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
T = function() {
  const e = this._license;
  return s`<uui-box headline=${this.localize.term("umbcheckout_license_details")}>
        ${this._applicationUrlSet ? p : s`<div class="umbcheckout-alert" role="alert">
              ${this.localize.term("umbcheckout_application_url_unset_before")}
              <a href="https://docs.umbraco.com/umbraco-cms/reference/configuration/webroutingsettings#umbraco-application-url" target="_blank" rel="noopener"
                >${this.localize.term("umbcheckout_application_url_unset_link")}</a
              >.<br />${this.localize.term("umbcheckout_application_url_unset_after")}
            </div>`}
        ${e ? s`${e.isDevelopmentLicense ? o(this, a, l).call(this, "umbcheckout_license_development", this.localize.term("umbcheckout_license_development_true")) : p}
              ${o(this, a, l).call(this, "umbcheckout_license_status", e.status)} ${o(this, a, l).call(this, "umbcheckout_registration_date", e.regDate)}
              ${e.expiryDateTime && e.expiryDateTime !== C ? o(this, a, l).call(this, "umbcheckout_expiry_date", e.expiryDateTime) : p}
              ${o(this, a, l).call(this, "umbcheckout_valid_domains", e.validDomains)} ${o(this, a, l).call(this, "umbcheckout_valid_path", e.validPaths)}` : s`<uui-loader></uui-loader>`}
        <uui-button look="primary" .state=${this._checkState} ?disabled=${this._checkState === "waiting"} @click=${o(this, a, S)}
          >${this.localize.term("umbcheckout_recheck_license")}</uui-button
        >
      </uui-box>
      ${(e?.addons ?? []).map(
    (t) => s`<uui-box headline=${this.localize.term("umbcheckout_license_addon")}>
          ${o(this, a, l).call(this, "umbcheckout_license_addon_name", t.name)} ${o(this, a, l).call(this, "umbcheckout_license_status", t.status)}
          ${t.nextDueDate && t.nextDueDate !== C ? o(this, a, l).call(this, "umbcheckout_expiry_date", t.nextDueDate) : p}
        </uui-box>`
  )}`;
};
n.styles = [
  V,
  O`
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
  I("umbcheckout-configuration-workspace-view")
], n);
const ie = n;
export {
  n as UmbCheckoutConfigurationWorkspaceViewElement,
  ie as default
};
//# sourceMappingURL=configuration-workspace-view.element-BSS9QiNC.js.map
