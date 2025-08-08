import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

const settingsWorkspace = {
    type: 'workspace',
    kind: 'default',
    alias: "umbcheckout-workspace",
    name: 'UmbCheckout Workspace',
    meta: {
        entityType: "umbcheckout",
        headline: '#umbcheckout_workspace_title',
    },
    conditions: [
        {
            alias: "Umb.Condition.SectionAlias",
            match: "Umb.Section.Settings"
        }
    ]
}

const overviewWorkspace = {
    "type": "workspaceView",
    "alias": "umbcheckout-overview-workspace",
    "name": "UmbCheckout Overview",
    element: () => import('./overview-workspace'),
    elementName: "umbcheckout-overview-workspace",
    "meta": {
        "label": "#umbcheckout_overview",
        "pathname": "overview",
        "icon": "icon-dashboard"
    },
    "conditions": [
        {
            "alias": UMB_WORKSPACE_CONDITION_ALIAS,
            "match": "umbcheckout-workspace"
        }
    ]
}

const overviewWorkspaceContext = {
		type: 'workspaceContext',
		name: 'UmbCheckout Overview Workspace Context',
		alias: 'umbcheckout-overview-workspace-context',
		api: () => import('./overview-workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'umbcheckout-workspace',
			},
		],
	}

const workspaceFooterApp = {
		type: 'workspaceFooterApp',
		alias: 'umbcheckout-workspace-footer-app',
		name: 'UmbCheckout Footer App',
		element: () => import('./workspace-footer-app.element.js'),
		weight: 900,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'umbcheckout-workspace',
			},
		],
	};

    const workspaceAction = {
		type: 'workspaceAction',
		kind: 'default',
		name: 'UmbCheckout Save Workspace Action',
		alias: 'umbcheckout-workspace-save-action',
		weight: 1000,
		api: () => import('./workspace-save-action.js'),
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'umbcheckout-workspace',
			},
		],
	}

export const manifests = [
    settingsWorkspace,
    overviewWorkspace,
    workspaceFooterApp,
    overviewWorkspaceContext,
    workspaceAction
];