import type {FormHTMLAttributes, ReactNode, SubmitEvent} from "react"
import {useEffect, useState} from "react"
import {APP_SETTINGS} from "@/config/appSettings"
import type {UserProfileModel} from "@/types/identity.ts"
import type {PremiseStatus} from "../premises/types"
import type {ContractStatus, InvoiceStatus, TicketType} from "@/types/modules.ts"

type ProcessingFormProps = Omit<FormHTMLAttributes<HTMLFormElement>, "onSubmit"> & {
    processing?: boolean
    onSubmit: (event: SubmitEvent<HTMLFormElement>) => void | Promise<void>
}

export function ProcessingForm({processing = false, onSubmit, children, ...formProps}: ProcessingFormProps) {
    const [submitting, setSubmitting] = useState(false)
    const handleSubmit = async (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        if (submitting || processing) return
        setSubmitting(true)
        try {
            await onSubmit(event)
        } finally {
            setSubmitting(false)
        }
    }

    return <form {...formProps} onSubmit={(event) => void handleSubmit(event)}>
        <fieldset disabled={processing || submitting} className="m-0 min-w-0 border-0 p-0">
            {children}
        </fieldset>
    </form>
}


// ─── Badge helpers ─────────────────────────────────────────────────────────

export function formatProfileFullName(profile: UserProfileModel | null | undefined) {
    return [profile?.userProfileLastName, profile?.userProfileFirstName]
        .map((part) => part?.trim())
        .filter((part): part is string => Boolean(part))
        .join(" ")
}

// Nền nhạt cùng tông với PremiseStatusBadge, dùng cho card / modal mặt bằng
export const premiseStatusSurface: Record<PremiseStatus, string> = {
    available: "bg-emerald-50",
    rented: "bg-blue-50",
    reserved: "bg-amber-50",
    maintenance: "bg-red-50",
}

export function PremiseStatusBadge({status}: { status: PremiseStatus }) {
    const map: Record<PremiseStatus, { label: string; cls: string }> = {
        available: {
            label: "Còn trống",
            cls: "bg-emerald-100 text-emerald-700 border border-emerald-200",
        },
        rented: {
            label: "Đang thuê",
            cls: "bg-blue-100 text-blue-700 border border-blue-200",
        },
        reserved: {
            label: "Đã đặt cọc",
            cls: "bg-amber-100 text-amber-700 border border-amber-200",
        },
        maintenance: {
            label: "Bảo trì",
            cls: "bg-red-100 text-red-700 border border-red-200",
        },
    }
    const {label, cls} = map[status]
    return (
        <span
            className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
        >
      {label}
    </span>
    )
}

export function ContractStatusBadge({status}: { status: ContractStatus }) {
    const map: Record<ContractStatus, { label: string; cls: string }> = {
        PendingApproval: {
            label: "Chờ duyệt",
            cls: "bg-amber-100 text-amber-700 border border-amber-200",
        },
        PendingSignature: {
            label: "Chờ ký",
            cls: "bg-sky-100 text-sky-700 border border-sky-200",
        },
        Signed: {
            label: "Đã ký",
            cls: "bg-emerald-100 text-emerald-700 border border-emerald-200",
        },
        Canceled: {
            label: "Đã hủy",
            cls: "bg-gray-100 text-gray-500 border border-gray-200",
        },
        Expired: {
            label: "Hết hạn",
            cls: "bg-orange-100 text-orange-700 border border-orange-200",
        },
        Terminated: {
            label: "Đã chấm dứt",
            cls: "bg-red-100 text-red-700 border border-red-200",
        },
    }
    const {label, cls} = map[status]
    return (
        <span
            className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
        >
      {label}
    </span>
    )
}

export function InvoiceStatusBadge({status}: { status: InvoiceStatus }) {
    const map: Record<InvoiceStatus, { label: string; cls: string }> = {
        paid: {
            label: "Đã thanh toán",
            cls: "bg-emerald-100 text-emerald-700 border border-emerald-200",
        },
        unpaid: {
            label: "Chưa thanh toán",
            cls: "bg-amber-100 text-amber-700 border border-amber-200",
        },
        overdue: {
            label: "Quá hạn",
            cls: "bg-red-100 text-red-700 border border-red-200",
        },
    }
    const {label, cls} = map[status]
    return (
        <span
            className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
        >
      {label}
    </span>
    )
}

