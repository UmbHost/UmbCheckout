import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbDetailStoreBase } from "@umbraco-cms/backoffice/store";
import type { UmbCheckoutConfigurationModel } from "../types.js";

export class UmbCheckoutConfigurationStore extends UmbDetailStoreBase<UmbCheckoutConfigurationModel> {
  constructor(host: UmbControllerHost) {
    super(host, UMBCHECKOUT_CONFIGURATION_STORE_CONTEXT.toString());
  }
}

export const UMBCHECKOUT_CONFIGURATION_STORE_CONTEXT = new UmbContextToken<UmbCheckoutConfigurationStore>("UmbCheckoutConfigurationStore");

export { UmbCheckoutConfigurationStore as api };
