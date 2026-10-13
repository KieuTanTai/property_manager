import { useEffect, useState } from "react"
import type { FormEvent } from "react"
import { contractApi } from "../../api/contracts"
import { MOCK_CONTRACTS, MOCK_REGULATIONS } from "../../mocks/contracts"
import type { ContractModel, ContractStatus as ApiContractStatus, ContractViolationModel, MonthlyInvoiceModel, RegulationModel } from "../../api/contracts"
import { ContractStatusBadge, FormField, Modal, inputCls, selectCls } from "./shared"
import type { ContractStatus } from "../../types/modules"
import { toDateInput, toUiContractStatus } from "../../scripts/contracts"
import { fmt } from "../../scripts/format"
import type { ContractForm, ContractsModuleProps } from "../../types/contracts"

const emptyForm: ContractForm = { accountId: "", deposit: "", rentalPrice: "", premiseReturnDate: "", status: "PendingApproval", terminationDate: "", regulationIds: [] }
const statuses: ApiContractStatus[] = ["PendingApproval", "PendingSignature", "Signed", "Expired", "Terminated", "Cancelled"]
const statusLabel: Record<ApiContractStatus, string> = { PendingApproval: "Chờ duyệt", PendingSignature: "Chờ ký", Cancelled: "Đã hủy", Expired: "Hết hạn", Signed: "Đã ký", Terminated: "Đã chấm dứt" }
const button = "rounded-lg px-4 py-2 text-sm font-medium transition-colors disabled:opacity-50"