export function TicketTypeBadge({type}: { type: TicketType }) {
    const map: Record<TicketType, { label: string; cls: string }> = {
        review: {
            label: "Đánh giá",
            cls: "bg-violet-100 text-violet-700 border border-violet-200",
        },
        complaint: {
            label: "Khiếu nại",
            cls: "bg-red-100 text-red-700 border border-red-200",
        },
        violation: {
            label: "Tố cáo vi phạm",
            cls: "bg-orange-100 text-orange-700 border border-orange-200",
        },
        feedback: {
            label: "Phản hồi",
            cls: "bg-sky-100 text-sky-700 border border-sky-200",
        },
    }
    const {label, cls} = map[type]
    return (
        <span
            className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
        >
      {label}
    </span>
    )
}

export type BaseTableColumn<T> = {
    key: string
    header: ReactNode
    headerClassName?: string
    cellClassName?: string
    render: (row: T, index: number) => ReactNode
}

type BaseTableProps<T> = {
    rows: T[]
    columns: BaseTableColumn<T>[]
    getRowKey: (row: T) => string
    isHaveStt?: boolean
    sttStart?: number
    pageSize?: number
    paginationResetKey?: string | number
    loading?: boolean
    emptyMessage?: string
    tableClassName?: string
    rowClassName?: (row: T, index: number) => string
    onRowClick?: (row: T) => void
    rowTitle?: string
}

/** Reusable data table with optional display-only sequence number and page controls. */
export function BaseTable<T>({
    rows,
    columns,
    getRowKey,
    isHaveStt = true,
    sttStart = 1,
    pageSize = APP_SETTINGS.pagination.defaultPageSize,
    paginationResetKey,
    loading = false,
    emptyMessage = "Không có dữ liệu.",
    tableClassName = "w-full text-sm",
    rowClassName,
    onRowClick,
    rowTitle,
}: BaseTableProps<T>) {
    const [requestedPage, setRequestedPage] = useState(1)
    const [isEditingPage, setIsEditingPage] = useState(false)
    const [pageInput, setPageInput] = useState("1")
    const pageCount = Math.max(1, Math.ceil(rows.length / pageSize))
    const page = Math.min(requestedPage, pageCount)
    const pageRows = rows.slice((page - 1) * pageSize, page * pageSize)
    const canPaginate = rows.length > pageSize

    useEffect(() => {
        setRequestedPage(1)
        setIsEditingPage(false)
    }, [paginationResetKey])
    useEffect(() => {
        if (requestedPage !== page) setRequestedPage(page)
    }, [page, requestedPage])
    useEffect(() => setPageInput(String(page)), [page])

    const commitPageInput = () => {
        const inputPage = Number(pageInput)
        const nextPage = Number.isFinite(inputPage)
            ? Math.max(1, Math.min(pageCount, Math.trunc(inputPage)))
            : page
        setRequestedPage(nextPage)
        setPageInput(String(nextPage))
        setIsEditingPage(false)
    }

    return <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
        <table className={tableClassName}>
            <thead className="bg-slate-50 text-xs text-slate-500">
            <tr>
                {isHaveStt && <th className="w-14 px-4 py-3 text-center font-medium">STT</th>}
                {columns.map((column) => <th key={column.key}
                    className={column.headerClassName ?? "px-4 py-3 text-left font-medium"}>{column.header}</th>)}
            </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
            {loading ? <tr><td colSpan={columns.length + (isHaveStt ? 1 : 0)}
                className="p-8 text-center text-slate-500">Đang tải...</td></tr> : rows.length ? pageRows.map((row, index) =>
                <tr key={getRowKey(row)} onClick={() => onRowClick?.(row)}
                    onKeyDown={onRowClick ? (event) => {
                        if (event.key === "Enter" || event.key === " ") {
                            event.preventDefault()
                            onRowClick(row)
                        }
                    } : undefined}
                    tabIndex={onRowClick ? 0 : undefined} title={rowTitle}
                    className={`${rowClassName?.(row, index) ?? ""} ${onRowClick ? "cursor-pointer" : ""}`}>
                    {isHaveStt && <td className="px-4 py-3 text-center tabular-nums text-slate-400">{sttStart + (page - 1) * pageSize + index}</td>}
                    {columns.map((column) => <td key={column.key} className={column.cellClassName ?? "px-4 py-3"}>
                        {column.render(row, index)}
                    </td>)}
                </tr>) : <tr><td colSpan={columns.length + (isHaveStt ? 1 : 0)}
                className="p-8 text-center text-slate-500">{emptyMessage}</td></tr>}
            </tbody>
        </table>
        {canPaginate && <footer className="flex justify-center border-t border-slate-100 px-3 py-3">
            <nav aria-label="Phân trang" className="flex items-center justify-center gap-4 text-sm">
                <button type="button" aria-label="Trang trước" disabled={page <= 1} onClick={() => setRequestedPage((current) => Math.max(1, current - 1))}
                    className="rounded-md border border-slate-200 p-2 text-slate-600 transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40">
                    <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="h-4 w-4">
                        <path strokeLinecap="round" strokeLinejoin="round" d="m15 18-6-6 6-6" />
                    </svg>
                </button>
                <span aria-live="polite" className="min-w-20 text-center text-slate-600">
                    {isEditingPage ? <input
                        aria-label="Chuyển đến trang"
                        type="number"
                        min={1}
                        max={pageCount}
                        step={1}
                        autoFocus
                        value={pageInput}
                        onChange={(event) => setPageInput(event.target.value)}
                        onBlur={commitPageInput}
                        onKeyDown={(event) => {
                            if (event.key === "Enter") commitPageInput()
                            if (event.key === "Escape") {
                                setPageInput(String(page))
                                setIsEditingPage(false)
                            }
                        }}
                        className="w-12 rounded border border-blue-300 px-1 py-0.5 text-center font-semibold text-blue-600 outline-none focus:ring-2 focus:ring-blue-200"
                    /> : <button
                        type="button"
                        aria-label={`Trang hiện tại ${page}, bấm để nhập số trang`}
                        onClick={() => {
                            setPageInput(String(page))
                            setIsEditingPage(true)
                        }}
                        className="rounded px-1 font-semibold text-blue-600 hover:bg-blue-50"
                    >{page}</button>}
                    <span className="mx-1">/</span>
                    {pageCount}
                </span>
                <button type="button" aria-label="Trang sau" disabled={page >= pageCount} onClick={() => setRequestedPage((current) => Math.min(pageCount, current + 1))}
                    className="rounded-md border border-slate-200 p-2 text-slate-600 transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40">
                    <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="h-4 w-4">
                        <path strokeLinecap="round" strokeLinejoin="round" d="m9 18 6-6-6-6" />
                    </svg>
                </button>
            </nav>
        </footer>}
    </div>
}


