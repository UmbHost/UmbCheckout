export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "UmbCheckout Entrypoint",
    alias: "UmbCheckout.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];
