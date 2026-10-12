import { css, customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import type { UmbPropertyDatasetElement, UmbPropertyValueData } from "@umbraco-cms/backoffice/property";
import { UMB_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/workspace";
import type { UmbCheckoutConfigurationWorkspaceContext } from "../configuration-workspace.context.js";
import type { UmbCheckoutConfigurationModel } from "../../types.js";

type Field = { alias: keyof UmbCheckoutConfigurationModel; editor: string; config?: Array<{ alias: string; value: unknown }>; licensedOnly?: boolean };

const FIELDS: Array<Field> = [
  { alias: "currencyCode", editor: "Umb.PropertyEditorUi.TextBox", config: [{ alias: "maxChars", value: 3 }] },
  { alias: "successPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
  { alias: "cancelPageKey", editor: "Umb.PropertyEditorUi.DocumentPicker", config: [{ alias: "validationLimit", value: { min: 0, max: 1 } }] },
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

@customElement("umbcheckout-configuration-workspace-view")
export class UmbCheckoutConfigurationWorkspaceViewElement extends UmbLitElement {
  #context?: UmbCheckoutConfigurationWorkspaceContext;
  @state() private _model?: UmbCheckoutConfigurationModel;

  constructor() {
    super();
    this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
      this.#context = context as UmbCheckoutConfigurationWorkspaceContext;
      this.observe(this.#context?.model, (model) => (this._model = model), "_model");
    });
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

  override render() {
    if (!this._model) return html`<uui-loader></uui-loader>`;
    const fields = FIELDS.filter((f) => !f.licensedOnly || this._model!.isLicensed);
    return html`
      <uui-box>
        <umb-property-dataset .value=${this.#toDataset(this._model)} @change=${this.#onChange}>
          ${fields.map(
            (f) => html`<umb-property
              alias=${f.alias}
              label=${this.localize.term(`umbcheckout_${LABEL_KEYS[f.alias]}_label`)}
              description=${this.localize.term(`umbcheckout_${LABEL_KEYS[f.alias]}_description`)}
              property-editor-ui-alias=${f.editor}
              .config=${f.config ?? []}></umb-property>`,
          )}
        </umb-property-dataset>
      </uui-box>
      ${this._model.isLicensed ? "" : html`<p class="unlicensed">${this.localize.term("umbcheckout_disabled_unlicensed")}</p>`}
    `;
  }

  static override styles = [css`:host { display: block; padding: var(--uui-size-layout-1); } .unlicensed { color: var(--uui-color-danger); }`];
}

export default UmbCheckoutConfigurationWorkspaceViewElement;

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-configuration-workspace-view": UmbCheckoutConfigurationWorkspaceViewElement;
  }
}
