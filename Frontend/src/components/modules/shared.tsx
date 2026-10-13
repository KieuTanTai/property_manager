import type { ReactNode } from "react"
import type { PremiseStatus } from "../premises/types"
import type { ContractStatus, InvoiceStatus, TicketType } from "@/types/modules.ts"


// ─── Badge helpers ─────────────────────────────────────────────────────────

// Nền nhạt cùng tông với PremiseStatusBadge, dùng cho card / modal mặt bằng
export const premiseStatusSurface: Record<PremiseStatus, string> = {
  available: "bg-emerald-50",
  rented: "bg-blue-50",
  reserved: "bg-amber-50",
  maintenance: "bg-red-50",
}

export function PremiseStatusBadge({ status }: { status: PremiseStatus }) {
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
  const { label, cls } = map[status]
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
    >
      {label}
    </span>
  )
}

export function ContractStatusBadge({ status }: { status: ContractStatus }) {
  const map: Record<ContractStatus, { label: string; cls: string }> = {
    pending_approval: {
      label: "Chờ duyệt",
      cls: "bg-amber-100 text-amber-700 border border-amber-200",
    },
    pending_signature: {
      label: "Chờ ký",
      cls: "bg-sky-100 text-sky-700 border border-sky-200",
    },
    signed: {
      label: "Đã ký",
      cls: "bg-emerald-100 text-emerald-700 border border-emerald-200",
    },
    canceled: {
      label: "Đã hủy",
      cls: "bg-gray-100 text-gray-500 border border-gray-200",
    },
    expired: {
      label: "Hết hạn",
      cls: "bg-orange-100 text-orange-700 border border-orange-200",
    },
    terminated: {
      label: "Đã chấm dứt",
      cls: "bg-red-100 text-red-700 border border-red-200",
    },
  }
  const { label, cls } = map[status]
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
    >
      {label}
    </span>
  )
}

export function InvoiceStatusBadge({ status }: { status: InvoiceStatus }) {
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
  const { label, cls } = map[status]
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
    >
      {label}
    </span>
  )
}

export function TicketTypeBadge({ type }: { type: TicketType }) {
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
  const { label, cls } = map[type]
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${cls}`}
    >
      {label}
    </span>
  )
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
      style={{ background: "rgba(10,14,26,0.6)" }}
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
            style={{ fontFamily: "DM Sans" }}
          >
            {title}
          </h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors text-xl leading-none"
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
