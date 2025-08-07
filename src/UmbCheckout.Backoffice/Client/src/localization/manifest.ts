const localization = {
    type: "localization",
    alias: "umbcheckout-localize-en",
    name: "UmbCheckout Localization",
    meta: {
        "culture": "en"
    },
    js: () => import('./en')
};

export const manifests = [
    localization
];