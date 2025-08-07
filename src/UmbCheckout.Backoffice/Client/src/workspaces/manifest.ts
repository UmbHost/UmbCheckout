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

const overview = {
    "type": "workspaceView",
    "alias": "umbcheckout-overview",
    "name": "UmbCheckout Overview",
    element: () => import('./overview'),
    elementName: "umbcheckout-overview",
    "meta": {
        "label": "#umbcheckout_overview",
        "pathname": "overview",
        "icon": "icon-dashboard"
    },
    "conditions": [
        {
            "alias": "Umb.Condition.WorkspaceAlias",
            "match": "umbcheckout-workspace"
        }
    ]
}

export const manifests = [
    settingsWorkspace,
    overview
];