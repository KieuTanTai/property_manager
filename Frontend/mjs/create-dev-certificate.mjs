import {chmodSync, existsSync, mkdirSync} from "node:fs"
import {execFileSync} from "node:child_process"
import path from "node:path"
import {fileURLToPath} from "node:url"

const frontendDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..")
const hasCustomCertificate = Boolean(process.env.HTTPS_CERT_FILE)
const hasCustomKey = Boolean(process.env.HTTPS_KEY_FILE)

if (hasCustomCertificate !== hasCustomKey) {
    throw new Error("Set both HTTPS_CERT_FILE and HTTPS_KEY_FILE, or leave both unset.")
}

const certificatePath = hasCustomCertificate
    ? path.resolve(process.env.HTTPS_CERT_FILE)
    : path.join(frontendDirectory, ".cert", "localhost.pem")
const keyPath = hasCustomKey
    ? path.resolve(process.env.HTTPS_KEY_FILE)
    : path.join(frontendDirectory, ".cert", "localhost-key.pem")

if (!existsSync(certificatePath) || !existsSync(keyPath)) {
    mkdirSync(path.dirname(certificatePath), {recursive: true})
    mkdirSync(path.dirname(keyPath), {recursive: true})

    execFileSync("openssl", [
        "req",
        "-x509",
        "-newkey",
        "rsa:2048",
        "-nodes",
        "-keyout",
        keyPath,
        "-out",
        certificatePath,
        "-sha256",
        "-days",
        "365",
        "-subj",
        "/CN=localhost",
        "-addext",
        "subjectAltName=DNS:localhost,IP:127.0.0.1,IP:::1",
    ], {stdio: "inherit"})

    chmodSync(keyPath, 0o600)
}
