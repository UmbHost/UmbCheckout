import { UMBCHECKOUT_SECTION_ALIAS } from "../section/constants.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "dashboard",
    alias: "UmbCheckout.Dashboard.Status",
    name: "UmbCheckout Status Dashboard",
    weight: 10,
    element: () => import("./status-dashboard.element.js"),
    meta: { label: "#umbcheckout_status", pathname: "status" },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: UMBCHECKOUT_SECTION_ALIAS }],
  },
];
