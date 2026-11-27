import type {SubmitEvent} from "react"
import {useEffect, useMemo, useState} from "react"
import {contractApi} from "@/api/contracts"
import type {ContractModel, ContractViolationModel} from "@/types/contracts"
import {BaseTable, FormField, inputCls, Modal, ModulePageHeader, ProcessingForm, selectCls} from "./shared"
import {fmt, isIntegerAmountInput} from "@/scripts/format.ts"
import type {Violation} from "@/types/modules.ts"
import {filterViolationsForAccount, mapContractViolationToView} from "@/scripts/violations.ts"
import type {ViolationFilter, ViolationForm, ViolationsModuleProps} from "@/types/violations.ts"

const emptyForm: ViolationForm = {
    contractId: "",
    tenant: "",
    content: "",
    penaltyAmount: 0,
    date: "",
    dueDate: "",
    resolved: false
}

export function ViolationsModule({
                                     accountId,
                                     readOnly = false,
                                     canCreate = !readOnly,
                                     canUpdate = !readOnly,
                                     ownOnly = readOnly
                                 }: ViolationsModuleProps) {
    const [contracts, setContracts] = useState<ContractModel[]>([])
    const [violations, setViolations] = useState<Violation[]>([])
    const [isLoading, setIsLoading] = useState(true)
    const [modal, setModal] = useState<"add" | "view" | null>(null)
    const [selected, setSelected] = useState<Violation | null>(null)
    const [isEditing, setIsEditing] = useState(false)
    const [form, setForm] = useState<ViolationForm>(emptyForm)
    const [filter, setFilter] = useState<ViolationFilter>("all")
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        const controller = new AbortController()
        const loadPages = async <T,>(loadPage: (cursor: string | null, signal: AbortSignal) => Promise<{
            items: T[]; nextCursor: string | null; isHasMore: boolean
        }>) => {
            const items: T[] = []
            let cursor: string | null = null
            do {
                const page = await loadPage(cursor, controller.signal)
                items.push(...page.items)
                cursor = page.isHasMore ? page.nextCursor : null
            } while (cursor && !controller.signal.aborted)
            return items
        }
        void Promise.all([
            loadPages((cursor, signal) => contractApi.contracts(cursor, signal)),
            loadPages((cursor, signal) => contractApi.violations(cursor, undefined, signal)),
        ]).then(([contractItems, violationItems]) => {
            if (controller.signal.aborted) return
            setContracts(contractItems)
            const contractById = new Map(contractItems.map((contract) => [contract.contractId, contract]))
            const mapped = violationItems.map((item: ContractViolationModel) =>
                mapContractViolationToView(item, contractById.get(item.contractId)?.contractAccountId ?? "—"),
            )
            setViolations(filterViolationsForAccount(mapped, contractItems, accountId, ownOnly))
        }).catch((cause: unknown) => {
            if (!controller.signal.aborted) setError(cause instanceof Error ? cause.message : "Không tải được danh sách vi phạm.")
        }).finally(() => {
            if (!controller.signal.aborted) setIsLoading(false)
        })
        return () => controller.abort()
    }, [accountId, ownOnly])

    const getTenant = (contractId: string) => contracts.find((contract) => contract.contractId === contractId)?.contractAccountId ?? "—"

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

    const save = async (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        if (!form.contractId || !form.content.trim() || !form.date || !form.dueDate || form.penaltyAmount < 0) return
        setSaving(true)
        setError(null)
        try {
            if (modal === "view" && selected) {
                const result = await contractApi.updateViolation(selected.id, {
                    contractId: form.contractId,
                    violationContent: form.content.trim(),
                    violationPenaltyAmount: Number(form.penaltyAmount),
                    violationDate: form.date,
                    violationDueDate: form.dueDate,
                    violationIsResolved: form.resolved,
                })
                const updated = mapContractViolationToView(result, getTenant(result.contractId))
                setViolations((current) => current.map((item) => item.id === updated.id ? updated : item))
                closeModal()
            } else if (modal === "add") {
                const result = await contractApi.createViolation({
                    contractId: form.contractId,
                    violationContent: form.content.trim(),
                    violationPenaltyAmount: Number(form.penaltyAmount),
                    violationDate: form.date,
                    violationDueDate: form.dueDate,
                })
                const created = mapContractViolationToView(result, getTenant(result.contractId))
                setViolations((current) => [created, ...current])
                closeModal()
            }
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Không lưu được vi phạm.")
        } finally {
            setSaving(false)
        }
    }

    const hardDelete = async (item: Violation) => {
        if (item.resolved || !canUpdate) return
        if (!window.confirm("Xóa vi phạm chưa xử lý này? Thao tác không thể hoàn tác.")) return
        setSaving(true)
        setError(null)
        try {
            await contractApi.deleteViolation(item.id)
            setViolations((current) => current.filter((violation) => violation.id !== item.id))
            closeModal()
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Không thể xóa vi phạm.")
        } finally {
            setSaving(false)
        }
    }

    const openAdd = () => {
        setSelected(null)
        setForm({
            ...emptyForm,
            contractId: contracts[0]?.contractId ?? "",
            tenant: contracts[0]?.contractAccountId ?? "",
            date: new Date().toISOString().slice(0, 10),
            dueDate: new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10)
        })
        setIsEditing(true)
        setModal("add")
    }

    const disabled = readOnly || (modal === "view" && !isEditing)

    return <div>
        <ModulePageHeader
            title={ownOnly ? "Vi phạm của tôi" : "Vi phạm hợp đồng"}
            description={`${violations.filter((item) => !item.resolved).length} vi phạm chưa xử lý`}
        >
            {canCreate && <button onClick={openAdd}
                                  className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700">+
                Ghi nhận vi phạm</button>}
        </ModulePageHeader>

        {error && <p role="alert"
                     className="mb-4 rounded-lg border border-red-100 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>}
        {isLoading && <p className="mb-4 text-sm text-gray-500">Đang tải dữ liệu vi phạm…</p>}
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
                {
                    key: "id",
                    header: "Mã vi phạm",
                    cellClassName: "max-w-40 truncate px-4 py-3 font-mono text-xs text-gray-500",
                    render: (item) => item.id
                },
                {
                    key: "contract",
                    header: "Mã hợp đồng",
                    cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-600",
                    render: (item) => item.contractId
                },
                {
                    key: "tenant",
                    header: "Khách thuê",
                    cellClassName: "px-4 py-3 font-medium text-gray-800",
                    render: (item) => item.tenant || "—"
                },
                {
                    key: "content",
                    header: "Nội dung",
                    cellClassName: "max-w-64 truncate px-4 py-3 text-gray-600",
                    render: (item) => <span title={item.content}>{item.content}</span>
                },
                {
                    key: "date",
                    header: "Ngày vi phạm",
                    cellClassName: "whitespace-nowrap px-4 py-3",
                    render: (item) => item.date
                },
                {
                    key: "dueDate",
                    header: "Hạn xử lý",
                    cellClassName: "whitespace-nowrap px-4 py-3",
                    render: (item) => item.dueDate
                },
                {
                    key: "amount",
                    header: "Tiền phạt",
                    headerClassName: "px-4 py-3 text-right font-medium",
                    cellClassName: "whitespace-nowrap px-4 py-3 text-right font-mono",
                    render: (item) => fmt(item.penaltyAmount)
                },
                {
                    key: "status",
                    header: "Trạng thái",
                    render: (item) => <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${item.resolved ? "bg-emerald-50 text-emerald-700" : "bg-red-50 text-red-700"}`}>{item.resolved ? "Đã xử lý" : "Chưa xử lý"}</span>
                },
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
                    setForm({...form, contractId, tenant: getTenant(contractId)})
                }}>{contracts.map((contract) => <option key={contract.contractId}
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
                                                            })
                                                        }}/></FormField>
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
                    </> : !canUpdate ? <button type="button" onClick={closeModal}
                                               className="rounded-lg border border-gray-200 px-4 py-2 text-sm text-gray-600">Đóng</button> : isEditing ? <>
                        <button type="button" disabled={!canUpdate} onClick={() => {
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
                        {selected && !selected.resolved && canUpdate &&
                            <button type="button" disabled={saving} onClick={() => hardDelete(selected)}
                                    className="rounded-lg border border-red-200 px-4 py-2 text-sm font-medium text-red-600 disabled:opacity-50">Xóa</button>}
                        {canUpdate && <button type="button" onClick={() => setIsEditing(true)}
                                              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white">Chỉnh
                            sửa
                        </button>}
                    </>}
                </div>
            </ProcessingForm>
        </Modal>}
    </div>
}
