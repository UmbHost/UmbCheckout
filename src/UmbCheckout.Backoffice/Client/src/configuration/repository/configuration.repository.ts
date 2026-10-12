import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbLocalizationController } from "@umbraco-cms/backoffice/localization-api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { UmbDetailRepositoryBase } from "@umbraco-cms/backoffice/repository";
import type { UmbCheckoutConfigurationModel } from "../types.js";
import { UmbCheckoutConfigurationServerDataSource } from "./configuration.server.data-source.js";
import { UMBCHECKOUT_CONFIGURATION_STORE_CONTEXT } from "./configuration.store.js";

export class UmbCheckoutConfigurationRepository extends UmbDetailRepositoryBase<UmbCheckoutConfigurationModel> {
  #localize = new UmbLocalizationController(this);

  constructor(host: UmbControllerHost) {
    super(host, UmbCheckoutConfigurationServerDataSource, UMBCHECKOUT_CONFIGURATION_STORE_CONTEXT);
  }

  // The configuration is a single row: create and save both store it, and both confirm it like v13 did.
  override async create(model: UmbCheckoutConfigurationModel, parentUnique: string | null) {
    const result = await super.create(model, parentUnique);
    if (!result.error) await this.#notifySaved();
    return result;
  }

  override async save(model: UmbCheckoutConfigurationModel) {
    const result = await super.save(model);
    if (!result.error) await this.#notifySaved();
    return result;
  }

  async #notifySaved() {
    const notifications = await this.getContext(UMB_NOTIFICATION_CONTEXT);
    notifications?.peek("positive", {
      data: {
        headline: this.#localize.term("umbcheckout_configuration_saved_title"),
        message: this.#localize.term("umbcheckout_configuration_saved_message"),
      },
    });
  }
}

export { UmbCheckoutConfigurationRepository as api };
