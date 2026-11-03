import { customElement, html, nothing, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UMBCHECKOUT_LICENSE_CONTEXT } from "../license/license.context-token.js";
import { UmbCheckoutAlertStyles } from "./alert.styles.js";

/**
 * The "running in unlicensed mode" message. Renders nothing until the licence is known to be invalid. With a paid add-on installed it
 * is v13's red alert; without one nothing needs a licence, so it is an info bar asking for support.
 */
@customElement("umbcheckout-unlicensed-alert")
export class UmbCheckoutUnlicensedAlertElement extends UmbLitElement {
  @state() private _unlicensed = false;
  @state() private _addOnsInstalled = false;

  constructor() {
    super();
    this.consumeContext(UMBCHECKOUT_LICENSE_CONTEXT, (context) => {
      this.observe(
        context?.status,
        (status) => {
          this._unlicensed = status !== undefined && !status.isValid;
          this._addOnsInstalled = status?.addOnsInstalled === true;
        },
        "_status",
      );
    });
  }

  override render() {
    if (!this._unlicensed) return nothing;
    return html`<div class="umbcheckout-alert ${this._addOnsInstalled ? "" : "umbcheckout-alert--info"}" role=${this._addOnsInstalled ? "alert" : "status"}>
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
