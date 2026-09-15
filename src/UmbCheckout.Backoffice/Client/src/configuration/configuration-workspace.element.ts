import { css, customElement, html, nothing, state, unsafeHTML, when } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
// Ensures the umb-input-multi-url element is defined
import "@umbraco-cms/backoffice/multi-url-picker";
import "../components/route-addendum.element.js";
import type { UmbInputMultiUrlElement } from "@umbraco-cms/backoffice/multi-url-picker";
import type { UUIBooleanInputEvent, UUIButtonState, UUIInputElement } from "@umbraco-cms/backoffice/external/uui";
import {
  UmbCheckoutConfigurationService,
  type UmbCheckoutConfigurationModel,
  type UmbCheckoutConfigurationResponseModel,
} from "../api/configuration.js";

@customElement("umbcheckout-configuration-workspace")
export class UmbCheckoutConfigurationWorkspaceElement extends UmbLitElement {
  @state()
  private _configuration?: UmbCheckoutConfigurationResponseModel;

  @state()
  private _saveButtonState?: UUIButtonState;

  @state()
  private _validationErrors: Array<string> = [];

  #notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

  constructor() {
    super();

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
      this.#notificationContext = context;
    });
  }

  override connectedCallback() {
    super.connectedCallback();
    this.#load();
  }

  async #load() {
    const { data } = await tryExecute(this, UmbCheckoutConfigurationService.getConfiguration());
    if (data) {
      this._configuration = data;
    }
  }

  #update<K extends keyof UmbCheckoutConfigurationModel>(key: K, value: UmbCheckoutConfigurationModel[K]) {
    if (!this._configuration) return;
    this._configuration = { ...this._configuration, [key]: value };
  }

  #validate(configuration: UmbCheckoutConfigurationModel) {
    const errors: Array<string> = [];
    if (configuration.successPageUrl.length !== 1) errors.push("successPageUrl");
    if (configuration.cancelPageUrl.length !== 1) errors.push("cancelPageUrl");
    if (!/^[A-Za-z]{3}$/.test(configuration.currencyCode ?? "")) errors.push("currencyCode");
    this._validationErrors = errors;
    return errors.length === 0;
  }

  async #onSave() {
    if (!this._configuration) return;

    const configuration: UmbCheckoutConfigurationModel = {
      successPageUrl: this._configuration.successPageUrl,
      cancelPageUrl: this._configuration.cancelPageUrl,
      currencyCode: this._configuration.currencyCode,
      enableShipping: this._configuration.enableShipping,
      storeBasketInCookie: this._configuration.storeBasketInCookie,
      basketInCookieExpiry: Number(this._configuration.basketInCookieExpiry),
      storeBasketInDatabase: this._configuration.storeBasketInDatabase,
      basketInDatabaseExpiry: Number(this._configuration.basketInDatabaseExpiry),
    };

    if (!this.#validate(configuration)) {
      this._saveButtonState = "failed";
      return;
    }

    this._saveButtonState = "waiting";

    const { data, error } = await tryExecute(this, UmbCheckoutConfigurationService.updateConfiguration(configuration), {
      disableNotifications: true,
    });

    if (error || !data) {
      this._saveButtonState = "failed";
      this.#notificationContext?.peek("danger", {
        data: {
          headline: this.localize.term("umbcheckout_configuration_failed_save_title"),
          message: this.localize.term("umbcheckout_configuration_failed_save_message"),
        },
      });
      return;
    }

    this._configuration = data;
    this._saveButtonState = "success";
    this.#notificationContext?.peek("positive", {
      data: {
        headline: this.localize.term("umbcheckout_configuration_saved_title"),
        message: this.localize.term("umbcheckout_configuration_saved_message"),
      },
    });
  }

  override render() {
    return html`
      <umb-body-layout headline="UmbCheckout">
        ${this._configuration
          ? html`<div class="dashboard-overview">
              <div class="left-col">${this.#renderHelp()} ${this.#renderConfiguration(this._configuration)}</div>
              <div class="right-col">${this.#renderLicense()}</div>
            </div>`
          : html`<uui-loader></uui-loader>`}
        ${when(
          this._configuration && !this._configuration.isLicensed,
          () => html`<div slot="footer-info" class="made-by">${unsafeHTML(this.localize.term("umbcheckout_made_by"))}</div>`,
        )}
        <uui-button
          slot="actions"
          look="primary"
          color="positive"
          label=${this.localize.term("buttons_save")}
          .state=${this._saveButtonState}
          ?disabled=${!this._configuration || this._saveButtonState === "waiting"}
          @click=${this.#onSave}></uui-button>
      </umb-body-layout>
    `;
  }

  #renderHelp() {
    return html`
      <uui-box headline=${this.localize.term("umbcheckout_help_support")}>
        <p>
          ${unsafeHTML(this.localize.term("umbcheckout_documentation_description"))}<br />
          ${unsafeHTML(this.localize.term("umbcheckout_github_report_issues"))}<br />
          ${when(this._configuration?.isLicensed, () => html`${unsafeHTML(this.localize.term("umbcheckout_support_ticket"))}<br />`)}
        </p>
        ${when(
          !this._configuration?.isLicensed,
          () => html`<p class="unlicensed">${unsafeHTML(this.localize.term("umbcheckout_unlicensed_warning"))}</p>`,
        )}
      </uui-box>
    `;
  }

  #renderConfiguration(configuration: UmbCheckoutConfigurationResponseModel) {
    return html`
      <uui-box headline=${this.localize.term("umbcheckout_configuration")}>
        <umb-property-layout
          label=${this.localize.term("umbcheckout_success_page_url_label")}
          description=${this.localize.term("umbcheckout_success_page_url_description")}
          mandatory
          ?invalid=${this._validationErrors.includes("successPageUrl")}>
          <umbcheckout-route-addendum slot="editor" addendum="successPageUrl">
            <umb-input-multi-url
              .min=${1}
              .max=${1}
              .urls=${configuration.successPageUrl}
              @change=${(e: Event) =>
                this.#update("successPageUrl", (e.target as UmbInputMultiUrlElement).urls)}></umb-input-multi-url>
          </umbcheckout-route-addendum>
        </umb-property-layout>

        <umb-property-layout
          label=${this.localize.term("umbcheckout_cancel_page_url_label")}
          description=${this.localize.term("umbcheckout_cancel_page_url_description")}
          mandatory
          ?invalid=${this._validationErrors.includes("cancelPageUrl")}>
          <umbcheckout-route-addendum slot="editor" addendum="cancelPageUrl">
            <umb-input-multi-url
              .min=${1}
              .max=${1}
              .urls=${configuration.cancelPageUrl}
              @change=${(e: Event) =>
                this.#update("cancelPageUrl", (e.target as UmbInputMultiUrlElement).urls)}></umb-input-multi-url>
          </umbcheckout-route-addendum>
        </umb-property-layout>

        <umb-property-layout
          label=${this.localize.term("umbcheckout_currency_code_label")}
          description=${this.localize.term("umbcheckout_currency_code_description")}
          mandatory
          ?invalid=${this._validationErrors.includes("currencyCode")}>
          <uui-input
            slot="editor"
            label=${this.localize.term("umbcheckout_currency_code_label")}
            maxlength="3"
            .value=${configuration.currencyCode}
            @input=${(e: Event) => this.#update("currencyCode", (e.target as UUIInputElement).value as string)}></uui-input>
        </umb-property-layout>

        <umb-property-layout
          label=${this.localize.term("umbcheckout_enable_shipping_label")}
          description=${this.localize.term("umbcheckout_enable_shipping_description")}>
          <uui-toggle
            slot="editor"
            label=${this.localize.term("umbcheckout_enable_shipping_label")}
            .checked=${configuration.enableShipping}
            @change=${(e: UUIBooleanInputEvent) => this.#update("enableShipping", e.target.checked)}></uui-toggle>
        </umb-property-layout>

        ${configuration.isLicensed ? this.#renderLicensedConfiguration(configuration) : nothing}
      </uui-box>
    `;
  }

  #renderLicensedConfiguration(configuration: UmbCheckoutConfigurationResponseModel) {
    return html`
      <umb-property-layout
        label=${this.localize.term("umbcheckout_store_basket_cookie_label")}
        description=${this.localize.term("umbcheckout_store_basket_cookie_description")}>
        <uui-toggle
          slot="editor"
          label=${this.localize.term("umbcheckout_store_basket_cookie_label")}
          .checked=${configuration.storeBasketInCookie}
          @change=${(e: UUIBooleanInputEvent) => this.#update("storeBasketInCookie", e.target.checked)}></uui-toggle>
      </umb-property-layout>

      <umb-property-layout
        label=${this.localize.term("umbcheckout_store_basket_cookie_expiry_label")}
        description=${this.localize.term("umbcheckout_store_basket_cookie_expiry_description")}
        mandatory>
        <uui-input
          slot="editor"
          type="number"
          min="1"
          label=${this.localize.term("umbcheckout_store_basket_cookie_expiry_label")}
          .value=${String(configuration.basketInCookieExpiry)}
          @input=${(e: Event) =>
            this.#update("basketInCookieExpiry", Number((e.target as UUIInputElement).value))}></uui-input>
      </umb-property-layout>

      <umb-property-layout
        label=${this.localize.term("umbcheckout_store_basket_database_label")}
        description=${this.localize.term("umbcheckout_store_basket_database_description")}>
        <uui-toggle
          slot="editor"
          label=${this.localize.term("umbcheckout_store_basket_database_label")}
          .checked=${configuration.storeBasketInDatabase}
          @change=${(e: UUIBooleanInputEvent) => this.#update("storeBasketInDatabase", e.target.checked)}></uui-toggle>
      </umb-property-layout>

      <umb-property-layout
        label=${this.localize.term("umbcheckout_store_basket_database_expiry_label")}
        description=${this.localize.term("umbcheckout_store_basket_database_expiry_description")}
        mandatory>
        <uui-input
          slot="editor"
          type="number"
          min="1"
          label=${this.localize.term("umbcheckout_store_basket_database_expiry_label")}
          .value=${String(configuration.basketInDatabaseExpiry)}
          @input=${(e: Event) =>
            this.#update("basketInDatabaseExpiry", Number((e.target as UUIInputElement).value))}></uui-input>
      </umb-property-layout>
    `;
  }

  #renderLicense() {
    // TODO: UmbHost.Licensing - re-enable once a Umbraco 17 compatible version of UmbHost.Licensing is available
    // The license details box (status, registration date, expiry date, valid domains / paths, addons, re-check license button)
    // and the "Umbraco Application Url is not set" warning were loaded from the UmbHost.Licensing LicenseStatusApi / LicenceCheck endpoints.
    return nothing;
  }

  static override styles = [
    UmbTextStyles,
    css`
      :host {
        display: block;
        width: 100%;
        height: 100%;
      }

      .dashboard-overview {
        display: flex;
        flex-direction: row;
        gap: var(--uui-size-layout-1);
      }

      .left-col {
        flex: 1 1 auto;
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-layout-1);
      }

      .right-col:empty {
        display: none;
      }

      .right-col {
        flex: 0 0 350px;
      }

      .unlicensed {
        color: var(--uui-color-danger);
        font-weight: bold;
      }

      .unlicensed a {
        color: inherit;
        text-decoration: underline;
      }
    `,
  ];
}

export default UmbCheckoutConfigurationWorkspaceElement;

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-configuration-workspace": UmbCheckoutConfigurationWorkspaceElement;
  }
}
