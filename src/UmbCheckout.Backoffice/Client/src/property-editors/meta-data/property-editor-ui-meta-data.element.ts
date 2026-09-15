import { css, customElement, html, property, repeat } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbPropertyValueChangeEvent } from "@umbraco-cms/backoffice/property-editor";
import type { UmbPropertyEditorUiElement } from "@umbraco-cms/backoffice/property-editor";
import type { UUIInputElement } from "@umbraco-cms/backoffice/external/uui";

export interface UmbCheckoutMetaDataItem {
  name: string;
  value: string;
}

/**
 * Property editor for a list of meta data name / value pairs, stored as [{ "name": "", "value": "" }]
 */
@customElement("umbcheckout-property-editor-ui-meta-data")
export class UmbCheckoutPropertyEditorUiMetaDataElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  @property({ type: Array })
  value?: Array<UmbCheckoutMetaDataItem>;

  @property({ type: Boolean, reflect: true })
  readonly = false;

  get #items(): Array<UmbCheckoutMetaDataItem> {
    return Array.isArray(this.value) ? this.value : [];
  }

  #setItems(items: Array<UmbCheckoutMetaDataItem>) {
    this.value = items;
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  #onAdd() {
    this.#setItems([...this.#items, { name: "", value: "" }]);
  }

  #onRemove(index: number) {
    this.#setItems(this.#items.filter((_, i) => i !== index));
  }

  #onInput(index: number, key: keyof UmbCheckoutMetaDataItem, event: Event) {
    event.stopPropagation();
    const inputValue = (event.target as UUIInputElement).value as string;
    this.#setItems(this.#items.map((item, i) => (i === index ? { ...item, [key]: inputValue } : item)));
  }

  override render() {
    return html`
      ${repeat(
        this.#items,
        (_, index) => index,
        (item, index) => html`
          <div class="item">
            <uui-input
              label=${this.localize.term("umbcheckout_meta_data_name")}
              placeholder=${this.localize.term("umbcheckout_meta_data_name")}
              .value=${item.name ?? ""}
              ?readonly=${this.readonly}
              @input=${(e: Event) => this.#onInput(index, "name", e)}></uui-input>
            <uui-input
              label=${this.localize.term("umbcheckout_meta_data_value")}
              placeholder=${this.localize.term("umbcheckout_meta_data_value")}
              .value=${item.value ?? ""}
              ?readonly=${this.readonly}
              @input=${(e: Event) => this.#onInput(index, "value", e)}></uui-input>
            ${this.readonly
              ? ""
              : html`<uui-button
                  compact
                  label=${this.localize.term("umbcheckout_meta_data_remove")}
                  @click=${() => this.#onRemove(index)}>
                  <uui-icon name="icon-trash"></uui-icon>
                </uui-button>`}
          </div>
        `,
      )}
      ${this.readonly
        ? ""
        : html`<uui-button
            id="add"
            look="placeholder"
            label=${this.localize.term("general_add")}
            @click=${this.#onAdd}></uui-button>`}
    `;
  }

  static override styles = [
    css`
      :host {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-2);
      }

      .item {
        display: flex;
        gap: var(--uui-size-space-2);
        align-items: center;
      }

      .item uui-input {
        flex: 1 1 auto;
      }

      #add {
        width: 100%;
      }
    `,
  ];
}

export default UmbCheckoutPropertyEditorUiMetaDataElement;

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-property-editor-ui-meta-data": UmbCheckoutPropertyEditorUiMetaDataElement;
  }
}
