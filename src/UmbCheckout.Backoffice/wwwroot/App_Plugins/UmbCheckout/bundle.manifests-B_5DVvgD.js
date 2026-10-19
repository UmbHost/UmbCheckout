import { U as o } from "./constants-BP4MpHU2.js";
import { UmbSubmitWorkspaceAction as i } from "@umbraco-cms/backoffice/workspace";
const a = [
  {
    name: "UmbCheckout Entrypoint",
    alias: "UmbCheckout.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-0n_KMTGo.js")
  }
], n = [
  {
    type: "localization",
    alias: "UmbCheckout.Localization.En",
    name: "UmbCheckout English",
    js: () => import("./en-Dx8iv9P-.js"),
    meta: { culture: "en" }
  }
], s = [
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
    meta: { label: "#umbcheckout_section", menu: o },
    conditions: [{ alias: "Umb.Condition.SectionAlias", match: "Umb.Section.Settings" }]
  }
], m = [
  {
    type: "globalContext",
    alias: "UmbCheckout.GlobalContext.License",
    name: "UmbCheckout License Context",
    api: () => import("./license.context-BEXJTVvj.js")
  }
], e = "umbcheckout-configuration", h = "configuration", c = "UmbCheckout.Repository.Configuration", r = "UmbCheckout.Store.Configuration", t = "UmbCheckout.Workspace.Configuration", u = [
  {
    type: "repository",
    alias: c,
    name: "UmbCheckout Configuration Repository",
    api: () => import("./configuration.repository-BhTFiAbK.js")
  },
  {
    type: "store",
    alias: r,
    name: "UmbCheckout Configuration Store",
    api: () => import("./configuration.store-BlabXKVF.js")
  }
], C = [
  {
    type: "workspace",
    kind: "routable",
    alias: t,
    name: "UmbCheckout Configuration Workspace",
    api: () => import("./configuration-workspace.context-BckiaSPA.js"),
    meta: { entityType: e }
  },
  {
    type: "workspaceView",
    alias: "UmbCheckout.WorkspaceView.Configuration",
    name: "UmbCheckout Configuration View",
    element: () => import("./configuration-workspace-view.element-Bqhut1sc.js"),
    weight: 100,
    meta: { label: "#umbcheckout_configuration", pathname: "settings", icon: "icon-settings" },
    conditions: [{ alias: "Umb.Condition.WorkspaceAlias", match: t }]
  },
  {
    type: "workspaceAction",
    kind: "default",
    alias: "UmbCheckout.WorkspaceAction.Configuration.Save",
    name: "Save UmbCheckout Configuration",
    api: i,
    meta: { look: "primary", color: "positive", label: "#buttons_save" },
    conditions: [{ alias: "Umb.Condition.WorkspaceAlias", match: t }]
  }
], p = [
  {
    type: "menuItem",
    alias: "UmbCheckout.MenuItem.Configuration",
    name: "UmbCheckout Configuration Menu Item",
    weight: 400,
    meta: {
      label: "#umbcheckout_configuration",
      icon: "icon-settings",
      entityType: e,
      menus: [o]
    }
  }
], U = [...u, ...C, ...p], b = [
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
], f = [...a, ...n, ...s, ...m, ...U, ...b];
export {
  h as U,
  e as a,
  c as b,
  t as c,
  f as m
};
//# sourceMappingURL=bundle.manifests-B_5DVvgD.js.map
