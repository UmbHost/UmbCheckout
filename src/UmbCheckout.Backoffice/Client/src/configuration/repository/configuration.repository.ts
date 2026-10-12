import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDetailRepositoryBase } from "@umbraco-cms/backoffice/repository";
import type { UmbCheckoutConfigurationModel } from "../types.js";
import { UmbCheckoutConfigurationServerDataSource } from "./configuration.server.data-source.js";
import { UMBCHECKOUT_CONFIGURATION_STORE_CONTEXT } from "./configuration.store.js";

export class UmbCheckoutConfigurationRepository extends UmbDetailRepositoryBase<UmbCheckoutConfigurationModel> {
  constructor(host: UmbControllerHost) {
    super(host, UmbCheckoutConfigurationServerDataSource, UMBCHECKOUT_CONFIGURATION_STORE_CONTEXT);
  }
}

export { UmbCheckoutConfigurationRepository as api };
