export const premiseMediaApi = {
  uploadImage: async (file: File): Promise<string> => {
    const formData = new FormData()
    formData.append("file", file)
    let response: Response
    try {
      response = await fetch("/api/premise-medias/upload", { method: "POST", body: formData })
    } catch {
      throw new Error("Không thể kết nối Premise API; kiểm tra backend và HTTPS.")
    }
    if (!response.ok) throw new Error((await response.text()) || `Không tải được ảnh (HTTP ${response.status}).`)
    const result = await response.json() as { imageUrl: string }
    return result.imageUrl
  },
}
