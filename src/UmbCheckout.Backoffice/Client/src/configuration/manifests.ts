import { UMB_CHECKOUT_MENU_ALIAS } from "../menu/manifests.js";

export const UMB_CHECKOUT_CONFIGURATION_ENTITY_TYPE = "umbcheckout-configuration";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "menuItem",
    alias: "UmbCheckout.MenuItem.Configuration",
    name: "UmbCheckout Configuration Menu Item",
    weight: 1000,
    meta: {
      label: "#umbcheckout_configuration",
      icon: "icon-shopping-basket-alt-2",
      entityType: UMB_CHECKOUT_CONFIGURATION_ENTITY_TYPE,
      menus: [UMB_CHECKOUT_MENU_ALIAS],
    },
  },
  {
    type: "workspace",
    alias: "UmbCheckout.Workspace.Configuration",
    name: "UmbCheckout Configuration Workspace",
    element: () => import("./configuration-workspace.element.js"),
    meta: {
      entityType: UMB_CHECKOUT_CONFIGURATION_ENTITY_TYPE,
    },
  },
];
