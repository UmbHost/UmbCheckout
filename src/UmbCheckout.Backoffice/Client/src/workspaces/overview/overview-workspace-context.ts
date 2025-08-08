import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext, UmbNotificationDefaultData } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { ConfigurationService, MultiUrlPicker, UpdateConfigurationData } from '../../api/backoffice';
import { UmbServerModelValidatorContext, UmbValidationContext } from '@umbraco-cms/backoffice/validation';

export class UmbCheckoutOverviewWorkspaceContextElement extends UmbContextBase {
    #validation = new UmbValidationContext(this);
    #serverValidation = new UmbServerModelValidatorContext(this);
    private _notificationContext?: UmbNotificationContext;
    propertyEditorsData: UmbPropertyValueData[];
    #localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host, UMBCHECKOUT_OVERVIEW_WORKSPACE_CONTEXT);

        this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
                    this._notificationContext = notificationContext;
                });
	}

    async save() : Promise<void> {
        await this.#serverValidation.validate().then(async (response) => {
            console.log('Server validation passed', response);
            console.log('Server validation passed');
        }, () => {
            console.log('Server validation failed');
            return Promise.reject();
        });
         await this.#validation.validate().then(async () => {
            const updateData: UpdateConfigurationData = {
            url: '/umbraco/umbcheckout/v2.0/configuration-api/update-configuration',
            body: {
                basketInCookieExpiry: this.propertyEditorsData.find((pe) => pe.alias === 'basketInCookieExpiry')?.value as number,
                basketInDatabaseExpiry: this.propertyEditorsData.find((pe) => pe.alias === 'basketInDatabaseExpiry')?.value as number,
                cancelPageUrl: this.propertyEditorsData.find((pe) => pe.alias === 'cancelPageUrl')?.value as MultiUrlPicker[],
                successPageUrl: this.propertyEditorsData.find((pe) => pe.alias === 'successPageUrl')?.value as MultiUrlPicker[],
                currencyCode: this.propertyEditorsData.find((pe) => pe.alias === 'currencyCode')?.value as string,
                enableShipping: this.propertyEditorsData.find((pe) => pe.alias === 'enableShipping')?.value as boolean,
                storeBasketInCookie: this.propertyEditorsData.find((pe) => pe.alias === 'storeBasketInCookie')?.value as boolean,
                storeBasketInDatabase: this.propertyEditorsData.find((pe) => pe.alias === 'storeBasketInDatabase')?.value as boolean,
            }
        }
        await ConfigurationService.updateConfiguration(updateData).then(() => {
            const data: UmbNotificationDefaultData = { headline: this.#localize.term("umbcheckout_configuration_saved_title"), message: this.#localize.term("umbcheckout_configuration_saved_message") };
            this._notificationContext?.peek('positive', { data });
        }).catch((error) => {
            const data: UmbNotificationDefaultData = { headline: this.#localize.term("umbcheckout_configuration_failed_save_title"), message: this.#localize.term("umbcheckout_configuration_failed_save_message") };
            this._notificationContext?.peek('danger', { data: data });
        });
        }, () => {
            const data: UmbNotificationDefaultData = { headline: this.#localize.term("umbcheckout_configuration_failed_save_title"), message: this.#localize.term("umbcheckout_configuration_failed_save_message") };
            this._notificationContext?.peek('danger', { data: data });
            return Promise.reject();
        });
    }
}

export const api = UmbCheckoutOverviewWorkspaceContextElement;

export const UMBCHECKOUT_OVERVIEW_WORKSPACE_CONTEXT = new UmbContextToken<UmbCheckoutOverviewWorkspaceContextElement>(
	'UmbWorkspaceContext',
	'umbcheckout-overview-workspace-context',
);