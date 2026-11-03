import { css, customElement, html, nothing, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UMBCHECKOUT_LICENSE_CONTEXT } from "../license/license.context-token.js";

/** v13's "Made with ♥ by UmbHost Limited" footer link, shown only when unlicensed. */
@customElement("umbcheckout-made-by")
export class UmbCheckoutMadeByElement extends UmbLitElement {
  @state() private _unlicensed = false;

  constructor() {
    super();
    this.consumeContext(UMBCHECKOUT_LICENSE_CONTEXT, (context) => {
      this.observe(context?.status, (status) => (this._unlicensed = status !== undefined && !status.isValid), "_status");
    });
  }

  override render() {
    if (!this._unlicensed) return nothing;
    return html`<uui-button look="outline" href="https://umbhost.net" target="_blank" rel="noopener">
      ${this.localize.term("umbcheckout_made_with")}
      <uui-icon class="heart" name="icon-hearts"></uui-icon>
      ${this.localize.term("umbcheckout_by_umbhost")}
    </uui-button>`;
  }

  static override styles = [css`.heart { color: var(--uui-palette-maroon-flush, #d42054); margin: 0 var(--uui-size-space-1); }`];
}

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-made-by": UmbCheckoutMadeByElement;
  }
}
