import { UMBCHECKOUT_SETTINGS_MENU_ALIAS } from "../../section/constants.js";
import { UMBCHECKOUT_CONFIGURATION_ENTITY_TYPE } from "../entity.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "menuItem",
    alias: "UmbCheckout.MenuItem.Configuration",
    name: "UmbCheckout Configuration Menu Item",
    weight: 400,
    meta: {
      label: "#umbcheckout_configuration",
      icon: "icon-settings",
      entityType: UMBCHECKOUT_CONFIGURATION_ENTITY_TYPE,
      menus: [UMBCHECKOUT_SETTINGS_MENU_ALIAS],
    },
  },
];
