import { customElement, html } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import "../../shared/made-by.element.js";

/** v13 header "UmbCheckout"; the footer shows the "Made by UmbHost" link when unlicensed. */
@customElement("umbcheckout-configuration-workspace-editor")
export class UmbCheckoutConfigurationWorkspaceEditorElement extends UmbLitElement {
  override render() {
    return html`<umb-workspace-editor headline=${this.localize.term("umbcheckout_section")}>
      <umbcheckout-made-by slot="footer-info"></umbcheckout-made-by>
    </umb-workspace-editor>`;
  }
}

export default UmbCheckoutConfigurationWorkspaceEditorElement;

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-configuration-workspace-editor": UmbCheckoutConfigurationWorkspaceEditorElement;
  }
}
