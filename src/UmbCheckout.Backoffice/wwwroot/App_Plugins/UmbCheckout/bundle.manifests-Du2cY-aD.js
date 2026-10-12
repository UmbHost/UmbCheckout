import { a as i, U as e, b as o } from "./constants-UFch91IU.js";
import { UmbSubmitWorkspaceAction as n } from "@umbraco-cms/backoffice/workspace";
const s = [
  {
    name: "UmbCheckout Entrypoint",
    alias: "UmbCheckout.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-0n_KMTGo.js")
  }
], m = [
  {
    type: "localization",
    alias: "UmbCheckout.Localization.En",
    name: "UmbCheckout English",
    js: () => import("./en-VaH6GnQm.js"),
    meta: { culture: "en" }
  }
], c = [
  {
    type: "section",
    alias: e,
    name: "UmbCheckout Section",
    weight: 50,
    meta: { label: "#umbcheckout_section", pathname: i }
  },
  {
    type: "menu",
    alias: o,
    name: "UmbCheckout Settings Menu"
  },
  {
    type: "sectionSidebarApp",
    kind: "menu",
    alias: "UmbCheckout.SectionSidebarApp.Settings",
    name: "UmbCheckout Settings Sidebar",
    weight: 100,
    meta: { label: "#umbcheckout_menu_settings", menu: o },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: e }]
  }
], u = [
  {
    type: "globalContext",
    alias: "UmbCheckout.GlobalContext.License",
    name: "UmbCheckout License Context",
    api: () => import("./license.context-fIjwuSaP.js")
  }
], C = [
  {
    type: "dashboard",
    alias: "UmbCheckout.Dashboard.Status",
    name: "UmbCheckout Status Dashboard",
    weight: 10,
    element: () => import("./status-dashboard.element-CCReWH2g.js"),
    meta: { label: "#umbcheckout_status", pathname: "status" },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: e }]
  }
], a = "umbcheckout-configuration", g = "configuration", r = "UmbCheckout.Repository.Configuration", b = "UmbCheckout.Store.Configuration", t = "UmbCheckout.Workspace.Configuration", p = [
  {
    type: "repository",
    alias: r,
    name: "UmbCheckout Configuration Repository",
    api: () => import("./configuration.repository-DpjqzrJW.js")
  },
  {
    type: "store",
    alias: b,
    name: "UmbCheckout Configuration Store",
    api: () => import("./configuration.store-BlabXKVF.js")
  }
], U = [
  {
    type: "workspace",
    kind: "routable",
    alias: t,
    name: "UmbCheckout Configuration Workspace",
    api: () => import("./configuration-workspace.context-ByZ6wxHU.js"),
    meta: { entityType: a }
  },
  {
    type: "workspaceView",
    alias: "UmbCheckout.WorkspaceView.Configuration",
    name: "UmbCheckout Configuration View",
    element: () => import("./configuration-workspace-view.element-BeT_lNG1.js"),
    weight: 100,
    meta: { label: "#umbcheckout_configuration", pathname: "settings", icon: "icon-settings" },
    conditions: [{ alias: "Umb.Condition.WorkspaceAlias", match: t }]
  },
  {
    type: "workspaceAction",
    kind: "default",
    alias: "UmbCheckout.WorkspaceAction.Configuration.Save",
    name: "Save UmbCheckout Configuration",
    api: n,
    meta: { look: "primary", color: "positive", label: "#buttons_save" },
    conditions: [{ alias: "Umb.Condition.WorkspaceAlias", match: t }]
  }
], l = [
  {
    type: "menuItem",
    alias: "UmbCheckout.MenuItem.Configuration",
    name: "UmbCheckout Configuration Menu Item",
    weight: 400,
    meta: {
      label: "#umbcheckout_configuration",
      icon: "icon-settings",
      entityType: a,
      menus: [o]
    }
  }
], h = [...p, ...U, ...l], S = [...s, ...m, ...c, ...u, ...C, ...h];
export {
  g as U,
  a,
  r as b,
  t as c,
  S as m
};
//# sourceMappingURL=bundle.manifests-Du2cY-aD.js.map
