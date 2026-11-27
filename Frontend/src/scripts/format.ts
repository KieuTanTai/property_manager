export const fmt = (value: number) =>
    new Intl.NumberFormat("vi-VN").format(value) + " ₫"

export const isIntegerAmountInput = (value: string) => /^\d*$/.test(value)

export const PREMISE_AREA_PATTERN = /^[0-9]+(?:\.[0-9]+)?$/

export const normalizePremiseArea = (value: string) =>
    value.trim().replace(/\s*m(?:²|2)$/i, "").trim()

export const formatPremiseArea = (value: string) => {
    const area = normalizePremiseArea(value)
    return area ? `${area} m²` : "—"
}

export const today = () => new Date().toISOString().slice(0, 10)

export const formatDateTime = (value: string) =>
    new Intl.DateTimeFormat("vi-VN", {dateStyle: "short", timeStyle: "short"}).format(new Date(value))
