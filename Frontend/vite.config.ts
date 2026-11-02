import {defineConfig} from "vite"
import react from "@vitejs/plugin-react"
import tailwindcss from "@tailwindcss/vite"
import fs from "node:fs"
import path from "node:path"
import {fileURLToPath} from "node:url"

const frontendDirectory = path.dirname(fileURLToPath(import.meta.url))
const certificatePath = process.env.HTTPS_CERT_FILE
    ? path.resolve(process.env.HTTPS_CERT_FILE)
    : path.join(frontendDirectory, ".cert", "localhost.pem")
const keyPath = process.env.HTTPS_KEY_FILE
    ? path.resolve(process.env.HTTPS_KEY_FILE)
    : path.join(frontendDirectory, ".cert", "localhost-key.pem")

const apiProxyTarget = process.env.GATEWAY_API_PROXY_TARGET || "https://localhost:7002"

export default defineConfig(({mode, command}) => {
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
                // Keep one frontend entry point; Gateway routes requests to their owning modules.
                "/api": {target: apiProxyTarget, changeOrigin: true, secure: false},
            },
        },
        preview: {
            host: process.env.HOST || "0.0.0.0",
            port: parseInt(process.env.PORT || "8443", 10),
            https,
        },
    }
})
