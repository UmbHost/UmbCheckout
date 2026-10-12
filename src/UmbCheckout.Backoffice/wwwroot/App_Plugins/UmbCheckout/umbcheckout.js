const t = [
  {
    name: "UmbCheckout Entrypoint",
    alias: "UmbCheckout.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-CkNlDGvj.js")
  }
], n = [
  {
    type: "localization",
    alias: "UmbCheckout.Localization.En",
    name: "UmbCheckout English",
    js: () => import("./en-VaH6GnQm.js"),
    meta: { culture: "en" }
  }
], o = [...t, ...n];
export {
  o as manifests
};
//# sourceMappingURL=umbcheckout.js.map
