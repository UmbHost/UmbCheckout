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
    js: () => import("./en-C6sqOqnp.js"),
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
], r = [
  {
    type: "globalContext",
    alias: "UmbCheckout.GlobalContext.License",
    name: "UmbCheckout License Context",
    api: () => import("./license.context-fIjwuSaP.js")
  }
], u = [
  {
    type: "dashboard",
    alias: "UmbCheckout.Dashboard.Status",
    name: "UmbCheckout Status Dashboard",
    weight: 10,
    element: () => import("./status-dashboard.element-CCReWH2g.js"),
    meta: { label: "#umbcheckout_status", pathname: "status" },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: e }]
  }
], a = "umbcheckout-configuration", S = "configuration", C = "UmbCheckout.Repository.Configuration", p = "UmbCheckout.Store.Configuration", t = "UmbCheckout.Workspace.Configuration", U = [
  {
    type: "repository",
    alias: C,
    name: "UmbCheckout Configuration Repository",
    api: () => import("./configuration.repository-BRryeB56.js")
  },
  {
    type: "store",
    alias: p,
    name: "UmbCheckout Configuration Store",
    api: () => import("./configuration.store-BlabXKVF.js")
  }
], b = [
  {
    type: "workspace",
    kind: "routable",
    alias: t,
    name: "UmbCheckout Configuration Workspace",
    api: () => import("./configuration-workspace.context-D_vcFVF9.js"),
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
], h = [...U, ...b, ...l], k = [
  {
    type: "propertyEditorUi",
    alias: "UmbCheckout.PropertyEditorUi.MetaData",
    name: "UmbCheckout MetaData",
    element: () => import("./metadata-editor.element-DNQL9cYi.js"),
    meta: {
      label: "UmbCheckout MetaData",
      icon: "icon-ordered-list",
      group: "eCommerce",
      propertyEditorSchemaAlias: "Umbraco.Plain.Json",
      supportsReadOnly: !0
    }
  }
], y = [...s, ...m, ...c, ...r, ...u, ...h, ...k];
export {
  S as U,
  a,
  C as b,
  t as c,
  y as m
};
//# sourceMappingURL=bundle.manifests-yQeLHgja.js.map
