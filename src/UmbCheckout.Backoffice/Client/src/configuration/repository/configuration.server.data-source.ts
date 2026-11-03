import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import type { UmbDetailDataSource } from "@umbraco-cms/backoffice/repository";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { ConfigurationService } from "../../api/sdk.gen.js";
import { UMBCHECKOUT_CONFIGURATION_ENTITY_TYPE, UMBCHECKOUT_CONFIGURATION_UNIQUE } from "../entity.js";
import type { UmbCheckoutConfigurationModel } from "../types.js";

/** Singleton detail data source: read and update only. */
export class UmbCheckoutConfigurationServerDataSource implements UmbDetailDataSource<UmbCheckoutConfigurationModel> {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async createScaffold() {
    return this.read();
  }

  async read() {
    const { data, error } = await tryExecute(this.#host, ConfigurationService.getConfiguration());
    if (error || !data) return { error };

    const model: UmbCheckoutConfigurationModel = {
      entityType: UMBCHECKOUT_CONFIGURATION_ENTITY_TYPE,
      unique: UMBCHECKOUT_CONFIGURATION_UNIQUE,
      currencyCode: data.currencyCode,
      successPageKey: data.successPageKey ?? null,
      cancelPageKey: data.cancelPageKey ?? null,
      enableShipping: data.enableShipping,
      storeBasketInCookie: data.storeBasketInCookie,
      basketInCookieExpiry: data.basketInCookieExpiry,
      storeBasketInDatabase: data.storeBasketInDatabase,
      basketInDatabaseExpiry: data.basketInDatabaseExpiry,
      isLicensed: data.isLicensed,
    };
    return { data: model };
  }

  async create(model: UmbCheckoutConfigurationModel) {
    return this.update(model);
  }

  async update(model: UmbCheckoutConfigurationModel) {
    const { error } = await tryExecute(
      this.#host,
      ConfigurationService.updateConfiguration({
        body: {
          currencyCode: model.currencyCode,
          successPageKey: model.successPageKey,
          cancelPageKey: model.cancelPageKey,
          enableShipping: model.enableShipping,
          storeBasketInCookie: model.storeBasketInCookie,
          basketInCookieExpiry: model.basketInCookieExpiry,
          storeBasketInDatabase: model.storeBasketInDatabase,
          basketInDatabaseExpiry: model.basketInDatabaseExpiry,
        },
      }),
    );
    if (error) return { error };
    return this.read();
  }

  async delete() {
    return { error: new Error("The configuration cannot be deleted.") };
  }
}
