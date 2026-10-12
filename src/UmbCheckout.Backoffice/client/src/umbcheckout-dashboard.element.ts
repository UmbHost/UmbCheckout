import { LitElement, html, css, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import {
  getLicenseStatus,
  getConfiguration,
  getHasUmbracoApplicationUrlSet,
  updateConfiguration,
  checkLicense,
  type LicenseStatus,
  type ConfigProperty,
} from './umbcheckout-resources.js';

@customElement('umbcheckout-dashboard')
export class UmbCheckoutDashboard extends LitElement {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }
    .dashboard-overview {
      display: flex;
      gap: var(--uui-size-layout-1);
    }
    .left-col {
      flex: 1 1 auto;
      width: calc(100% - 370px);
    }
    .right-col {
      flex: 0 0 350px;
      position: sticky;
      top: 0;
      align-self: flex-start;
    }
    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }
    .property-row {
      padding: var(--uui-size-space-4) 0;
      border-bottom: 1px solid var(--uui-color-border);
    }
    .property-label {
      font-weight: bold;
      margin-bottom: var(--uui-size-space-2);
    }
    .property-description {
      color: var(--uui-color-text-alt);
      font-size: 0.85em;
      margin-bottom: var(--uui-size-space-2);
    }
    .license-row {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-2) 0;
    }
    .license-label {
      font-weight: bold;
    }
    .addon-item {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-2) 0;
      border-top: 1px solid var(--uui-color-border);
    }
    .warning-box {
      background: var(--uui-color-warning);
      padding: var(--uui-size-space-4);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-4);
    }
    .footer {
      display: flex;
      justify-content: flex-end;
      padding: var(--uui-size-space-4) 0;
    }
  `;

  @state() private _properties: ConfigProperty[] = [];
  @state() private _licenseState: LicenseStatus | null = null;
  @state() private _hasUmbracoUrl = true;
  @state() private _umbracoUrlMessage = '';
  @state() private _saveState: 'init' | 'busy' | 'success' | 'error' = 'init';
  @state() private _checkLicenseState: 'init' | 'busy' | 'success' | 'error' = 'init';

  async connectedCallback() {
    super.connectedCallback();
    await this._loadData();
  }

  private async _loadData() {
    try {
      const [properties, hasUrl, license] = await Promise.all([
        getConfiguration(),
        getHasUmbracoApplicationUrlSet(),
        getLicenseStatus(),
      ]);
      this._properties = properties;
      this._hasUmbracoUrl = hasUrl;
      this._licenseState = license;
    } catch (e) {
      console.error('Failed to load dashboard data', e);
    }
  }

  private async _saveConfiguration() {
    this._saveState = 'busy';
    try {
      const values: Record<string, any> = {};
      for (const prop of this._properties) {
        values[prop.alias] = prop.value;
      }
      await updateConfiguration(values);
      this._saveState = 'success';
      setTimeout(() => (this._saveState = 'init'), 2000);
    } catch {
      this._saveState = 'error';
      setTimeout(() => (this._saveState = 'init'), 2000);
    }
  }

  private async _checkLicense() {
    this._checkLicenseState = 'busy';
    try {
      await checkLicense();
      this._checkLicenseState = 'success';
      setTimeout(() => window.location.reload(), 1500);
    } catch {
      this._checkLicenseState = 'error';
      setTimeout(() => (this._checkLicenseState = 'init'), 2000);
    }
  }

  private _onPropertyChange(alias: string, value: any) {
    this._properties = this._properties.map((p) =>
      p.alias === alias ? { ...p, value } : p
    );
  }

  private _renderProperty(prop: ConfigProperty) {
    return html`
      <div class="property-row">
        <div class="property-label">${prop.label}</div>
        <div class="property-description">${prop.description}</div>
        ${prop.view === 'boolean'
          ? html`<uui-toggle
              .checked=${prop.value === true || prop.value === 'true' || prop.value === '1'}
              @change=${(e: Event) =>
                this._onPropertyChange(prop.alias, (e.target as any).checked)}
            ></uui-toggle>`
          : prop.view === 'integer'
            ? html`<uui-input
                type="number"
                .value=${prop.value ?? ''}
                @change=${(e: Event) =>
                  this._onPropertyChange(prop.alias, (e.target as HTMLInputElement).value)}
              ></uui-input>`
            : html`<uui-input
                .value=${prop.value ?? ''}
                @change=${(e: Event) =>
                  this._onPropertyChange(prop.alias, (e.target as HTMLInputElement).value)}
              ></uui-input>`}
      </div>
    `;
  }

  render() {
    return html`
      <umb-body-layout header-transparent>
        <div slot="header">
          <h3>UmbCheckout</h3>
        </div>

        <div class="dashboard-overview">
          <div class="left-col">
            ${!this._hasUmbracoUrl
              ? html`<uui-box headline="Warning">
                  <div class="warning-box">
                    The "Umbraco Application Url" configuration setting is unset.
                    <a href="https://docs.umbraco.com/umbraco-cms/reference/configuration/webroutingsettings#umbraco-application-url" target="_blank">
                      Set the value here</a>. This is required to match your license.
                  </div>
                </uui-box>`
              : nothing}

            <uui-box headline="Help &amp; Support">
              <p>Documentation available at <a href="https://docs.umbcheckout.net/" target="_blank">https://docs.umbcheckout.net/</a></p>
              <p>Please report any issues in the bug tracker found on the <a target="_blank" href="https://github.com/UmbHost/UmbCheckout/issues">GitHub Repo</a></p>
              <p>Open a support ticket through <a target="_blank" href="https://my.umbhost.net/submitticket.php?step=2&deptid=8&subject=UmbCheckout:%20[ISSUE%20NAME%20HERE]">My.UmbHost</a></p>
            </uui-box>

            <uui-box headline="Configuration">
              ${this._properties.map((p) => this._renderProperty(p))}
            </uui-box>
          </div>

          <div class="right-col">
            ${this._licenseState
              ? html`
                  <uui-box headline="License Details">
                    ${this._licenseState.isDevelopment
                      ? html`<div class="license-row">
                          <span class="license-label">Development License</span>
                          <span>Yes</span>
                        </div>`
                      : nothing}
                    <div class="license-row">
                      <span class="license-label">License Status</span>
                      <span>${this._licenseState.status}</span>
                    </div>
                    <div class="license-row">
                      <span class="license-label">Registration Date</span>
                      <span>${this._licenseState.registrationDate}</span>
                    </div>
                    ${this._licenseState.expiryDate
                      ? html`<div class="license-row">
                          <span class="license-label">Expiry Date</span>
                          <span>${this._licenseState.expiryDate}</span>
                        </div>`
                      : nothing}
                    <div class="license-row">
                      <span class="license-label">Valid Domains</span>
                      <span>${this._licenseState.validDomains?.join(', ')}</span>
                    </div>
                    <div class="license-row">
                      <span class="license-label">Valid Paths</span>
                      <span>${this._licenseState.validPaths?.join(', ')}</span>
                    </div>
                    ${this._licenseState.addons?.map(
                      (addon) => html`
                        <div class="addon-item">
                          <span>${addon.name}</span>
                          <span>${addon.status}</span>
                        </div>
                      `
                    )}
                    <uui-button
                      look="secondary"
                      label="Re-Check License"
                      .state=${this._checkLicenseState === 'busy' ? 'waiting' : undefined}
                      @click=${this._checkLicense}
                    >
                      Re-Check License
                    </uui-button>
                  </uui-box>
                `
              : nothing}
          </div>
        </div>

        <div slot="footer">
          <div class="footer">
            <uui-button
              look="primary"
              color="positive"
              label="Save"
              .state=${this._saveState === 'busy' ? 'waiting' : this._saveState === 'success' ? 'success' : this._saveState === 'error' ? 'failed' : undefined}
              @click=${this._saveConfiguration}
            >
              Save
            </uui-button>
          </div>
        </div>
      </umb-body-layout>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umbcheckout-dashboard': UmbCheckoutDashboard;
  }
}
