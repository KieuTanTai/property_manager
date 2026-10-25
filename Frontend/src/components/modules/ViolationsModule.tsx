import type {SubmitEvent} from "react"
import {useMemo, useState} from "react"
import {MOCK_CONTRACTS} from "../../mocks/contracts"
import {getMockTenant, MOCK_VIOLATIONS} from "../../mocks/violations"
import {BaseTable, FormField, inputCls, Modal, ProcessingForm, selectCls} from "./shared"
import {fmt, isIntegerAmountInput} from "../../scripts/format"
import type {Violation} from "../../types/modules"
import {filterViolationsForAccount} from "../../scripts/violations"
import {createUuid} from "../../scripts/ids"
import type {ViolationFilter, ViolationForm, ViolationsModuleProps} from "../../types/violations"

const emptyForm: ViolationForm = {
    contractId: "",
    tenant: "",
    content: "",
    penaltyAmount: 0,
    date: "",
    dueDate: "",
    resolved: false
}

const newMockId = createUuid

export function ViolationsModule({accountId, readOnly = false}: ViolationsModuleProps) {
    const initialMockViolations = filterViolationsForAccount(MOCK_VIOLATIONS, MOCK_CONTRACTS, accountId, readOnly)
    const [violations, setViolations] = useState<Violation[]>(initialMockViolations)
    const [modal, setModal] = useState<"add" | "view" | null>(null)
    const [selected, setSelected] = useState<Violation | null>(null)
    const [isEditing, setIsEditing] = useState(false)
    const [form, setForm] = useState<ViolationForm>(emptyForm)
    const [filter, setFilter] = useState<ViolationFilter>("all")
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const visible = useMemo(() => violations.filter((item) =>
        filter === "all" || item.resolved === (filter === "resolved"),
    ), [filter, violations])

    const openView = (item: Violation) => {
        setSelected(item)
        setForm({...item})
        setIsEditing(false)
        setModal("view")
    }

    const closeModal = () => {
        setModal(null)
        setSelected(null)
        setIsEditing(false)
        setForm(emptyForm)
    }

    const save = (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        if (!form.contractId || !form.content.trim() || !form.date || !form.dueDate || form.penaltyAmount < 0) return
        setSaving(true)
        setError(null)
        try {
            if (modal === "view" && selected) {
                const updated = {
                    ...selected,
                    content: form.content.trim(),
                    penaltyAmount: Number(form.penaltyAmount),
                    date: form.date,
                    dueDate: form.dueDate,
                    resolved: form.resolved
                }
                setViolations((current) => current.map((item) => item.id === updated.id ? updated : item))
                closeModal()
            } else if (modal === "add") {
                const created: Violation = {id: newMockId(), ...form, tenant: getMockTenant(form.contractId)}
                setViolations((current) => [created, ...current])
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
        closeModal()
    }

    const openAdd = () => {
        setSelected(null)
        setForm({
            ...emptyForm,
            contractId: MOCK_CONTRACTS[0]?.contractId ?? "",
            tenant: getMockTenant(MOCK_CONTRACTS[0]?.contractId ?? ""),
            date: new Date().toISOString().slice(0, 10),
            dueDate: new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10)
        })
        setIsEditing(true)
        setModal("add")
    }

    const disabled = readOnly || (modal === "view" && !isEditing)

    return <div>
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
            <div><h1 className="text-xl font-bold text-gray-900"
                     style={{fontFamily: "DM Sans"}}>{readOnly ? "Vi phạm của tôi" : "Vi phạm hợp đồng"}</h1><p
                className="mt-1 text-sm text-gray-400">{violations.filter((item) => !item.resolved).length} vi phạm chưa
                xử lý · {violations.length} mock records</p></div>
            {!readOnly && <button onClick={openAdd}
                                  className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700">+
                Ghi nhận vi phạm</button>}
        </div>

        {error && <p role="alert"
                     className="mb-4 rounded-lg border border-red-100 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>}
        <div className="mb-4 flex w-fit gap-1 rounded-lg bg-slate-100 p-1">
            {(["all", "unresolved", "resolved"] as const).map((item) => <button key={item} onClick={() => {
                setFilter(item);
            }}
                                                                                className={`rounded-md px-3 py-1.5 text-xs font-medium ${filter === item ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}>{item === "all" ? "Tất cả" : item === "unresolved" ? "Chưa xử lý" : "Đã xử lý"}</button>)}
        </div>

        <BaseTable
            rows={visible}
            getRowKey={(item) => item.id}
            emptyMessage="Không có vi phạm."
            tableClassName="w-full min-w-[1000px] text-sm"
            paginationResetKey={filter}
            rowClassName={() => "hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none"}
            rowTitle="Bấm để xem chi tiết"
            onRowClick={openView}
            columns={[
                {key: "id", header: "Mã vi phạm", cellClassName: "max-w-40 truncate px-4 py-3 font-mono text-xs text-gray-500", render: (item) => item.id},
                {key: "contract", header: "Mã hợp đồng", cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-600", render: (item) => item.contractId},
                {key: "tenant", header: "Khách thuê", cellClassName: "px-4 py-3 font-medium text-gray-800", render: (item) => item.tenant || "—"},
                {key: "content", header: "Nội dung", cellClassName: "max-w-64 truncate px-4 py-3 text-gray-600", render: (item) => <span title={item.content}>{item.content}</span>},
                {key: "date", header: "Ngày vi phạm", cellClassName: "whitespace-nowrap px-4 py-3", render: (item) => item.date},
                {key: "dueDate", header: "Hạn xử lý", cellClassName: "whitespace-nowrap px-4 py-3", render: (item) => item.dueDate},
                {key: "amount", header: "Tiền phạt", headerClassName: "px-4 py-3 text-right font-medium", cellClassName: "whitespace-nowrap px-4 py-3 text-right font-mono", render: (item) => fmt(item.penaltyAmount)},
                {key: "status", header: "Trạng thái", render: (item) => <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${item.resolved ? "bg-emerald-50 text-emerald-700" : "bg-red-50 text-red-700"}`}>{item.resolved ? "Đã xử lý" : "Chưa xử lý"}</span>},
            ]}
        />

        {modal && <Modal title={modal === "add" ? "Ghi nhận vi phạm" : "Chi tiết vi phạm"} onClose={closeModal}>
            <ProcessingForm processing={saving} onSubmit={save}>
                {modal === "view" && selected &&
                    <FormField label="Mã vi phạm"><input className={inputCls} value={selected.id}
                                                         disabled/></FormField>}
                <FormField label="Mã hợp đồng"><select required className={selectCls} value={form.contractId}
                                                       disabled={modal === "view" || readOnly} onChange={(event) => {
                    const contractId = event.target.value;
                    setForm({...form, contractId, tenant: getMockTenant(contractId)})
                }}>{MOCK_CONTRACTS.map((contract) => <option key={contract.contractId}
                                                             value={contract.contractId}>{contract.contractId}</option>)}</select></FormField>
                <FormField label="Khách thuê"><input className={inputCls} value={form.tenant} disabled/></FormField>
                <FormField label="Ngày vi phạm"><input required type="date" className={inputCls} value={form.date}
                                                       disabled={disabled} onChange={(event) => setForm({
                    ...form,
                    date: event.target.value
                })}/></FormField>
                <FormField label="Hạn xử lý"><input required type="date" className={inputCls} value={form.dueDate}
                                                    disabled={disabled} onChange={(event) => setForm({
                    ...form,
                    dueDate: event.target.value
                })}/></FormField>
                <FormField label="Nội dung vi phạm"><textarea required maxLength={150} rows={3} className={inputCls}
                                                              value={form.content} disabled={disabled}
                                                              onChange={(event) => setForm({
                                                                  ...form,
                                                                  content: event.target.value
                                                              })}/></FormField>
                <FormField label="Tiền phạt (₫)"><input type="number" min="0" step="1" className={inputCls}
                                                        value={form.penaltyAmount} disabled={disabled}
                                                        onChange={(event) => {
                                                            if (!isIntegerAmountInput(event.target.value)) return
                                                            setForm({
                                                            ...form,
                                                            penaltyAmount: event.target.value === "" ? 0 : Number(event.target.value)
                                                        })}}/></FormField>
                {modal === "view" && <FormField label="Trạng thái"><select className={selectCls}
                                                                           value={form.resolved ? "resolved" : "open"}
                                                                           disabled={disabled}
                                                                           onChange={(event) => setForm({
                                                                               ...form,
                                                                               resolved: event.target.value === "resolved"
                                                                           })}>
                    <option value="open">Chưa xử lý</option>
                    <option value="resolved">Đã xử lý</option>
                </select></FormField>}
                <div className="mt-6 flex flex-wrap justify-end gap-2 border-t border-gray-100 pt-4">
                    {modal === "add" ? <>
                        <button type="button" onClick={closeModal}
                                className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Hủy
                        </button>
                        <button type="submit" disabled={saving || !form.contractId || !form.content.trim()}
                                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50">{saving ? "Đang lưu..." : "Ghi nhận"}</button>
                    </> : readOnly ? <button type="button" onClick={closeModal}
                                             className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Đóng</button> : isEditing ? <>
                        <button type="button" onClick={() => {
                            if (selected) setForm({...selected});
                            setIsEditing(false)
                        }} className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Hủy
                        </button>
                        <button type="submit" disabled={saving}
                                className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50">{saving ? "Đang lưu..." : "Lưu thay đổi"}</button>
                    </> : <>
                        <button type="button" onClick={closeModal}
                                className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Đóng
                        </button>
                        {selected && !selected.resolved &&
                            <button type="button" disabled={saving} onClick={() => hardDelete(selected)}
                                    className="rounded-lg border border-red-200 px-4 py-2 text-sm font-medium text-red-600 disabled:opacity-50">Xóa</button>}
                        <button type="button" onClick={() => setIsEditing(true)}
                                className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white">Chỉnh sửa
                        </button>
                    </>}
                </div>
            </ProcessingForm>
        </Modal>}
    </div>
}
