export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "propertyEditorUi",
    alias: "UmbCheckout.PropertyEditorUi.MetaData",
    name: "UmbCheckout Meta Data Property Editor UI",
    element: () => import("./meta-data/property-editor-ui-meta-data.element.js"),
    meta: {
      label: "Meta Data",
      icon: "icon-ordered-list",
      group: "eCommerce",
      propertyEditorSchemaAlias: "umbCheckoutMetaData",
      supportsReadOnly: true,
    },
  },
];
