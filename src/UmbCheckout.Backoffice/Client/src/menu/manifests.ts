/**
 * The alias of the UmbCheckout menu in the Settings section.
 * Payment providers (e.g. UmbCheckout.Stripe) add their own menu items to this menu.
 */
export const UMB_CHECKOUT_MENU_ALIAS = "UmbCheckout.Menu";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "menu",
    alias: UMB_CHECKOUT_MENU_ALIAS,
    name: "UmbCheckout Menu",
  },
  {
    type: "sectionSidebarApp",
    kind: "menu",
    alias: "UmbCheckout.SectionSidebarMenu",
    name: "UmbCheckout Sidebar Menu",
    weight: 90,
    meta: {
      label: "#umbcheckout_group",
      menu: UMB_CHECKOUT_MENU_ALIAS,
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Settings",
      },
    ],
  },
];
