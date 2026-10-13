import {
  ContractStatusBadge,
  FormField,
  InvoiceStatusBadge,
  Modal,
  PremiseStatusBadge,
  TicketTypeBadge,
  inputCls,
  selectCls
} from "./shared"
import { ACCOUNTS, CONTRACTS, INVOICES, LOCATIONS, PREMISES, TICKETS, VIOLATIONS, BUSINESS_TYPES, WHITELIST_PRODUCTS } from "../../mocks/shared"
import { fmt } from "../../scripts/format"
import type { Premise, PremisesSubTab, PremiseStatus } from "../premises/types"

// ─── Dashboard ─────────────────────────────────────────────────────────────

function StatCard({
  label,
  value,
  sub,
  color,
}: {
  label: string
  value: string | number
  sub?: string
  color: string
}) {
  return (
    <div className="bg-white rounded-xl border border-gray-100 p-5 flex flex-col gap-2">
      <div className="flex items-center gap-2">
        <div className={`w-1.5 h-8 rounded-full ${color}`} />
        <span className="text-xs font-medium text-gray-400 uppercase tracking-wider">
          {label}
        </span>
      </div>
      <div
        className="text-2xl font-bold text-gray-900"
        style={{ fontFamily: "DM Sans" }}
      >
        {value}
      </div>
      {sub && <div className="text-xs text-gray-400">{sub}</div>}
    </div>
  )
}

export function Dashboard() {
  const rented = PREMISES.filter((p) => p.status === "rented").length
  const available = PREMISES.filter((p) => p.status === "available").length
  const activeContracts = CONTRACTS.filter((c) => c.status === "signed").length
  const overdueInvoices = INVOICES.filter((i) => i.status === "overdue").length
  const unresolvedVio = VIOLATIONS.filter((v) => !v.resolved).length
  const openTickets = TICKETS.filter((t) => !t.resolved).length
  const totalRevenue = INVOICES.filter((i) => i.status === "paid").reduce(
    (s, i) => s + i.totalAmount,
    0,
  )

  return (
    <div>
      <div className="mb-6">
        <h1
          className="text-xl font-bold text-gray-900"
          style={{ fontFamily: "DM Sans" }}
        >
          Tổng quan hệ thống
        </h1>
        <p className="text-sm text-gray-400 mt-0.5">
          Cập nhật lần cuối: 29/09/2026
        </p>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
        <StatCard
          label="Mặt bằng đang thuê"
          value={rented}
          sub={`${available} còn trống`}
          color="bg-blue-500"
        />
        <StatCard
          label="Hợp đồng hiệu lực"
          value={activeContracts}
          sub="Đã ký kết"
          color="bg-emerald-500"
        />
        <StatCard
          label="Doanh thu tháng"
          value={fmt(totalRevenue)}
          sub="Đã thu"
          color="bg-violet-500"
        />
        <StatCard
          label="Hóa đơn quá hạn"
          value={overdueInvoices}
          sub={`${unresolvedVio} vi phạm chưa xử lý`}
          color="bg-red-500"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
        {/* Premise status breakdown */}
        <div className="bg-white rounded-xl border border-gray-100 p-5">
          <h2
            className="text-sm font-semibold text-gray-700 mb-4"
            style={{ fontFamily: "DM Sans" }}
          >
            Trạng thái mặt bằng
          </h2>
          <div className="space-y-3">
            {([
              "available",
              "rented",
              "reserved",
              "maintenance",
            ] as PremiseStatus[]).map((s) => {
              const count = PREMISES.filter((p) => p.status === s).length
              const pct = Math.round((count / PREMISES.length) * 100)
              const colors: Record<PremiseStatus, string> = {
                available: "bg-emerald-500",
                rented: "bg-blue-500",
                reserved: "bg-amber-500",
                maintenance: "bg-red-400",
              }
              const labels: Record<PremiseStatus, string> = {
                available: "Còn trống",
                rented: "Đang thuê",
                reserved: "Đã đặt cọc",
                maintenance: "Bảo trì",
              }
              return (
                <div key={s}>
                  <div className="flex justify-between items-center mb-1">
                    <span className="text-xs text-gray-500">{labels[s]}</span>
                    <span
                      className="text-xs font-medium text-gray-700"
                      style={{ fontFamily: "JetBrains Mono, monospace" }}
                    >
                      {count} / {PREMISES.length}
                    </span>
                  </div>
                  <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                    <div
                      className={`h-full rounded-full ${colors[s]} transition-all`}
                      style={{ width: `${pct}%` }}
                    />
                  </div>
                </div>
              )
            })}
          </div>
        </div>

        {/* Recent activity */}
        <div className="bg-white rounded-xl border border-gray-100 p-5">
          <h2
            className="text-sm font-semibold text-gray-700 mb-4"
            style={{ fontFamily: "DM Sans" }}
          >
            Hoạt động gần đây
          </h2>
          <div className="space-y-3">
            {[
              {
                icon: "edit_note",
                text: "Hợp đồng CTR-2024-004 chờ ký",
                time: "2 giờ trước",
                color: "text-amber-600",
              },
              {
                icon: "warning",
                text: "Vi phạm mới: Nguyễn Văn A - A01",
                time: "5 giờ trước",
                color: "text-red-600",
              },
              {
                icon: "confirmation_number",
                text: "Ticket mới: Sự cố điều hòa tầng 1",
                time: "8 giờ trước",
                color: "text-blue-600",
              },
              {
                icon: "credit_card",
                text: "Hóa đơn INV-2024-004 quá hạn",
                time: "1 ngày trước",
                color: "text-red-600",
              },
              {
                icon: "task_alt",
                text: "Hợp đồng CTR-2024-001 đã ký",
                time: "3 ngày trước",
                color: "text-emerald-600",
              },
            ].map((a, i) => (
              <div
                key={i}
                className="flex items-center gap-3 py-1.5 border-b border-gray-50 last:border-0"
              >
                <span className="material-symbols-outlined text-base" aria-hidden="true">{a.icon}</span>
                <div className="flex-1 min-w-0">
                  <p className={`text-xs font-medium ${a.color} truncate`}>
                    {a.text}
                  </p>
                </div>
                <span className="text-xs text-gray-300 whitespace-nowrap">
                  {a.time}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Alerts */}
      {(overdueInvoices > 0 || unresolvedVio > 0 || openTickets > 0) && (
        <div className="bg-white rounded-xl border border-gray-100 p-5">
          <h2
            className="text-sm font-semibold text-gray-700 mb-3"
            style={{ fontFamily: "DM Sans" }}
          >
            Cần xử lý
          </h2>
          <div className="flex flex-wrap gap-3">
            {overdueInvoices > 0 && (
              <div className="flex items-center gap-2 bg-red-50 border border-red-100 rounded-lg px-3 py-2">
                <span className="text-red-500 text-sm">●</span>
                <span className="text-xs text-red-700">
                  {overdueInvoices} hóa đơn quá hạn thanh toán
                </span>
              </div>
            )}
            {unresolvedVio > 0 && (
              <div className="flex items-center gap-2 bg-orange-50 border border-orange-100 rounded-lg px-3 py-2">
                <span className="text-orange-500 text-sm">●</span>
                <span className="text-xs text-orange-700">
                  {unresolvedVio} vi phạm chưa xử lý
                </span>
              </div>
            )}
            {openTickets > 0 && (
              <div className="flex items-center gap-2 bg-amber-50 border border-amber-100 rounded-lg px-3 py-2">
                <span className="text-amber-500 text-sm">●</span>
                <span className="text-xs text-amber-700">
                  {openTickets} yêu cầu chờ phản hồi
                </span>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
