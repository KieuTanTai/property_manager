export const toDateInput = (value: string) =>
    value ? new Date(value).toISOString().slice(0, 10) : ""
