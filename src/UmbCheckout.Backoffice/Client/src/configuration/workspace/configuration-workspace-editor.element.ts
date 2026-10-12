import { customElement, html } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";

@customElement("umbcheckout-configuration-workspace-editor")
export class UmbCheckoutConfigurationWorkspaceEditorElement extends UmbLitElement {
  override render() {
    return html`<umb-workspace-editor headline=${this.localize.term("umbcheckout_configuration")}></umb-workspace-editor>`;
  }
}

export default UmbCheckoutConfigurationWorkspaceEditorElement;

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-configuration-workspace-editor": UmbCheckoutConfigurationWorkspaceEditorElement;
  }
}
