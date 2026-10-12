import type { UmbEntityModel } from "@umbraco-cms/backoffice/entity";

export interface UmbCheckoutConfigurationModel extends UmbEntityModel {
  currencyCode: string;
  successPageKey: string | null;
  cancelPageKey: string | null;
  enableShipping: boolean;
  storeBasketInCookie: boolean;
  basketInCookieExpiry: number;
  storeBasketInDatabase: boolean;
  basketInDatabaseExpiry: number;
  isLicensed: boolean;
}
