export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Umbraco Extension 1Entrypoint",
    alias: "Umbraco.Extension1.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.ts"),
  },
];
