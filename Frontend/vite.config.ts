import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"
import tailwindcss from "@tailwindcss/vite"
import fs from "node:fs"
import path from "node:path"
import { fileURLToPath } from "node:url"

const frontendDirectory = path.dirname(fileURLToPath(import.meta.url))
const certificatePath = process.env.HTTPS_CERT_FILE
  ? path.resolve(process.env.HTTPS_CERT_FILE)
  : path.join(frontendDirectory, ".cert", "localhost.pem")
const keyPath = process.env.HTTPS_KEY_FILE
  ? path.resolve(process.env.HTTPS_KEY_FILE)
  : path.join(frontendDirectory, ".cert", "localhost-key.pem")

const apiProxyTarget = process.env.IDENTITY_API_PROXY_TARGET || "https://localhost:7048"

export default defineConfig(({ mode, command }) => {
  const emitSourcemaps = mode === "development"
  const https = command === "build"
    ? undefined
    : {
        cert: fs.readFileSync(certificatePath),
        key: fs.readFileSync(keyPath),
      }

  return {
    base: "/",
    build: {
      sourcemap: emitSourcemaps ? "inline" : false,
      minify: !emitSourcemaps,
    },
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        "@": path.resolve(frontendDirectory, "./src"),
      },
    },
    server: {
      host: process.env.HOST || "0.0.0.0",
      port: parseInt(process.env.PORT || "8443", 10),
      strictPort: true,
      https,
      proxy: {
        "/api/account": { target: apiProxyTarget, changeOrigin: true, secure: false },
        "/api/authorization": { target: apiProxyTarget, changeOrigin: true, secure: false },
        "/api/profile": { target: apiProxyTarget, changeOrigin: true, secure: false },
        // Local ASP.NET development endpoints use a self-signed certificate.
        "/api/Contract": { target: "https://localhost:7255", changeOrigin: true, secure: false },
        "/api/Regulation": { target: "https://localhost:7255", changeOrigin: true, secure: false },
        "/api/Invoice": { target: "https://localhost:7255", changeOrigin: true, secure: false },
        "/api/Receipt": { target: "https://localhost:7255", changeOrigin: true, secure: false },
        "/api/ContactInfo": { target: "https://localhost:7255", changeOrigin: true, secure: false },
        "/api/Violation": { target: "https://localhost:7255", changeOrigin: true, secure: false },
        "/api/tickets": { target: "https://localhost:7092", changeOrigin: true, secure: false },
        "/api/ticket-medias": { target: "https://localhost:7092", changeOrigin: true, secure: false },
        "/api/notifications": { target: "https://localhost:7092", changeOrigin: true, secure: false },
        "/api/notification-recipients": { target: "https://localhost:7092", changeOrigin: true, secure: false },
        "/api/premise-medias": { target: "https://localhost:7081", changeOrigin: true, secure: false },
      },
    },
    preview: {
      host: process.env.HOST || "0.0.0.0",
      port: parseInt(process.env.PORT || "8443", 10),
      https,
    },
  }
})
