export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "localization",
    alias: "UmbCheckout.Localization.En",
    name: "UmbCheckout English",
    js: () => import("./en.js"),
    meta: { culture: "en" },
  },
];
