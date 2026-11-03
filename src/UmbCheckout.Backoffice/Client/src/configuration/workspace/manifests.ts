import { UmbSubmitWorkspaceAction } from "@umbraco-cms/backoffice/workspace";
import { UMBCHECKOUT_CONFIGURATION_ENTITY_TYPE, UMBCHECKOUT_CONFIGURATION_WORKSPACE_ALIAS } from "../entity.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "workspace",
    kind: "routable",
    alias: UMBCHECKOUT_CONFIGURATION_WORKSPACE_ALIAS,
    name: "UmbCheckout Configuration Workspace",
    api: () => import("./configuration-workspace.context.js"),
    meta: { entityType: UMBCHECKOUT_CONFIGURATION_ENTITY_TYPE },
  },
  {
    type: "workspaceView",
    alias: "UmbCheckout.WorkspaceView.Configuration",
    name: "UmbCheckout Configuration View",
    element: () => import("./views/configuration-workspace-view.element.js"),
    weight: 100,
    meta: { label: "#umbcheckout_configuration", pathname: "settings", icon: "icon-settings" },
    conditions: [{ alias: "Umb.Condition.WorkspaceAlias", match: UMBCHECKOUT_CONFIGURATION_WORKSPACE_ALIAS }],
  },
  {
    type: "workspaceAction",
    kind: "default",
    alias: "UmbCheckout.WorkspaceAction.Configuration.Save",
    name: "Save UmbCheckout Configuration",
    api: UmbSubmitWorkspaceAction,
    meta: { look: "primary", color: "positive", label: "#buttons_save" },
    conditions: [{ alias: "Umb.Condition.WorkspaceAlias", match: UMBCHECKOUT_CONFIGURATION_WORKSPACE_ALIAS }],
  },
];
