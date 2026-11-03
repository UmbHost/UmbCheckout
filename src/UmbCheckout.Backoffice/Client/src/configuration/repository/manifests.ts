import { UMBCHECKOUT_CONFIGURATION_REPOSITORY_ALIAS, UMBCHECKOUT_CONFIGURATION_STORE_ALIAS } from "../entity.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "repository",
    alias: UMBCHECKOUT_CONFIGURATION_REPOSITORY_ALIAS,
    name: "UmbCheckout Configuration Repository",
    api: () => import("./configuration.repository.js"),
  },
  {
    type: "store",
    alias: UMBCHECKOUT_CONFIGURATION_STORE_ALIAS,
    name: "UmbCheckout Configuration Store",
    api: () => import("./configuration.store.js"),
  },
];
