import { defineConfig } from 'vite';

export default defineConfig({
  build: {
    lib: {
      entry: 'src/index.ts',
      formats: ['es'],
    },
    outDir: '../wwwroot',
    emptyOutDir: false,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
      output: {
        entryFileNames: 'umbcheckout-backoffice.js',
      },
    },
  },
});
