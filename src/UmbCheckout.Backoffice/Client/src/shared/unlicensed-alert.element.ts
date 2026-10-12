import { customElement, html, nothing, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UMBCHECKOUT_LICENSE_CONTEXT } from "../license/license.context-token.js";
import { UmbCheckoutAlertStyles } from "./alert.styles.js";

/** v13's red "running in unlicensed mode" alert. Renders nothing until the licence is known to be invalid. */
@customElement("umbcheckout-unlicensed-alert")
export class UmbCheckoutUnlicensedAlertElement extends UmbLitElement {
  @state() private _unlicensed = false;

  constructor() {
    super();
    this.consumeContext(UMBCHECKOUT_LICENSE_CONTEXT, (context) => {
      this.observe(context?.status, (status) => (this._unlicensed = status !== undefined && !status.isValid), "_status");
    });
  }

  override render() {
    if (!this._unlicensed) return nothing;
    return html`<div class="umbcheckout-alert" role="alert">
      ${this.localize.term("umbcheckout_unlicensed_warning_before")}
      <a href="https://umbcheckout.net" target="_blank" rel="noopener">${this.localize.term("umbcheckout_purchase_license")}</a>
      ${this.localize.term("umbcheckout_unlicensed_warning_after")}
    </div>`;
  }

  static override styles = [UmbCheckoutAlertStyles];
}

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-unlicensed-alert": UmbCheckoutUnlicensedAlertElement;
  }
}
