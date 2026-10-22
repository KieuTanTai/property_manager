import { useEffect, useState } from "react"

const defaultMaxImageCount = 5
const maxImageSize = 10 * 1024 * 1024

function PreviewImage({ file, onClick }: { file: File; onClick: () => void }) {
  const [url, setUrl] = useState("")

  useEffect(() => {
    const objectUrl = URL.createObjectURL(file)
    setUrl(objectUrl)
    return () => URL.revokeObjectURL(objectUrl)
  }, [file])

  return <button type="button" onClick={onClick} className="group relative h-20 w-20 overflow-hidden rounded-lg border border-slate-200 bg-slate-100" aria-label={`Xem ảnh ${file.name}`}>
    {url && <img src={url} alt={file.name} className="h-full w-full object-cover transition group-hover:scale-105" />}
  </button>
}

function FullscreenImage({ file }: { file: File }) {
  const [url, setUrl] = useState("")
  useEffect(() => {
    const objectUrl = URL.createObjectURL(file)
    setUrl(objectUrl)
    return () => URL.revokeObjectURL(objectUrl)
  }, [file])
  return <img src={url} alt={file.name} onClick={(event) => event.stopPropagation()} className="max-h-[calc(100vh-3rem)] max-w-[calc(100vw-3rem)] object-contain" />
}

export function ImageAttachments({
  files,
  onChange,
  maxCount = defaultMaxImageCount,
}: {
  files: File[]
  onChange: (files: File[]) => void
  maxCount?: number
}) {
  const [activeIndex, setActiveIndex] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (activeIndex === null) return
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") setActiveIndex(null)
      if (event.key === "ArrowLeft") setActiveIndex((index) => index === null ? null : (index - 1 + files.length) % files.length)
      if (event.key === "ArrowRight") setActiveIndex((index) => index === null ? null : (index + 1) % files.length)
    }
    window.addEventListener("keydown", closeOnEscape)
    return () => window.removeEventListener("keydown", closeOnEscape)
  }, [activeIndex, files.length])

  const addFiles = (selectedFiles: FileList | null) => {
    if (!selectedFiles) return
    const nextFiles = Array.from(selectedFiles)
    const invalidFile = nextFiles.find((file) => !file.type.startsWith("image/") || file.size > maxImageSize)
    if (invalidFile) {
      setError(invalidFile.size > maxImageSize ? "Dung lượng mỗi ảnh tối đa 10 MB." : "Chỉ hỗ trợ tệp hình ảnh.")
      return
    }
    if (files.length + nextFiles.length > maxCount) {
      setError(`Có thể chọn thêm tối đa ${maxCount} ảnh.`)
      return
    }
    setError(null)
    onChange([...files, ...nextFiles])
  }

  return <div className="space-y-2">
    <label className="inline-flex cursor-pointer items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50">
      <span className="material-symbols-outlined text-lg" aria-hidden="true">add_photo_alternate</span>
      Thêm ảnh
      <input type="file" accept="image/jpeg,image/png,image/webp,image/gif" multiple className="sr-only" onChange={(event) => { addFiles(event.currentTarget.files); event.currentTarget.value = "" }} />
    </label>
    <p className="text-xs text-slate-500">Có thể thêm tối đa {maxCount} ảnh, 10 MB mỗi ảnh.</p>
    {error && <p role="alert" className="text-xs text-red-600">{error}</p>}
    {files.length > 0 && <div className="flex flex-wrap gap-2">
      {files.map((file, index) => <div key={`${file.name}-${file.lastModified}-${index}`} className="relative">
        <PreviewImage file={file} onClick={() => setActiveIndex(index)} />
        <button type="button" aria-label={`Xóa ảnh ${file.name}`} onClick={() => { onChange(files.filter((_, fileIndex) => fileIndex !== index)); setActiveIndex(null) }} className="absolute -right-1 -top-1 flex h-6 w-6 items-center justify-center rounded-full bg-slate-900 text-sm text-white">×</button>
      </div>)}
    </div>}
    {activeIndex !== null && files[activeIndex] && <div role="dialog" aria-modal="true" aria-label={`Ảnh ${activeIndex + 1} trong ${files.length}`} onClick={() => setActiveIndex(null)} className="fixed inset-0 z-[80] flex items-center justify-center bg-black/95 p-4">
      <button type="button" aria-label="Đóng ảnh" onClick={() => setActiveIndex(null)} className="absolute right-5 top-4 z-10 flex h-11 w-11 items-center justify-center rounded-full bg-white/10 text-3xl text-white">&times;</button>
      <FullscreenImage file={files[activeIndex]} />
      {files.length > 1 && <>
        <button type="button" aria-label="Ảnh trước" onClick={(event) => { event.stopPropagation(); setActiveIndex((activeIndex - 1 + files.length) % files.length) }} className="absolute left-4 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full bg-white/10 text-3xl text-white">‹</button>
        <button type="button" aria-label="Ảnh tiếp theo" onClick={(event) => { event.stopPropagation(); setActiveIndex((activeIndex + 1) % files.length) }} className="absolute right-4 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full bg-white/10 text-3xl text-white">›</button>
        <span className="absolute bottom-4 left-1/2 -translate-x-1/2 rounded-full bg-white/10 px-3 py-1 text-sm text-white">{activeIndex + 1} / {files.length}</span>
      </>}
    </div>}
  </div>
}
