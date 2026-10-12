import { css, customElement, html, nothing, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { ConfigurationService } from "../api/sdk.gen.js";
import { client } from "../api/client.gen.js";
import { whenUmbCheckoutClientReady } from "../core/api-client.js";
import { UMBCHECKOUT_LICENSE_CONTEXT } from "../license/license.context-token.js";
import type { UmbCheckoutLicenseStatus } from "../license/license.context.js";

@customElement("umbcheckout-status-dashboard")
export class UmbCheckoutStatusDashboardElement extends UmbLitElement {
  @state() private _license?: UmbCheckoutLicenseStatus;
  @state() private _applicationUrlSet = true;

  constructor() {
    super();
    this.consumeContext(UMBCHECKOUT_LICENSE_CONTEXT, (context) => {
      this.observe(context?.status, (status) => (this._license = status), "_license");
    });
    this.#loadApplicationUrl();
  }

  async #loadApplicationUrl() {
    await whenUmbCheckoutClientReady(client);
    const { data } = await tryExecute(this, ConfigurationService.getApplicationUrlStatus());
    this._applicationUrlSet = data?.isSet ?? true;
  }

  override render() {
    return html`
      ${this._applicationUrlSet
        ? nothing
        : html`<uui-box><p class="warning">${this.localize.term("umbcheckout_application_url_missing")}</p></uui-box>`}
      <uui-box headline=${this.localize.term("umbcheckout_license_status")}>
        ${this._license
          ? html`<p>
                <uui-tag color=${this._license.isValid ? "positive" : "danger"}>${this._license.status}</uui-tag>
                ${this._license.isDevelopmentLicense
                  ? html`<uui-tag color="warning">${this.localize.term("umbcheckout_license_development")}</uui-tag>`
                  : nothing}
              </p>
              ${this._license.isValid ? nothing : html`<p>${this.localize.term("umbcheckout_disabled_unlicensed")}</p>`}`
          : html`<uui-loader></uui-loader>`}
      </uui-box>
    `;
  }

  static override styles = [
    css`
      :host { display: grid; gap: var(--uui-size-layout-1); padding: var(--uui-size-layout-1); }
      .warning { color: var(--uui-color-warning-standalone); }
    `,
  ];
}

export default UmbCheckoutStatusDashboardElement;

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-status-dashboard": UmbCheckoutStatusDashboardElement;
  }
}
