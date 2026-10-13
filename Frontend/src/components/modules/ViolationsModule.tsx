import { useMemo, useState } from "react"
import type { FormEvent } from "react"
import { MOCK_CONTRACTS } from "../../mocks/contracts"
import { getMockTenant, MOCK_VIOLATIONS } from "../../mocks/violations"
import { FormField, Modal, inputCls, selectCls } from "./shared"
import { fmt } from "../../scripts/format"
import type { Violation } from "../../types/modules"
import { filterViolationsForAccount } from "../../scripts/violations"
import { createUuid } from "../../scripts/ids"
import type { ViolationFilter, ViolationForm, ViolationsModuleProps } from "../../types/violations"

const PAGE_SIZE = 10
const emptyForm: ViolationForm = { contractId: "", tenant: "", content: "", penaltyAmount: 0, date: "", dueDate: "", resolved: false }

const newMockId = createUuid

export function ViolationsModule({ accountId, readOnly = false }: ViolationsModuleProps) {
  const initialMockViolations = filterViolationsForAccount(MOCK_VIOLATIONS, MOCK_CONTRACTS, accountId, readOnly)
  const [violations, setViolations] = useState<Violation[]>(initialMockViolations)
  const [modal, setModal] = useState<"add" | "view" | null>(null)
  const [selected, setSelected] = useState<Violation | null>(null)
  const [isEditing, setIsEditing] = useState(false)
  const [form, setForm] = useState<ViolationForm>(emptyForm)
  const [filter, setFilter] = useState<ViolationFilter>("all")
  const [page, setPage] = useState(1)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // API query đang tắt để kiểm thử bằng mock data; giữ query cùng contract controller để bật lại sau.
  // useEffect(() => {
  //   let cursor: string | null = null
  //   const page = await contractApi.violations(cursor, PAGE_SIZE)
  //   setViolations(page.items.map(mapViolationDtoToUi))
  // }, [])

  const visible = useMemo(() => violations.filter((item) =>
    filter === "all" || item.resolved === (filter === "resolved"),
  ), [filter, violations])
  const pageCount = Math.max(1, Math.ceil(visible.length / PAGE_SIZE))
  const pageItems = visible.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  const openView = (item: Violation) => {
    setSelected(item)
    setForm({ ...item })
    setIsEditing(false)
    setModal("view")
  }

  const closeModal = () => {
    setModal(null)
    setSelected(null)
    setIsEditing(false)
    setForm(emptyForm)
  }

  const save = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!form.contractId || !form.content.trim() || !form.date || !form.dueDate || form.penaltyAmount < 0) return
    setSaving(true)
    setError(null)
    try {
      if (modal === "view" && selected) {
        const updated = { ...selected, content: form.content.trim(), penaltyAmount: Number(form.penaltyAmount), date: form.date, dueDate: form.dueDate, resolved: form.resolved }
        setViolations((current) => current.map((item) => item.id === updated.id ? updated : item))
        closeModal()
      } else if (modal === "add") {
        const created: Violation = { id: newMockId(), ...form, tenant: getMockTenant(form.contractId) }
        setViolations((current) => [created, ...current])
        setPage(1)
        closeModal()
      }
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Không lưu được mock vi phạm.")
    } finally {
      setSaving(false)
    }
  }

  const hardDelete = (item: Violation) => {
    if (item.resolved || readOnly) return
    if (!window.confirm("Xóa mock vi phạm chưa xử lý này? Thao tác không thể hoàn tác.")) return
    setViolations((current) => current.filter((violation) => violation.id !== item.id))
    setPage((current) => Math.min(current, Math.max(1, Math.ceil((visible.length - 1) / PAGE_SIZE))))
    closeModal()
  }

  const openAdd = () => {
    setSelected(null)
    setForm({ ...emptyForm, contractId: MOCK_CONTRACTS[0]?.contractId ?? "", tenant: getMockTenant(MOCK_CONTRACTS[0]?.contractId ?? ""), date: new Date().toISOString().slice(0, 10), dueDate: new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10) })
    setIsEditing(true)
    setModal("add")
  }

  const disabled = readOnly || (modal === "view" && !isEditing)

  return <div>
    <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
      <div><h1 className="text-xl font-bold text-gray-900" style={{ fontFamily: "DM Sans" }}>{readOnly ? "Vi phạm của tôi" : "Vi phạm hợp đồng"}</h1><p className="mt-1 text-sm text-gray-400">{violations.filter((item) => !item.resolved).length} vi phạm chưa xử lý · {violations.length} mock records</p></div>
      {!readOnly && <button onClick={openAdd} className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700">+ Ghi nhận vi phạm</button>}
    </div>

    {error && <p role="alert" className="mb-4 rounded-lg border border-red-100 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>}
    <div className="mb-4 flex w-fit gap-1 rounded-lg bg-slate-100 p-1">
      {(["all", "unresolved", "resolved"] as const).map((item) => <button key={item} onClick={() => { setFilter(item); setPage(1) }} className={`rounded-md px-3 py-1.5 text-xs font-medium ${filter === item ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}>{item === "all" ? "Tất cả" : item === "unresolved" ? "Chưa xử lý" : "Đã xử lý"}</button>)}
    </div>

    <div className="overflow-x-auto rounded-xl border border-gray-100 bg-white">
      <table className="w-full min-w-[1000px] text-sm">
        <thead className="border-b border-gray-100 bg-gray-50 text-left text-xs uppercase tracking-wide text-gray-500"><tr>
          <th className="px-4 py-3 font-medium">Mã vi phạm</th><th className="px-4 py-3 font-medium">Mã hợp đồng</th><th className="px-4 py-3 font-medium">Khách thuê</th><th className="px-4 py-3 font-medium">Nội dung</th><th className="px-4 py-3 font-medium">Ngày vi phạm</th><th className="px-4 py-3 font-medium">Hạn xử lý</th><th className="px-4 py-3 text-right font-medium">Tiền phạt</th><th className="px-4 py-3 font-medium">Trạng thái</th>
        </tr></thead>
        <tbody className="divide-y divide-gray-50">
          {pageItems.map((item) => <tr key={item.id} tabIndex={0} onClick={() => openView(item)} onKeyDown={(event) => { if (event.key === "Enter") openView(item) }} className="cursor-pointer hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none" title="Bấm để xem chi tiết">
            <td className="max-w-40 truncate px-4 py-3 font-mono text-xs text-gray-500">{item.id}</td><td className="max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-600">{item.contractId}</td><td className="px-4 py-3 font-medium text-gray-800">{item.tenant || "—"}</td><td className="max-w-64 truncate px-4 py-3 text-gray-600" title={item.content}>{item.content}</td><td className="whitespace-nowrap px-4 py-3">{item.date}</td><td className="whitespace-nowrap px-4 py-3">{item.dueDate}</td><td className="whitespace-nowrap px-4 py-3 text-right font-mono">{fmt(item.penaltyAmount)}</td><td className="px-4 py-3"><span className={`rounded-full px-2.5 py-1 text-xs font-medium ${item.resolved ? "bg-emerald-50 text-emerald-700" : "bg-red-50 text-red-700"}`}>{item.resolved ? "Đã xử lý" : "Chưa xử lý"}</span></td>
          </tr>)}
          {pageItems.length === 0 && <tr><td colSpan={8} className="px-4 py-10 text-center text-gray-400">Không có vi phạm.</td></tr>}
        </tbody>
      </table>
      <div className="flex items-center justify-between border-t border-gray-100 px-4 py-3 text-xs text-gray-500">
        <span>{visible.length ? `${(page - 1) * PAGE_SIZE + 1}–${Math.min(page * PAGE_SIZE, visible.length)} / ${visible.length}` : "0 bản ghi"}</span>
        <div className="flex items-center gap-2"><button disabled={page <= 1} onClick={() => setPage((current) => current - 1)} className="rounded-md border border-gray-200 px-3 py-1.5 disabled:cursor-not-allowed disabled:opacity-40">Trước</button><span>Trang {page} / {pageCount}</span><button disabled={page >= pageCount} onClick={() => setPage((current) => current + 1)} className="rounded-md border border-gray-200 px-3 py-1.5 disabled:cursor-not-allowed disabled:opacity-40">Sau</button></div>
      </div>
    </div>

    {modal && <Modal title={modal === "add" ? "Ghi nhận vi phạm" : "Chi tiết vi phạm"} onClose={closeModal}>
      <form onSubmit={save}>
        {modal === "view" && selected && <FormField label="Mã vi phạm"><input className={inputCls} value={selected.id} disabled /></FormField>}
        <FormField label="Mã hợp đồng"><select required className={selectCls} value={form.contractId} disabled={modal === "view" || readOnly} onChange={(event) => { const contractId = event.target.value; setForm({ ...form, contractId, tenant: getMockTenant(contractId) }) }}>{MOCK_CONTRACTS.map((contract) => <option key={contract.contractId} value={contract.contractId}>{contract.contractId}</option>)}</select></FormField>
        <FormField label="Khách thuê"><input className={inputCls} value={form.tenant} disabled /></FormField>
        <FormField label="Ngày vi phạm"><input required type="date" className={inputCls} value={form.date} disabled={disabled} onChange={(event) => setForm({ ...form, date: event.target.value })} /></FormField>
        <FormField label="Hạn xử lý"><input required type="date" className={inputCls} value={form.dueDate} disabled={disabled} onChange={(event) => setForm({ ...form, dueDate: event.target.value })} /></FormField>
        <FormField label="Nội dung vi phạm"><textarea required maxLength={150} rows={3} className={inputCls} value={form.content} disabled={disabled} onChange={(event) => setForm({ ...form, content: event.target.value })} /></FormField>
        <FormField label="Tiền phạt (₫)"><input type="number" min="0" className={inputCls} value={form.penaltyAmount} disabled={disabled} onChange={(event) => setForm({ ...form, penaltyAmount: Number(event.target.value) })} /></FormField>
        {modal === "view" && <FormField label="Trạng thái"><select className={selectCls} value={form.resolved ? "resolved" : "open"} disabled={disabled} onChange={(event) => setForm({ ...form, resolved: event.target.value === "resolved" })}><option value="open">Chưa xử lý</option><option value="resolved">Đã xử lý</option></select></FormField>}
        <div className="mt-6 flex flex-wrap justify-end gap-2 border-t border-gray-100 pt-4">
          {modal === "add" ? <><button type="button" onClick={closeModal} className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Hủy</button><button type="submit" disabled={saving || !form.contractId || !form.content.trim()} className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50">{saving ? "Đang lưu..." : "Ghi nhận"}</button></> : readOnly ? <button type="button" onClick={closeModal} className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Đóng</button> : isEditing ? <><button type="button" onClick={() => { if (selected) setForm({ ...selected }); setIsEditing(false) }} className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Hủy</button><button type="submit" disabled={saving} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50">{saving ? "Đang lưu..." : "Lưu thay đổi"}</button></> : <><button type="button" onClick={closeModal} className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Đóng</button>{selected && !selected.resolved && <button type="button" disabled={saving} onClick={() => hardDelete(selected)} className="rounded-lg border border-red-200 px-4 py-2 text-sm font-medium text-red-600 disabled:opacity-50">Xóa</button>}<button type="button" onClick={() => setIsEditing(true)} className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white">Chỉnh sửa</button></>}
        </div>
      </form>
    </Modal>}
  </div>
}
