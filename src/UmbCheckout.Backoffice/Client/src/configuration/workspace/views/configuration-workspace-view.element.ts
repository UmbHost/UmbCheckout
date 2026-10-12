import { css, customElement, html, nothing, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import type { UmbPropertyDatasetElement, UmbPropertyValueData } from "@umbraco-cms/backoffice/property";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { UMB_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/workspace";
import type { UUIButtonState } from "@umbraco-cms/backoffice/external/uui";
import { ConfigurationService } from "../../../api/sdk.gen.js";
import { client } from "../../../api/client.gen.js";
import { whenUmbCheckoutClientReady } from "../../../core/api-client.js";
import { UMBCHECKOUT_LICENSE_CONTEXT } from "../../../license/license.context-token.js";
import type { UmbCheckoutLicenseContext, UmbCheckoutLicenseStatus } from "../../../license/license.context.js";
import { UmbCheckoutAlertStyles } from "../../../shared/alert.styles.js";
import "../../../shared/unlicensed-alert.element.js";
import type { UmbCheckoutConfigurationWorkspaceContext } from "../configuration-workspace.context.js";
import type { UmbCheckoutConfigurationModel } from "../../types.js";

type Field = { alias: keyof UmbCheckoutConfigurationModel; editor: string; config?: Array<{ alias: string; value: unknown }>; licensedOnly?: boolean };

const FIELDS: Array<Field> = [
  { alias: "successPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "cancelPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "currencyCode", editor: "Umb.PropertyEditorUi.TextBox", config: [{ alias: "maxChars", value: 3 }] },
  { alias: "enableShipping", editor: "Umb.PropertyEditorUi.Toggle" },
  { alias: "storeBasketInCookie", editor: "Umb.PropertyEditorUi.Toggle", licensedOnly: true },
  { alias: "basketInCookieExpiry", editor: "Umb.PropertyEditorUi.Integer", licensedOnly: true },
  { alias: "storeBasketInDatabase", editor: "Umb.PropertyEditorUi.Toggle", licensedOnly: true },
  { alias: "basketInDatabaseExpiry", editor: "Umb.PropertyEditorUi.Integer", licensedOnly: true },
];

const LABEL_KEYS: Record<string, string> = {
  currencyCode: "currency_code",
  successPageKey: "success_page_url",
  cancelPageKey: "cancel_page_url",
  enableShipping: "enable_shipping",
  storeBasketInCookie: "store_basket_cookie",
  basketInCookieExpiry: "store_basket_cookie_expiry",
  storeBasketInDatabase: "store_basket_database",
  basketInDatabaseExpiry: "store_basket_database_expiry",
};

/** The licence server's placeholder for "no date". */
const NO_DATE = "0000-00-00";

/** v13's Settings → UmbCheckout page: Help & Support and Configuration on the left, licence details on the right. */
@customElement("umbcheckout-configuration-workspace-view")
export class UmbCheckoutConfigurationWorkspaceViewElement extends UmbLitElement {
  #context?: UmbCheckoutConfigurationWorkspaceContext;
  #licenseContext?: UmbCheckoutLicenseContext;
  #notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

  @state() private _model?: UmbCheckoutConfigurationModel;
  @state() private _license?: UmbCheckoutLicenseStatus;
  @state() private _applicationUrlSet = true;
  @state() private _checkState?: UUIButtonState;

  constructor() {
    super();
    this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
      this.#context = context as UmbCheckoutConfigurationWorkspaceContext;
      this.observe(this.#context?.model, (model) => (this._model = model), "_model");
    });
    this.consumeContext(UMBCHECKOUT_LICENSE_CONTEXT, (context) => {
      this.#licenseContext = context;
      this.observe(context?.status, (status) => (this._license = status), "_license");
    });
    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => (this.#notificationContext = context));
    this.#loadApplicationUrl();
  }

  async #loadApplicationUrl() {
    await whenUmbCheckoutClientReady(client);
    const { data } = await tryExecute(this, ConfigurationService.getApplicationUrlStatus());
    this._applicationUrlSet = data?.isSet ?? true;
  }

  #toDataset(model: UmbCheckoutConfigurationModel): Array<UmbPropertyValueData> {
    return FIELDS.map((f) => {
      const value = model[f.alias];
      // Document picker values are comma-separated GUID strings in 17.0.
      return { alias: f.alias, value: value ?? (f.editor.endsWith("DocumentPicker") ? "" : value) };
    });
  }

  #onChange(event: Event) {
    const dataset = event.target as UmbPropertyDatasetElement;
    for (const { alias, value } of dataset.value) {
      const field = FIELDS.find((f) => f.alias === alias);
      if (!field) continue;
      const normalised = field.editor.endsWith("DocumentPicker") ? (typeof value === "string" && value ? value.split(",")[0] : null) : value;
      this.#context?.setValue(field.alias, normalised as never);
    }
  }

  async #recheckLicense() {
    if (!this.#licenseContext) return;
    this._checkState = "waiting";
    const result = await this.#licenseContext.requestCheck();
    if (result?.accepted) {
      this._checkState = "success";
      this.#notificationContext?.peek("positive", {
        data: { headline: this.localize.term("umbcheckout_license_check_requested_title"), message: this.localize.term("umbcheckout_license_check_requested_message") },
      });
    } else if (result) {
      this._checkState = "failed";
      this.#notificationContext?.peek("warning", {
        data: {
          headline: this.localize.term("umbcheckout_license_check_request_wait_title"),
          message: `${this.localize.term("umbcheckout_license_check_request_wait_message")} ${result.retryAfterSeconds ?? 0} ${this.localize.term("umbcheckout_seconds")}`,
        },
      });
    } else {
      // tryExecute has already shown the error notification.
      this._checkState = "failed";
    }
  }

  #renderHelp() {
    const licensed = this._license?.isValid === true;
    return html`<uui-box headline=${this.localize.term("umbcheckout_help_support")}>
      <p class="muted">
        ${this.localize.term("umbcheckout_documentation_description")}
        <a href="https://docs.umbcheckout.net/" target="_blank" rel="noopener">https://docs.umbcheckout.net/</a><br />
        ${this.localize.term("umbcheckout_github_report_issues")}
        <a href="https://github.com/UmbHost/UmbCheckout/issues" target="_blank" rel="noopener">${this.localize.term("umbcheckout_github_repo")}</a><br />
        ${licensed
          ? html`${this.localize.term("umbcheckout_support_ticket")}
              <a href="https://my.umbhost.net/submitticket.php?step=2&deptid=8&subject=UmbCheckout:%20[ISSUE%20NAME%20HERE]" target="_blank" rel="noopener">My.UmbHost</a>`
          : nothing}
      </p>
      <umbcheckout-unlicensed-alert></umbcheckout-unlicensed-alert>
    </uui-box>`;
  }

  #renderConfiguration(model: UmbCheckoutConfigurationModel) {
    const fields = FIELDS.filter((f) => !f.licensedOnly || model.isLicensed);
    return html`<uui-box headline=${this.localize.term("umbcheckout_configuration")}>
      <umb-property-dataset .value=${this.#toDataset(model)} @change=${this.#onChange}>
        ${fields.map(
          (f) => html`<umb-property
            alias=${f.alias}
            label=${this.localize.term(`umbcheckout_${LABEL_KEYS[f.alias]}_label`)}
            description=${this.localize.term(`umbcheckout_${LABEL_KEYS[f.alias]}_description`)}
            property-editor-ui-alias=${f.editor}
            .config=${f.config ?? []}></umb-property>`,
        )}
      </umb-property-dataset>
    </uui-box>`;
  }

  #row(labelKey: string, value: unknown) {
    return html`<umb-property-layout label=${this.localize.term(labelKey)} orientation="vertical">
      <div slot="editor">${value}</div>
    </umb-property-layout>`;
  }

  #renderLicense() {
    const license = this._license;
    return html`<uui-box headline=${this.localize.term("umbcheckout_license_details")}>
        ${this._applicationUrlSet
          ? nothing
          : html`<div class="umbcheckout-alert" role="alert">
              ${this.localize.term("umbcheckout_application_url_unset_before")}
              <a href="https://docs.umbraco.com/umbraco-cms/reference/configuration/webroutingsettings#umbraco-application-url" target="_blank" rel="noopener"
                >${this.localize.term("umbcheckout_application_url_unset_link")}</a
              >.<br />${this.localize.term("umbcheckout_application_url_unset_after")}
            </div>`}
        ${license
          ? html`${license.isDevelopmentLicense ? this.#row("umbcheckout_license_development", this.localize.term("umbcheckout_license_development_true")) : nothing}
              ${this.#row("umbcheckout_license_status", license.status)} ${this.#row("umbcheckout_registration_date", license.regDate)}
              ${license.expiryDateTime && license.expiryDateTime !== NO_DATE ? this.#row("umbcheckout_expiry_date", license.expiryDateTime) : nothing}
              ${this.#row("umbcheckout_valid_domains", license.validDomains)} ${this.#row("umbcheckout_valid_path", license.validPaths)}`
          : html`<uui-loader></uui-loader>`}
        <uui-button look="primary" .state=${this._checkState} ?disabled=${this._checkState === "waiting"} @click=${this.#recheckLicense}
          >${this.localize.term("umbcheckout_recheck_license")}</uui-button
        >
      </uui-box>
      ${(license?.addons ?? []).map(
        (addon) => html`<uui-box headline=${this.localize.term("umbcheckout_license_addon")}>
          ${this.#row("umbcheckout_license_addon_name", addon.name)} ${this.#row("umbcheckout_license_status", addon.status)}
          ${addon.nextDueDate && addon.nextDueDate !== NO_DATE ? this.#row("umbcheckout_expiry_date", addon.nextDueDate) : nothing}
        </uui-box>`,
      )}`;
  }

  override render() {
    if (!this._model) return html`<uui-loader></uui-loader>`;
    return html`<div class="dashboard-overview">
      <div class="left-col">${this.#renderHelp()} ${this.#renderConfiguration(this._model)}</div>
      <div class="right-col">${this.#renderLicense()}</div>
    </div>`;
  }

  static override styles = [
    UmbCheckoutAlertStyles,
    css`
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
    `,
  ];
}

export default UmbCheckoutConfigurationWorkspaceViewElement;

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-configuration-workspace-view": UmbCheckoutConfigurationWorkspaceViewElement;
  }
}
