import { LitElement, html, css, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

interface MetaDataItem {
  name: string;
  value: string;
}

@customElement('umbcheckout-metadata-editor')
export class UmbCheckoutMetaDataEditor extends LitElement {
  static styles = css`
    :host {
      display: block;
    }
    .item-row {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      padding: var(--uui-size-space-3) 0;
      border-bottom: 1px solid var(--uui-color-border);
    }
    .drag-handle {
      cursor: move;
      opacity: 0.5;
    }
    .drag-handle:hover {
      opacity: 1;
    }
    .inputs {
      display: flex;
      flex: 1;
      gap: var(--uui-size-space-3);
      flex-wrap: wrap;
    }
    .inputs uui-input {
      flex: 1;
      min-width: 150px;
    }
    .add-button {
      margin-top: var(--uui-size-space-3);
    }
    .confirm-prompt {
      display: flex;
      gap: var(--uui-size-space-2);
      align-items: center;
    }
    .dragging {
      opacity: 0.5;
    }
  `;

  @property({ type: Array }) value: MetaDataItem[] = [];
  @property({ type: Boolean }) readonly = false;

  @state() private _confirmIndex: number | null = null;
  @state() private _dragIndex: number | null = null;

  private _dispatchChange() {
    this.dispatchEvent(
      new CustomEvent('change', {
        detail: { value: this.value.filter((item) => item.name || item.value) },
        bubbles: true,
        composed: true,
      })
    );
  }

  private _addItem() {
    this.value = [...this.value, { name: '', value: '' }];
    this._dispatchChange();
  }

  private _removeItem(index: number) {
    this.value = this.value.filter((_, i) => i !== index);
    this._confirmIndex = null;
    this._dispatchChange();
  }

  private _updateItem(index: number, field: 'name' | 'value', val: string) {
    this.value = this.value.map((item, i) =>
      i === index ? { ...item, [field]: val } : item
    );
    this._dispatchChange();
  }

  private _onDragStart(e: DragEvent, index: number) {
    this._dragIndex = index;
    if (e.dataTransfer) {
      e.dataTransfer.effectAllowed = 'move';
    }
  }

  private _onDragOver(e: DragEvent, index: number) {
    e.preventDefault();
    if (this._dragIndex === null || this._dragIndex === index) return;
    const items = [...this.value];
    const [moved] = items.splice(this._dragIndex, 1);
    items.splice(index, 0, moved);
    this.value = items;
    this._dragIndex = index;
  }

  private _onDragEnd() {
    this._dragIndex = null;
    this._dispatchChange();
  }

  render() {
    return html`
      ${this.value.map(
        (item, index) => html`
          <div
            class="item-row ${this._dragIndex === index ? 'dragging' : ''}"
            draggable=${this.readonly ? 'false' : 'true'}
            @dragstart=${(e: DragEvent) => this._onDragStart(e, index)}
            @dragover=${(e: DragEvent) => this._onDragOver(e, index)}
            @dragend=${this._onDragEnd}
          >
            ${!this.readonly
              ? html`<uui-icon name="icon-navigation" class="drag-handle"></uui-icon>`
              : nothing}
            <div class="inputs">
              <uui-input
                placeholder="Name"
                .value=${item.name}
                ?readonly=${this.readonly}
                @change=${(e: Event) =>
                  this._updateItem(index, 'name', (e.target as HTMLInputElement).value)}
              ></uui-input>
              <uui-input
                placeholder="Value"
                .value=${item.value}
                ?readonly=${this.readonly}
                @change=${(e: Event) =>
                  this._updateItem(index, 'value', (e.target as HTMLInputElement).value)}
              ></uui-input>
            </div>
            ${!this.readonly
              ? this._confirmIndex === index
                ? html`
                    <div class="confirm-prompt">
                      <uui-button
                        look="primary"
                        color="danger"
                        compact
                        label="Confirm"
                        @click=${() => this._removeItem(index)}
                      >
                        Confirm
                      </uui-button>
                      <uui-button
                        compact
                        label="Cancel"
                        @click=${() => (this._confirmIndex = null)}
                      >
                        Cancel
                      </uui-button>
                    </div>
                  `
                : html`
                    <uui-button
                      look="default"
                      color="danger"
                      compact
                      label="Remove"
                      @click=${() => (this._confirmIndex = index)}
                    >
                      <uui-icon name="icon-trash"></uui-icon>
                    </uui-button>
                  `
              : nothing}
          </div>
        `
      )}
      ${!this.readonly
        ? html`
            <uui-button
              class="add-button"
              look="placeholder"
              label="Add"
              @click=${this._addItem}
            >
              Add
            </uui-button>
          `
        : nothing}
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umbcheckout-metadata-editor': UmbCheckoutMetaDataEditor;
  }
}
