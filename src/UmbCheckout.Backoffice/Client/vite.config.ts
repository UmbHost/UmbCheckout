import { defineConfig } from "vite";

export default defineConfig({
  build: {
    lib: {
      entry: {
        umbcheckout: "src/bundle.manifests.ts",
        "umbcheckout-core": "src/exports.ts",
      },
      formats: ["es"],
      fileName: (_format, name) => `${name}.js`,
    },
    outDir: "../wwwroot/App_Plugins/UmbCheckout",
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },
});
