export const fmt = (value: number) =>
  new Intl.NumberFormat("vi-VN").format(value) + " ₫"

export const today = () => new Date().toISOString().slice(0, 10)

export const formatDateTime = (value: string) =>
  new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short" }).format(new Date(value))
