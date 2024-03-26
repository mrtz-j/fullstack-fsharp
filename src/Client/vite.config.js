import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";
import fable from "vite-plugin-fable";
import { dependencies } from './package.json';

const vendorDeps = ['react', 'react-dom']

const chunksFromDeps = (deps, vendorDeps) => {
  const chunks = {}
  Object.keys(deps).forEach((key) => {
    if (vendorDeps.includes(key)) {
      return
    }
    chunks[key] = [key]
  })
  return chunks
}

const serverPort = 8085
const clientPort = 8080

const proxy = {
  target: `http://localhost:${serverPort}/`,
  changeOrigin: false,
  secure: false,
  ws: true
}

/** @type {import('vite').UserConfig} */
export default defineConfig({
  plugins: [
      fable(),
      react({include: /\.fs$/, jsxRuntime: "classic"}),
  ],
  root: ".",
  publicDir: "./src/Client/public",
  build: {
    outDir: "../../dist/public",
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: vendorDeps,
          ...chunksFromDeps(dependencies, vendorDeps)
        },
        entryFileNames: 'js/[name][hash].js',
        chunkFileNames: 'js/[name][hash].chunk.js',
        assetFileNames: '[ext]/[name][hash].[ext]'
      },
    }
  },
  server: {
    host: '0.0.0.0',
    port: clientPort,
    strictPort: true,
    proxy: {
        '/api': proxy,
    },
    watch: {
        ignored: [
            "bin",
            "obj",
        ],
    }
  }
});
