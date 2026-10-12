import { UMBCHECKOUT_SETTINGS_MENU_ALIAS } from "../section/constants.js";

// v13: [Tree("settings", "umbCheckout", TreeGroup = "umbCheckoutGroup")] — a "UmbCheckout" group in Umbraco's Settings section.
export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "menu",
    alias: UMBCHECKOUT_SETTINGS_MENU_ALIAS,
    name: "UmbCheckout Settings Menu",
  },
  {
    type: "sectionSidebarApp",
    kind: "menu",
    alias: "UmbCheckout.SectionSidebarApp.Settings",
    name: "UmbCheckout Settings Sidebar",
    weight: 100,
    meta: { label: "#umbcheckout_section", menu: UMBCHECKOUT_SETTINGS_MENU_ALIAS },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: "Umb.Section.Settings" }],
  },
];
