export function getImageDisplayUrl(imageUrl: string): string {
  try {
    const url = new URL(imageUrl)
    if (url.hostname !== "drive.google.com") return imageUrl

    const fileId = url.searchParams.get("id")
      ?? url.pathname.match(/^\/file\/d\/([^/]+)/)?.[1]
    if (!fileId) return imageUrl

    return `https://drive.google.com/uc?export=download&id=${encodeURIComponent(fileId)}`
  } catch {
    return imageUrl
  }
}
