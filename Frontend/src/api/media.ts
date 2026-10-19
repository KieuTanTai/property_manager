export type MediaUploadPurpose = "avatar" | "premise" | "ticket-and-notification"

export const mediaApi = {
  uploadImage: async (file: File, purpose: MediaUploadPurpose): Promise<string> => {
    const formData = new FormData()
    formData.append("file", file)
    let response: Response
    try {
      response = await fetch(`/api/media/upload/${purpose}`, { method: "POST", body: formData })
    } catch {
      throw new Error("Không thể kết nối Media API; kiểm tra backend và HTTPS.")
    }
    if (!response.ok) throw new Error((await response.text()) || `Không tải được ảnh (HTTP ${response.status}).`)
    const result = await response.json() as { imageUrl: string }
    return result.imageUrl
  },
}
