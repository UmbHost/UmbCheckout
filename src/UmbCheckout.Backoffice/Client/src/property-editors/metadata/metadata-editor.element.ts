import { css, customElement, html, property, repeat } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbChangeEvent } from "@umbraco-cms/backoffice/event";
import type { UmbPropertyEditorUiElement } from "@umbraco-cms/backoffice/property-editor";

export type UmbCheckoutMetaDataItem = { name: string; value: string };

@customElement("umbcheckout-metadata-editor")
export class UmbCheckoutMetaDataEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  @property({ attribute: false })
  value?: Array<UmbCheckoutMetaDataItem>;

  @property({ type: Boolean, reflect: true })
  readonly = false;

  get #items(): Array<UmbCheckoutMetaDataItem> {
    // Values migrated from older versions may arrive as a JSON string.
    if (typeof this.value === "string") {
      try { return JSON.parse(this.value) as Array<UmbCheckoutMetaDataItem>; } catch { return []; }
    }
    return this.value ?? [];
  }

  #set(items: Array<UmbCheckoutMetaDataItem>) {
    this.value = items;
    this.dispatchEvent(new UmbChangeEvent());
  }

  #update(index: number, key: keyof UmbCheckoutMetaDataItem, text: string) {
    this.#set(this.#items.map((item, i) => (i === index ? { ...item, [key]: text } : item)));
  }

  override render() {
    const items = this.#items;
    return html`
      ${repeat(
        items,
        (_item, index) => index,
        (item, index) => html`<div class="row">
          <uui-input label=${this.localize.term("umbcheckout_metadata_name")} .value=${item.name} ?readonly=${this.readonly}
            @change=${(e: Event) => this.#update(index, "name", (e.target as HTMLInputElement).value)}></uui-input>
          <uui-input label=${this.localize.term("umbcheckout_metadata_value")} .value=${item.value} ?readonly=${this.readonly}
            @change=${(e: Event) => this.#update(index, "value", (e.target as HTMLInputElement).value)}></uui-input>
          ${this.readonly ? "" : html`<uui-button compact label=${this.localize.term("general_remove")} @click=${() => this.#set(items.filter((_x, i) => i !== index))}><uui-icon name="icon-trash"></uui-icon></uui-button>`}
        </div>`,
      )}
      ${this.readonly ? "" : html`<uui-button look="placeholder" label=${this.localize.term("general_add")} @click=${() => this.#set([...items, { name: "", value: "" }])}></uui-button>`}
    `;
  }

  static override styles = [css`.row { display: grid; grid-template-columns: 1fr 1fr auto; gap: var(--uui-size-space-3); margin-bottom: var(--uui-size-space-3); } uui-button[look="placeholder"] { width: 100%; }`];
}

export default UmbCheckoutMetaDataEditorElement;

declare global {
  interface HTMLElementTagNameMap { "umbcheckout-metadata-editor": UmbCheckoutMetaDataEditorElement; }
}
