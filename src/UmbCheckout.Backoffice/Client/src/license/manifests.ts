export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "globalContext",
    alias: "UmbCheckout.GlobalContext.License",
    name: "UmbCheckout License Context",
    api: () => import("./license.context.js"),
  },
];
