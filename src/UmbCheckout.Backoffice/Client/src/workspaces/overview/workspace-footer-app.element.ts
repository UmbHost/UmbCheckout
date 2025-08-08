import { customElement, html, LitElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umbcheckout-workspace-footer-app')
export class UmbCheckoutWorkspaceFooterAppElement extends UmbElementMixin(LitElement) {
	#localize = new UmbLocalizationController(this);
	
	constructor() {
		super();
	}

	override render() {
		return html`
		<span>${this.#localize.term("umbcheckout_made_with")} <umb-icon name="icon-hearts" color="color-red"></umb-icon> ${this.#localize.term("umbcheckout_made_by")} <a href="https://umbcheckout.net" target="_blank">${this.#localize.term("umbcheckout_company_name")}</a></span>`;
	}

    static styles = [
		UmbTextStyles,
        css`
        span {
            display: flex;
            margin-left: var(--uui-size-layout-1);
            }
            umb-icon, a {
                margin: 0 4px;
            }`
    ]
}

export default UmbCheckoutWorkspaceFooterAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umbcheckout-workspace-footer-app': UmbCheckoutWorkspaceFooterAppElement;
	}
}