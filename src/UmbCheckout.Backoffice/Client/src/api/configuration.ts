import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";
import type { UmbLinkPickerLink } from "@umbraco-cms/backoffice/multi-url-picker";

export const UMB_CHECKOUT_API_BASE_PATH = "/umbraco/umbcheckout/api/v1";

export interface UmbCheckoutConfigurationModel {
  successPageUrl: Array<UmbLinkPickerLink>;
  cancelPageUrl: Array<UmbLinkPickerLink>;
  currencyCode: string;
  enableShipping: boolean;
  storeBasketInCookie: boolean;
  basketInCookieExpiry: number;
  storeBasketInDatabase: boolean;
  basketInDatabaseExpiry: number;
}

export interface UmbCheckoutConfigurationResponseModel extends UmbCheckoutConfigurationModel {
  isLicensed: boolean;
  hasUmbracoApplicationUrlSet: boolean;
}

const security = [{ scheme: "bearer", type: "http" }] as const;

export class UmbCheckoutConfigurationService {
  static getConfiguration() {
    return umbHttpClient.get<{ 200: UmbCheckoutConfigurationResponseModel }>({
      security: [...security],
      url: `${UMB_CHECKOUT_API_BASE_PATH}/configuration`,
    });
  }

  static updateConfiguration(model: UmbCheckoutConfigurationModel) {
    return umbHttpClient.put<{ 200: UmbCheckoutConfigurationResponseModel }>({
      security: [...security],
      url: `${UMB_CHECKOUT_API_BASE_PATH}/configuration`,
      body: model,
      headers: {
        "Content-Type": "application/json",
      },
    });
  }
}