export function ContractsModule({ accountId, readOnly = false }: ContractsModuleProps) {
  const [contracts, setContracts] = useState<ContractModel[]>(MOCK_CONTRACTS)
  const [regulations, setRegulations] = useState<RegulationModel[]>(MOCK_REGULATIONS)
  const [filter, setFilter] = useState<ContractStatus | "all">("all")
  const [selected, setSelected] = useState<ContractModel | null>(null)
  const [modal, setModal] = useState<"add" | "detail" | null>(null)
  const [editing, setEditing] = useState(false)
  const [form, setForm] = useState<ContractForm>(emptyForm)
  const [loading, setLoading] = useState(readOnly)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)

  useEffect(() => {
    if (!readOnly) return
    let cancelled = false
    const loadOwnContracts = async () => {
      setLoading(true)
      try {
        const all: ContractModel[] = []
        let cursor: string | null = null
        let hasMore = true
        while (hasMore) {
          const page = await contractApi.contracts(cursor)
          all.push(...page.items)
          cursor = page.nextCursor
          hasMore = page.isHasMore
        }
        if (!cancelled) setContracts(all.filter((item) => item.contractAccountId === accountId))
      } catch (cause) {
        if (!cancelled) setError(cause instanceof Error ? cause.message : "Không tải được hợp đồng của tài khoản.")
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    void loadOwnContracts()
    return () => { cancelled = true }
  }, [accountId, readOnly])

  // Tạm dùng fixtures vì Contract API chưa có dữ liệu local.
  // Khi bật backend, bỏ comment để nạp response và regulations từ API:
  // useEffect(() => {
  //   Promise.all([contractApi.contracts(null), contractApi.regulations()])
  //     .then(([page, regulationPage]) => {
  //       setContracts(page.items)
  //       setRegulations(regulationPage.items)
  //     })
  //     .catch((cause) => setError(cause instanceof Error ? cause.message : "Không tải được hợp đồng."))
  // }, [])

  const visibleContracts = contracts
  const filtered = visibleContracts.filter((item) => filter === "all" || toUiContractStatus(item.contractStatus) === filter)
  const openCreate = () => { setForm({ ...emptyForm }); setEditing(false); setModal("add") }
  const openDetail = (item: ContractModel) => { setSelected(item); setEditing(false); setModal("detail") }
  const startEdit = () => {
    if (!selected) return
    setForm({ accountId: selected.contractAccountId, deposit: String(selected.contractDeposit), rentalPrice: String(selected.contractRentalPrice), premiseReturnDate: toDateInput(selected.contractPremiseReturnDate), status: selected.contractStatus, terminationDate: toDateInput(selected.contractTerminationDate ?? ""), regulationIds: selected.contractRegulations.map((regulation) => regulation.regulationId) })
    setEditing(true)
  }
  const toggleRegulation = (id: string) => setForm((current) => ({ ...current, regulationIds: current.regulationIds.includes(id) ? current.regulationIds.filter((item) => item !== id) : [...current.regulationIds, id] }))
  const save = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSaving(true); setError(null); setNotice(null)
    try {
      const base = { accountId: form.accountId.trim(), deposit: Number(form.deposit), rentalPrice: Number(form.rentalPrice), premiseReturnDate: new Date(`${form.premiseReturnDate}T00:00:00`).toISOString(), regulationIds: form.regulationIds }
      const selectedRegulations = regulations.filter((item) => base.regulationIds.includes(item.regulationId))
      const now = new Date().toISOString()
      const saved: {
          contractId: string;
          contractAccountId: string;
          contractDeposit: number;
          contractRentalPrice: number;
          contractPremiseReturnDate: string;
          contractStatus: "PendingApproval" | "PendingSignature" | "Cancelled" | "Expired" | "Signed" | "Terminated";
          contractTerminationDate: string | null;
          contractCreatedAt: string;
          contractUpdatedAt: string;
          contractViolations: ContractViolationModel[];
          contractInvoices: MonthlyInvoiceModel[];
          contractRegulations: RegulationModel[]
      } | null = modal === "add" ? {
        contractId: crypto.randomUUID(), contractAccountId: base.accountId,
        contractDeposit: base.deposit, contractRentalPrice: base.rentalPrice,
        contractPremiseReturnDate: base.premiseReturnDate, contractStatus: "PendingApproval",
        contractTerminationDate: null, contractCreatedAt: now, contractUpdatedAt: now,
        contractViolations: [], contractInvoices: [], contractRegulations: selectedRegulations,
      } : selected ? {
        ...selected, contractAccountId: base.accountId, contractDeposit: base.deposit,
        contractRentalPrice: base.rentalPrice, contractPremiseReturnDate: base.premiseReturnDate,
        contractStatus: form.status,
        contractTerminationDate: form.terminationDate ? new Date(`${form.terminationDate}T00:00:00`).toISOString() : null,
        contractUpdatedAt: now, contractRegulations: selectedRegulations,
      } : null
      if (!saved) return
      // API write requests for the live-data mode:
      // modal === "add" ? await contractApi.createContract(base) : await contractApi.updateContract(selected!.contractId, { ...base, status: form.status, terminationDate: saved.contractTerminationDate })
      setContracts((current) => modal === "add" ? [saved, ...current] : current.map((item) => item.contractId === saved.contractId ? saved : item))
      setNotice(modal === "add" ? "Đã tạo hợp đồng." : "Đã cập nhật hợp đồng.")
      setModal(null); setSelected(null); setEditing(false)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Không lưu được hợp đồng.")
    } finally { setSaving(false) }
  }

  const valid = form.accountId.trim().length > 0 && Number(form.deposit) > 0 && Number(form.rentalPrice) > 0 && Boolean(form.premiseReturnDate)
  const renderForm = (edit: boolean) => <>
    <div className="grid gap-x-4 sm:grid-cols-2">
      <FormField label="Account ID"><input className={inputCls} required value={form.accountId} onChange={(event) => setForm({ ...form, accountId: event.target.value })} placeholder="GUID tài khoản" /></FormField>
      <FormField label="Ngày hoàn trả mặt bằng"><input className={inputCls} required type="date" value={form.premiseReturnDate} onChange={(event) => setForm({ ...form, premiseReturnDate: event.target.value })} /></FormField>
      <FormField label="Tiền đặt cọc (VND)"><input className={inputCls} required type="number" min="0.01" value={form.deposit} onChange={(event) => setForm({ ...form, deposit: event.target.value })} /></FormField>
      <FormField label="Giá thuê (VND)"><input className={inputCls} required type="number" min="0.01" value={form.rentalPrice} onChange={(event) => setForm({ ...form, rentalPrice: event.target.value })} /></FormField>
    </div>
    {edit && <div className="grid gap-x-4 sm:grid-cols-2"><FormField label="Trạng thái"><select className={selectCls} value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value as ApiContractStatus })}>{statuses.map((status) => <option key={status} value={status}>{statusLabel[status]}</option>)}</select></FormField><FormField label="Ngày chấm dứt"><input className={inputCls} type="date" value={form.terminationDate} onChange={(event) => setForm({ ...form, terminationDate: event.target.value })} /></FormField></div>}
    <FormField label="Quy định hợp đồng (RegulationIds)"><div className="max-h-48 space-y-1 overflow-y-auto rounded-lg border border-slate-200 p-2">{regulations.filter((item) => item.regulationIsActive).map((item) => <label key={item.regulationId} className="flex cursor-pointer items-start gap-2 rounded-md p-2 hover:bg-slate-50"><input type="checkbox" className="mt-1 accent-blue-600" checked={form.regulationIds.includes(item.regulationId)} onChange={() => toggleRegulation(item.regulationId)} /><span><span className="block text-sm text-slate-800">{item.regulationName}</span><span className="text-xs text-slate-500">{item.regulationDescription || item.regulationId}</span></span></label>)}{regulations.length === 0 && <p className="p-2 text-xs text-slate-500">Không có regulation active để chọn.</p>}</div></FormField>
  </>

  return <div>
    <div className="mb-5 flex items-center justify-between"><div><h1 className="text-xl font-bold text-slate-900">{readOnly ? "Hợp đồng của tôi" : "Quản lý hợp đồng"}</h1><p className="mt-1 text-sm text-slate-500">{readOnly ? `${visibleContracts.length} hợp đồng của tài khoản` : `Dữ liệu mẫu theo ContractModel · ${contracts.length} hợp đồng`}</p></div>{!readOnly && <button className={`${button} bg-blue-600 text-white hover:bg-blue-700`} onClick={openCreate}>+ Tạo hợp đồng</button>}</div>
    {error && <div role="alert" className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}{notice && <div role="status" className="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{notice}</div>}
    <div className="mb-4 flex flex-wrap gap-1 rounded-lg bg-slate-100 p-1 w-fit">{(["all", "pending_approval", "pending_signature", "signed", "expired", "terminated", "canceled"] as const).map((status) => <button key={status} className={`rounded-md px-3 py-1.5 text-xs ${filter === status ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`} onClick={() => setFilter(status)}>{status === "all" ? "Tất cả" : statusLabel[({ pending_approval: "PendingApproval", pending_signature: "PendingSignature", signed: "Signed", expired: "Expired", terminated: "Terminated", canceled: "Cancelled" } as const)[status]]}</button>)}</div>
    <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white"><table className="w-full min-w-[900px] text-sm"><thead className="bg-slate-50 text-xs text-slate-500"><tr><th className="px-4 py-3 text-left">Contract ID</th><th className="px-4 py-3 text-left">Account ID</th><th className="px-4 py-3 text-right">Đặt cọc</th><th className="px-4 py-3 text-right">Giá thuê</th><th className="px-4 py-3 text-center">Quy định</th><th className="px-4 py-3 text-left">Ngày hoàn trả</th><th className="px-4 py-3 text-left">Trạng thái</th></tr></thead><tbody className="divide-y divide-slate-100">{loading ? <tr><td colSpan={7} className="p-8 text-center text-slate-500">Đang tải...</td></tr> : filtered.map((item) => <tr key={item.contractId} onClick={() => openDetail(item)} className="cursor-pointer hover:bg-blue-50/50"><td className="max-w-48 truncate px-4 py-3 font-mono text-xs text-slate-500">{item.contractId}</td><td className="max-w-48 truncate px-4 py-3 font-mono text-xs text-slate-500">{item.contractAccountId}</td><td className="px-4 py-3 text-right">{fmt(item.contractDeposit)}</td><td className="px-4 py-3 text-right font-semibold">{fmt(item.contractRentalPrice)}</td><td className="px-4 py-3 text-center">{item.contractRegulations.length}</td><td className="px-4 py-3">{new Date(item.contractPremiseReturnDate).toLocaleDateString("vi-VN")}</td><td className="px-4 py-3"><ContractStatusBadge status={toUiContractStatus(item.contractStatus)} /></td></tr>)}{!loading && filtered.length === 0 && <tr><td colSpan={7} className="p-8 text-center text-slate-500">Không có hợp đồng.</td></tr>}</tbody></table><div className="border-t border-slate-100 p-3 text-xs text-slate-500">Đang dùng fixtures cục bộ; cursor query API được giữ ở comment phía trên.</div></div>
    {modal === "add" && <Modal title="Tạo hợp đồng mới" onClose={() => setModal(null)} wide><form onSubmit={(event) => void save(event)}>{renderForm(false)}<div className="flex justify-end gap-2"><button type="button" className={`${button} border border-slate-200`} onClick={() => setModal(null)}>Hủy</button><button className={`${button} bg-blue-600 text-white`} disabled={!valid || saving}>{saving ? "Đang lưu..." : "Tạo hợp đồng"}</button></div></form></Modal>}
    {modal === "detail" && selected && <Modal title={editing ? "Chỉnh sửa hợp đồng" : "Chi tiết hợp đồng"} onClose={() => setModal(null)} wide>{editing ? <form onSubmit={(event) => void save(event)}>{renderForm(true)}<div className="flex justify-end gap-2"><button type="button" className={`${button} border border-slate-200`} onClick={() => setEditing(false)}>Hủy</button><button className={`${button} bg-blue-600 text-white`} disabled={!valid || saving}>Lưu thay đổi</button></div></form> : <div><div className="mb-4 flex flex-wrap items-center justify-between gap-3"><div><p className="text-xs text-slate-500">Contract ID</p><p className="break-all font-mono text-sm">{selected.contractId}</p></div><ContractStatusBadge status={toUiContractStatus(selected.contractStatus)} /></div><div className="grid gap-3 sm:grid-cols-2"><div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Account ID</p><p className="mt-1 break-all font-mono text-xs">{selected.contractAccountId}</p></div><div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Ngày hoàn trả</p><p className="mt-1 text-sm">{new Date(selected.contractPremiseReturnDate).toLocaleDateString("vi-VN")}</p></div><div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Ngày tạo</p><p className="mt-1 text-sm">{new Date(selected.contractCreatedAt).toLocaleString("vi-VN")}</p></div><div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Ngày cập nhật</p><p className="mt-1 text-sm">{new Date(selected.contractUpdatedAt).toLocaleString("vi-VN")}</p></div></div><div className="mt-5 grid gap-4 md:grid-cols-2"><section><h3 className="mb-2 text-sm font-semibold">Regulations</h3><ul className="space-y-2 text-sm text-slate-600">{selected.contractRegulations.map((item) => <li key={item.regulationId} className="rounded-lg bg-slate-50 p-3"><span className="font-medium text-slate-800">{item.regulationName}</span>{item.regulationDescription && <span className="mt-1 block text-xs text-slate-500">{item.regulationDescription}</span>}</li>)}{selected.contractRegulations.length === 0 && <li className="text-xs text-slate-400">Chưa gắn quy định.</li>}</ul></section><section><h3 className="mb-2 flex items-center gap-2 text-sm font-semibold text-red-700">Violations <span className="rounded-full bg-red-100 px-2 py-0.5 text-xs">{selected.contractViolations.length}</span></h3><ul className="max-h-64 space-y-2 overflow-y-auto">{selected.contractViolations.map((item) => <li key={item.violationId} className={`rounded-lg border p-3 text-sm ${item.violationIsResolved ? "border-emerald-100 bg-emerald-50" : "border-red-100 bg-red-50"}`}><p className={`font-medium ${item.violationIsResolved ? "text-emerald-800" : "text-red-800"}`}>{item.violationContent}</p><div className={`mt-1 flex flex-wrap justify-between gap-2 text-xs ${item.violationIsResolved ? "text-emerald-700" : "text-red-700"}`}><span>{new Date(item.violationDate).toLocaleDateString("vi-VN")} · Phạt {fmt(item.violationPenaltyAmount ?? 0)}</span><span>{item.violationIsResolved ? "Đã xử lý" : "Chưa xử lý"}</span></div></li>)}{selected.contractViolations.length === 0 && <li className="text-xs text-slate-400">Chưa có vi phạm.</li>}</ul></section></div><div className="mt-5 flex justify-end gap-2"><button className={`${button} border border-slate-200`} onClick={() => setModal(null)}>Đóng</button>{!readOnly && <button className={`${button} bg-blue-600 text-white`} onClick={startEdit}>Chỉnh sửa</button>}</div></div>}</Modal>}
  </div>
}
