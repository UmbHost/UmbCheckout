import { LitElement, html, customElement, css, state, when, repeat, nothing } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCheckoutTextStyles } from "../style/text-style.style";
import { LicenceStatusResponseReadable, LicencingService } from "../api/licencing";
import { ConfigurationService } from "../api/backoffice";
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext, UmbNotificationDefaultData } from "@umbraco-cms/backoffice/notification";
import { UUIButtonState } from "@umbraco-ui/uui-button";
import { UmbLocalizationController } from "@umbraco-cms/backoffice/localization-api";

@customElement('umbcheckout-overview')
export default class UmbHostCloudflarePurgeOverviewViewElement extends UmbElementMixin(LitElement) {

    private _notificationContext?: UmbNotificationContext;
    #localize = new UmbLocalizationController(this);
    
    @state()
    private licenceStatus?: LicenceStatusResponseReadable;

    @state()
    private hasUmbracoApplicationUrlSet: boolean = false;

    @state()
    private recheckLicenceButtonState?: UUIButtonState;

    @state()
    private checkingLicence: boolean = true;

    connectedCallback() {
        super.connectedCallback();

        this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
            this._notificationContext = notificationContext;
        });
        this.#getLicenceStatus();
        this.#checkUmbracoApplicationUrl();
    }
    async #getLicenceStatus() {
        await LicencingService.getLicenceStatus().then((response) => {
            this.licenceStatus = response.data;
            this.checkingLicence = false;
        });
    }

    async #checkUmbracoApplicationUrl() {
        const hasUmbracoApplicationUrlSet = await ConfigurationService.hasUmbracoApplicationUrlSet();
        this.hasUmbracoApplicationUrlSet = hasUmbracoApplicationUrlSet.data;
    }

    async recheckLicence() {
        this.recheckLicenceButtonState = 'waiting';
        await LicencingService.checkLicence().then((response) => {
            this.checkingLicence = true;
            if (response.data.accepted === "Accepted") {
                const data: UmbNotificationDefaultData = { headline: this.localize.term("umbcheckout_license_check_requested_title"), message: this.localize.term("umbcheckout_license_check_requested_message") };
                this._notificationContext?.peek('positive', { data });
                this.#getLicenceStatus();
                

                setTimeout( () => {
                    this.checkingLicence = false;
                    this.recheckLicenceButtonState = 'success';
                }, 5000);
            }

            if (response.data.accepted === "Wait") {
                const data: UmbNotificationDefaultData = { headline: this.localize.term("umbcheckout_license_check_request_wait_title"), message: this.#localize.string("#umbcheckout_license_check_request_wait_message", response.data.timeLeft) };
                this._notificationContext?.peek('warning', { data });
                this.recheckLicenceButtonState = 'failed';
                this.checkingLicence = false;
            }

        }).catch((error) => {
            const data: UmbNotificationDefaultData = { headline: this.localize.term("umbcheckout_license_check_error_title"), message: this.localize.term("umbcheckout_license_check_error_message") };
            this._notificationContext?.peek('danger', { data });
            this.recheckLicenceButtonState = 'failed';
            this.checkingLicence = false;
        });
    }

    render() {
        return html`   
        <section id="umbcheckout-workspace">
            <div class="dashboard-overview">
                <div class="left-col">
                    <uui-box class="introduction" headline=${this.localize.term("umbcheckout_workspace_overview_title")}>
                        <umb-localize key="umbcheckout_workspace_overview_introduction"></umb-localize>

                        ${when(this.licenceStatus?.status === "Active" || this.licenceStatus?.status === "Expired" || this.licenceStatus?.status === "Invalid", () => html`
                            <p class="red bold"><strong><umb-localize class="red bold underline" key="umbcheckout_unlicenced_warning"></umb-localize></strong></p>
                        `)}
                    </uui-box>
                </div>
                <div class="right-col">
                    <uui-box id="licence-details" headline=${this.localize.term("umbcheckout_licence_details")}>
                    ${this.checkingLicence ? html `<uui-loader></uui-loader>` : html`    
                        <div class="licence-detail-container">
                            ${when(!this.hasUmbracoApplicationUrlSet, () => html`<p class="red bold"><strong><umb-localize key="umbcheckout_umbraco_application_url_unset"></umb-localize></strong></p>`)}
                            ${when(this.licenceStatus?.isDevelopmentLicense, () => html`<p><strong>${this.localize.term("umbcheckout_license_development")}</strong>: ${this.localize.term("umbcheckout_license_development_true")}</p>`)}
                            <p><strong>${this.localize.term("umbcheckout_licence_status")}</strong>: ${this.licenceStatus?.status}</p>
                            <p><strong>${this.localize.term("umbcheckout_licence_registration_date")}</strong>: ${this.licenceStatus?.regDate}</p>
                            <p><strong>${this.localize.term("umbcheckout_licence_expiry_date")}</strong>: ${when(this.licenceStatus?.expiryDateTime !== "0000-00-00", () => html `${this.licenceStatus?.expiryDateTime}`)}</p>
                            <p><strong>${this.localize.term("umbcheckout_licence_valid_domains")}</strong>: ${this.licenceStatus?.validDomains}</p>
                            <p><strong>${this.localize.term("umbcheckout_licence_valid_paths")}</strong>: ${this.licenceStatus?.validPaths}</p>
                        </div>
                        <uui-button look="primary" ?disabled=${!this.hasUmbracoApplicationUrlSet} .state=${this.recheckLicenceButtonState} @click="${this.recheckLicence}">
                            ${this.localize.term("umbcheckout_licence_recheck")}
                        </uui-button>
                        `}
                    </uui-box>

                    ${this.checkingLicence ? nothing : html` 
                        ${repeat(this.licenceStatus?.licenceAddons ?? [], (addon) => html`
                        <uui-box id="licence-details-addons" headline=${this.localize.term("umbcheckout_license_addon")}>
                            <div class="licence-detail-container">
                                <p><strong>${this.localize.term("umbcheckout_license_addon_name")}</strong>: ${addon.name}</p>
                                <p><strong>${this.localize.term("umbcheckout_licence_status")}</strong>: ${addon.status}</p>
                                <p><strong>${this.localize.term("umbcheckout_licence_expiry_date")}</strong>: ${when(addon.nextDueDate !== "0000-00-00", () => html `${addon.nextDueDate}`)}</p>
                            </div>
                        </uui-box>
                        `)}
                    `}
                </div>
            </div>
        </section>
    `
    }

    static styles = [
        UmbTextStyles,
        UmbCheckoutTextStyles,
        css`
            #umbcheckout-workspace {
				padding: var(--uui-size-layout-1);
			}
            #licence-warning {
                margin-top: var(--uui-size-layout-1);
            }
            uui-box {
				p:first-child {
					margin-top: 0;
				}
			}
            uui-box:not(:last-of-type) {
				margin-bottom: var(--uui-size-layout-1);
			}

            .dashboard-overview {
                /*top: 0px;*/
                flex-direction: row;
                display: flex;
            }

            #licence-details, #licence-details-addons {
                display: flex;
                flex-direction: column;
                gap: var(--uui-size-space-3);
            }

            .licence-detail-container {
                display: block;
            }

            .left-col {
                flex: 1 1 auto;
                margin-right: 20px;
                width: calc(100% - 370px)
            }

            .right-col {
                flex: 0 0 350px;
                margin-top: 0;
                align-self: start
            }
  `];
}

declare global {
    interface HTMLElementTagNameMap {
        'umbcheckout-overview': UmbHostCloudflarePurgeOverviewViewElement
    }
}