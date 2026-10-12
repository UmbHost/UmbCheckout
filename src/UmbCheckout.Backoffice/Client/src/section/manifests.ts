import { UMBCHECKOUT_SECTION_ALIAS, UMBCHECKOUT_SECTION_PATHNAME, UMBCHECKOUT_SETTINGS_MENU_ALIAS } from "./constants.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "section",
    alias: UMBCHECKOUT_SECTION_ALIAS,
    name: "UmbCheckout Section",
    weight: 50,
    meta: { label: "#umbcheckout_section", pathname: UMBCHECKOUT_SECTION_PATHNAME },
  },
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
    meta: { label: "#umbcheckout_menu_settings", menu: UMBCHECKOUT_SETTINGS_MENU_ALIAS },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: UMBCHECKOUT_SECTION_ALIAS }],
  },
];
