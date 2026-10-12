import { a, U as t, b as e } from "./constants-UFch91IU.js";
const n = [
  {
    name: "UmbCheckout Entrypoint",
    alias: "UmbCheckout.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-0n_KMTGo.js")
  }
], i = [
  {
    type: "localization",
    alias: "UmbCheckout.Localization.En",
    name: "UmbCheckout English",
    js: () => import("./en-VaH6GnQm.js"),
    meta: { culture: "en" }
  }
], o = [
  {
    type: "section",
    alias: t,
    name: "UmbCheckout Section",
    weight: 50,
    meta: { label: "#umbcheckout_section", pathname: a }
  },
  {
    type: "menu",
    alias: e,
    name: "UmbCheckout Settings Menu"
  },
  {
    type: "sectionSidebarApp",
    kind: "menu",
    alias: "UmbCheckout.SectionSidebarApp.Settings",
    name: "UmbCheckout Settings Sidebar",
    weight: 100,
    meta: { label: "#umbcheckout_menu_settings", menu: e },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: t }]
  }
], s = [
  {
    type: "globalContext",
    alias: "UmbCheckout.GlobalContext.License",
    name: "UmbCheckout License Context",
    api: () => import("./license.context-fIjwuSaP.js")
  }
], m = [
  {
    type: "dashboard",
    alias: "UmbCheckout.Dashboard.Status",
    name: "UmbCheckout Status Dashboard",
    weight: 10,
    element: () => import("./status-dashboard.element-zRYul-PQ.js"),
    meta: { label: "#umbcheckout_status", pathname: "status" },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: t }]
  }
], b = [...n, ...i, ...o, ...s, ...m];
export {
  b as manifests
};
//# sourceMappingURL=umbcheckout.js.map