// ─── Modal ─────────────────────────────────────────────────────────────────

export function Modal({
                          title,
                          onClose,
                          children,
                          wide = false,
                          compact = false,
                          panelClassName = "",
                      }: {
    title: string
    onClose: () => void
    children: ReactNode
    wide?: boolean
    compact?: boolean
    panelClassName?: string
}) {
    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center"
            style={{background: "rgba(10,14,26,0.6)"}}
            onMouseDown={(event) => {
                if (event.target === event.currentTarget) onClose()
            }}


        >
            <div
                className={`rounded-xl shadow-2xl w-full ${
                    wide ? (compact ? "max-w-5xl" : "max-w-6xl") : "max-w-lg"
                } mx-4 max-h-[calc(100vh-3rem)] overflow-y-auto ${panelClassName || "bg-white"}`}
                onMouseDown={(event) => event.stopPropagation()}
            >
                <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
                    <h3
                        className="text-base font-semibold text-gray-900"
                        style={{fontFamily: "DM Sans"}}
                    >
                        {title}
                    </h3>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-gray-600 transition-colors text-2xl leading-none"
                    >
                        &times;
                    </button>
                </div>
                <div className="px-6 py-5">{children}</div>
            </div>
        </div>
    )
}

export function FormField({
                              label,
                              children,
                          }: {
    label: string
    children: ReactNode
}) {
    return (
        <div className="mb-4">
            <label className="block text-xs font-medium text-gray-500 mb-1 uppercase tracking-wide">
                {label}
            </label>
            {children}
        </div>
    )
}

export const inputCls =
    "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white transition disabled:border-gray-300 disabled:bg-gray-100 disabled:text-gray-500 disabled:cursor-not-allowed"
export const selectCls =
    "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white transition bg-white disabled:border-gray-300 disabled:bg-gray-100 disabled:text-gray-500 disabled:cursor-not-allowed"
