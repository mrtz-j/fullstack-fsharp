import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";
import { resolve } from 'path';
import { dependencies } from './package.json';

const vendorDeps = ['react', 'react-dom']

const chunksFromDeps = (deps, vendorDeps) => {
  const chunks = {}
  Object.keys(deps).forEach((key) => {
    if (vendorDeps.includes(key) || key.startsWith('@fluetnui')) {
      return
    }
    chunks[key] = [key]
  })
  return chunks
}

const serverPort = 8085
const clientPort = 8080

const proxy = {
  target: `http://127.0.0.1:${serverPort}/`,
  changeOrigin: false,
  secure: false,
  ws: true
}

export default defineConfig({
  plugins: [react()],
  root: ".",
  clearScreen: false,
  publicDir: "./src/Client/public",
  build: {
    outDir: "./deploy/public",
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      input: {
        main: resolve(__dirname, "./src/Client/index.html")
      },
      output: {
        manualChunks: {
          vendor: vendorDeps,
          ...chunksFromDeps(dependencies, vendorDeps)
        },
        entryFileNames: 'js/main.min.js',
        chunkFileNames: 'js/[name].min.js',
        assetFileNames: '[ext]/[name].[ext]'
      },
    }
  },
  server: {
    watch: {
      ignored: [
        "**/*.fs" // Don't watch F# files
      ]
    },
    host: '0.0.0.0',
    port: clientPort,
    https: false,
    cors: false,
    proxy: {
      '/api': proxy,
    }
  }
});
