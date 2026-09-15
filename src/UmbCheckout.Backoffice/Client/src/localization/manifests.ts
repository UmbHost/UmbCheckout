export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "localization",
    alias: "UmbCheckout.Localization.En",
    name: "UmbCheckout English Localization",
    weight: -100,
    js: () => import("./en.js"),
    meta: {
      culture: "en",
    },
  },
];
