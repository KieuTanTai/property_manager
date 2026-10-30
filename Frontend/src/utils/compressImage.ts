import imageCompression from "browser-image-compression"
import compressionWorkerUrl from "browser-image-compression/dist/browser-image-compression.js?url"

export type ImageCompressionPurpose = "avatar" | "standard"

const IMAGE_LIMITS: Record<ImageCompressionPurpose, { maxBytes: number; maxDimension: number }> = {
    avatar: {maxBytes: 200 * 1024, maxDimension: 512},
    standard: {maxBytes: 300 * 1024, maxDimension: 1600},
}

/** Compress before Base64 encoding so the original image is never sent to or stored by the API. */
export async function compressImageToDataUrl(file: File, purpose: ImageCompressionPurpose = "standard"): Promise<string> {
    if (!file.type.startsWith("image/")) throw new Error("Vui lòng chọn một tệp hình ảnh.")

    const {maxBytes, maxDimension} = IMAGE_LIMITS[purpose]
    const compressed = await imageCompression(file, {
        maxSizeMB: maxBytes / (1024 * 1024),
        maxWidthOrHeight: maxDimension,
        initialQuality: 0.82,
        maxIteration: 12,
        useWebWorker: true,
        libURL: compressionWorkerUrl,
        fileType: "image/webp",
    })

    if (compressed.size > maxBytes) {
        throw new Error(`Không thể nén ảnh xuống tối đa ${Math.round(maxBytes / 1024)} KB. Hãy chọn ảnh nhỏ hơn.`)
    }

    return await new Promise<string>((resolve, reject) => {
        const reader = new FileReader()
        reader.onload = () => typeof reader.result === "string"
            ? resolve(reader.result)
            : reject(new Error("Không thể mã hóa ảnh đã nén."))
        reader.onerror = () => reject(new Error("Không thể đọc ảnh đã nén."))
        reader.readAsDataURL(compressed)
    })
}
