export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "propertyEditorUi",
    alias: "UmbCheckout.PropertyEditorUi.MetaData",
    name: "UmbCheckout MetaData",
    element: () => import("./metadata-editor.element.js"),
    meta: {
      label: "UmbCheckout MetaData",
      icon: "icon-ordered-list",
      group: "eCommerce",
      propertyEditorSchemaAlias: "Umbraco.Plain.Json",
      supportsReadOnly: true,
    },
  },
];
