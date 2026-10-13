import { useState } from "react"
import {
  ACCOUNTS,
  CONTRACTS,
  INVOICES,
  LOCATIONS,
  PREMISES,
  TICKETS,
  VIOLATIONS,
  BUSINESS_TYPES,
  WHITELIST_PRODUCTS,
  ContractStatusBadge,
  FormField,
  InvoiceStatusBadge,
  Modal,
  PremiseStatusBadge,
  TicketTypeBadge,
  fmt,
  inputCls,
  selectCls,
} from "./shared"
import type {
  Contract,
  ContractStatus,
  Invoice,
  InvoiceStatus,
  Location,
  Ticket,
  TicketType,
  Violation,
  BusinessType,
  WhitelistProduct,
} from "./shared"
import type { Premise, PremisesSubTab, PremiseStatus } from "../premises/types"

// ─── Violations Module ───────────────────────────────────────────────────────

export function ViolationsModule() {
  const [violations, setViolations] = useState<Violation[]>(VIOLATIONS)
  const [modal, setModal] = useState<"add" | null>(null)
  const [form, setForm] = useState({
    contractId: "",
    tenant: "",
    content: "",
    penaltyAmount: "",
    date: "",
    dueDate: "",
  })

  const resolve = (id: string) =>
    setViolations(
      violations.map((v) => (v.id === id ? { ...v, resolved: true } : v)),
    )

  const handleAdd = () => {
    const v: Violation = {
      id: `VIO-${String(violations.length + 1).padStart(3, "0")}`,
      ...form,
      penaltyAmount: Number(form.penaltyAmount),
      resolved: false,
    }
    setViolations([...violations, v])
    setModal(null)
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1
            className="text-xl font-bold text-gray-900"
            style={{ fontFamily: "DM Sans" }}
          >
            Vi phạm hợp đồng
          </h1>
          <p className="text-sm text-gray-400 mt-0.5">
            {violations.filter((v) => !v.resolved).length} vi phạm chưa xử lý
          </p>
        </div>
        <button
          onClick={() => {
            setForm({
              contractId: "",
              tenant: "",
              content: "",
              penaltyAmount: "",
              date: new Date().toISOString().slice(0, 10),
              dueDate: "",
            })
            setModal("add")
          }}
          className="bg-red-600 hover:bg-red-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors flex items-center gap-2"
        >
          <span>+</span> Ghi nhận vi phạm
        </button>
      </div>

      <div className="space-y-3">
        {violations.map((v) => (
          <div
            key={v.id}
            className={`bg-white rounded-xl border p-5 transition-all ${
              v.resolved ? "border-gray-100 opacity-60" : "border-red-100"
            }`}
          >
            <div className="flex items-start justify-between gap-4">
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-1">
                  <span className="font-mono text-xs text-gray-400">
                    {v.id}
                  </span>
                  <span className="text-gray-300">·</span>
                  <span className="text-xs text-gray-500">
                    HĐ: {v.contractId}
                  </span>
                  <span className="text-gray-300">·</span>
                  <span className="font-medium text-sm text-gray-800">
                    {v.tenant}
                  </span>
                </div>
                <p className="text-sm text-gray-800 mb-2">{v.content}</p>
                <div className="flex items-center gap-4 text-xs text-gray-400">
                  <span>Vi phạm: {v.date}</span>
                  <span>Đến hạn: {v.dueDate}</span>
                  {v.penaltyAmount > 0 && (
                    <span className="font-mono font-semibold text-red-600">
                      {fmt(v.penaltyAmount)}
                    </span>
                  )}
                </div>
              </div>
              <div className="flex items-center gap-2 shrink-0">
                {v.resolved ? (
                  <span className="text-xs text-emerald-600 bg-emerald-50 border border-emerald-100 px-2 py-1 rounded-lg">
                    Đã xử lý
                  </span>
                ) : (
                  <button
                    onClick={() => resolve(v.id)}
                    className="text-xs text-white bg-emerald-600 hover:bg-emerald-700 px-3 py-1.5 rounded-lg transition-colors font-medium"
                  >
                    Đánh dấu xử lý
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>

      {modal === "add" && (
        <Modal title="Ghi nhận vi phạm mới" onClose={() => setModal(null)}>
          <div className="grid grid-cols-2 gap-x-4">
            <FormField label="Mã hợp đồng">
              <input
                className={inputCls}
                value={form.contractId}
                onChange={(e) =>
                  setForm({ ...form, contractId: e.target.value })
                }
                placeholder="CTR-2024-001"
              />
            </FormField>
            <FormField label="Khách thuê">
              <input
                className={inputCls}
                value={form.tenant}
                onChange={(e) => setForm({ ...form, tenant: e.target.value })}
              />
            </FormField>
            <FormField label="Ngày vi phạm">
              <input
                className={inputCls}
                type="date"
                value={form.date}
                onChange={(e) => setForm({ ...form, date: e.target.value })}
              />
            </FormField>
            <FormField label="Ngày đến hạn">
              <input
                className={inputCls}
                type="date"
                value={form.dueDate}
                onChange={(e) => setForm({ ...form, dueDate: e.target.value })}
              />
            </FormField>
          </div>
          <FormField label="Nội dung vi phạm">
            <textarea
              className={inputCls}
              rows={3}
              value={form.content}
              onChange={(e) => setForm({ ...form, content: e.target.value })}
            />
          </FormField>
          <FormField label="Tiền phạt (₫)">
            <input
              className={inputCls}
              type="number"
              value={form.penaltyAmount}
              onChange={(e) =>
                setForm({ ...form, penaltyAmount: e.target.value })
              }
            />
          </FormField>
          <div className="flex justify-end gap-2 mt-2">
            <button
              onClick={() => setModal(null)}
              className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Hủy
            </button>
            <button
              onClick={handleAdd}
              className="px-4 py-2 text-sm bg-red-600 text-white rounded-lg hover:bg-red-700"
            >
              Ghi nhận
            </button>
          </div>
        </Modal>
      )}
    </div>
  )
}
