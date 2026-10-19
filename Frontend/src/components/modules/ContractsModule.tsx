import type {SubmitEvent} from "react"
import {useEffect, useState} from "react"
import {contractApi} from "@/api/contracts.ts"
import {identityApi} from "@/api/identity.ts"
import type {AccountProfileOption} from "@/types/identity.ts"
import type {
    ContractModel,
    ContractStatus,
    RegulationModel
} from "@/types/contracts.ts"

import {statuses, recordStatuses} from "@/types/contracts.ts";
import {BaseTable, ContractStatusBadge, FormField, formatProfileFullName, inputCls, Modal, ProcessingForm, selectCls} from "./shared"
import {toDateInput} from "@/scripts/contracts.ts"
import {fmt} from "@/scripts/format.ts"
import type {ContractForm, ContractsModuleProps} from "@/types/contracts.ts"

const emptyForm: ContractForm = {
    accountId: "",
    deposit: "",
    rentalPrice: "",
    premiseReturnDate: "",
    status: "PendingApproval",
    terminationDate: "",
    regulationIds: []
}
const button = "rounded-lg px-4 py-2 text-sm font-medium transition-colors disabled:opacity-50"

export function ContractsModule({accountId, readOnly = false}: ContractsModuleProps) {
    const [contracts, setContracts] = useState<ContractModel[]>([])
    const [regulations, setRegulations] = useState<RegulationModel[]>([])
    const [customerAccounts, setCustomerAccounts] = useState<AccountProfileOption[]>([])
    const [filter, setFilter] = useState<ContractStatus | "all">("all")
    const [selected, setSelected] = useState<ContractModel | null>(null)
    const [modal, setModal] = useState<"add" | "detail" | null>(null)
    const [editing, setEditing] = useState(false)
    const [form, setForm] = useState<ContractForm>(emptyForm)
    const [loading, setLoading] = useState(true)
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [notice, setNotice] = useState<string | null>(null)

    useEffect(() => {
        let active = true
        const load = async () => {
            setLoading(true)
            setError(null)
            try {
                const [allContracts, allRegulations] = await Promise.all([
                    (async () => {
                        const items: ContractModel[] = []
                        let cursor: string | null = null
                        do {
                            const page = await contractApi.contracts(cursor)
                            items.push(...page.items)
                            cursor = page.isHasMore ? page.nextCursor : null
                        } while (cursor)
                        return items
                    })(),
                    (async () => {
                        const items: RegulationModel[] = []
                        let cursor: string | null = null
                        do {
                            const page = await contractApi.regulations(cursor)
                            items.push(...page.items)
                            cursor = page.isHasMore ? page.nextCursor : null
                        } while (cursor)
                        return items
                    })(),
                ])
                if (active) {
                    const normalizedAccountId = accountId?.trim().toLowerCase()
                    setContracts(readOnly && normalizedAccountId
                        ? allContracts.filter((item) => item.contractAccountId.trim().toLowerCase() === normalizedAccountId)
                        : allContracts)
                    setRegulations(allRegulations)
                }
            } catch (cause) {
                if (active) setError(cause instanceof Error ? cause.message : "Không tải được hợp đồng.")
            } finally {
                if (active) setLoading(false)
            }
        }
        void load()
        return () => { active = false }
    }, [accountId, readOnly])

    useEffect(() => {
        let active = true
        const loadCustomerAccounts = async () => {
            try {
                const accounts = await identityApi.getAccountsByRoleCode("Customer")
                if (active) setCustomerAccounts(accounts)
            } catch (cause) {
                if (active) setError(cause instanceof Error ? cause.message : "Không tải được danh sách khách hàng.")
            }
        }
        void loadCustomerAccounts()
        return () => { active = false }
    }, [])

    const visibleContracts = contracts
    const filtered = visibleContracts.filter((item) => filter === "all" || item.contractStatus === filter)
    const selectedAccount = customerAccounts.find((item) => item.accountId.toLowerCase() === form.accountId.toLowerCase())
    const selectedAccountName = selectedAccount
        ? formatProfileFullName(selectedAccount.userProfile) || selectedAccount.accountEmail
        : ""
    const openCreate = () => {
        setForm({...emptyForm, regulationIds: regulations.filter((item) => item.regulationIsActive).map((item) => item.regulationId)});
        setEditing(false);
        setModal("add")
    }
    const openDetail = (item: ContractModel) => {
        setSelected(item);
        setEditing(false);
        setModal("detail")
    }
    const startEdit = () => {
        if (!selected) return
        setForm({
            accountId: selected.contractAccountId,
            deposit: String(selected.contractDeposit),
            rentalPrice: String(selected.contractRentalPrice),
            premiseReturnDate: toDateInput(selected.contractPremiseReturnDate),
            status: selected.contractStatus,
            terminationDate: toDateInput(selected.contractTerminationDate ?? ""),
            regulationIds: selected.contractRegulations.map((regulation) => regulation.regulationId)
        })
        setEditing(true)
    }
    const toggleRegulation = (id: string) => setForm((current) => ({
        ...current,
        regulationIds: current.regulationIds.includes(id) ? current.regulationIds.filter((item) => item !== id) : [...current.regulationIds, id]
    }))
    const save = async (event: SubmitEvent<HTMLFormElement>) => {
        event.preventDefault()
        setSaving(true);
        setError(null);
        setNotice(null)
        try {
            const base = {
                accountId: form.accountId.trim(),
                deposit: Number(form.deposit),
                rentalPrice: Number(form.rentalPrice),
                premiseReturnDate: new Date(`${form.premiseReturnDate}T00:00:00`).toISOString(),
                regulationIds: form.regulationIds
            }
            const saved = modal === "add"
                ? await contractApi.createContract(base)
                : selected ? await contractApi.updateContract(selected.contractId, {
                    ...base,
                    status: form.status,
                    terminationDate: form.terminationDate ? new Date(`${form.terminationDate}T00:00:00`).toISOString() : null,
                }) : null
            if (!saved) return
            setContracts((current) => modal === "add" ? [saved, ...current] : current.map((item) => item.contractId === saved.contractId ? saved : item))
            setNotice(modal === "add" ? "Đã tạo hợp đồng." : "Đã cập nhật hợp đồng.")
            setModal(null);
            setSelected(null);
            setEditing(false)
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Không lưu được hợp đồng.")
        } finally {
            setSaving(false)
        }
    }

    const valid = form.accountId.trim().length > 0 && Number(form.deposit) > 0 && Number(form.rentalPrice) > 0 && Boolean(form.premiseReturnDate)
    const renderForm = (edit: boolean) => <div className={!edit ? "grid gap-5 md:grid-cols-[minmax(0,0.8fr)_minmax(0,1.2fr)]" : ""}>
        {!edit && <aside className="rounded-xl border border-slate-200 bg-slate-50 p-4">
            <h3 className="mb-3 text-sm font-semibold text-slate-800">Thông tin khách hàng</h3>
            {selectedAccount ? <div className="space-y-3 text-sm">
                <div><p className="text-xs text-slate-500">Họ và tên</p><p className="font-medium text-slate-900">{selectedAccountName}</p></div>
                <div><p className="text-xs text-slate-500">Email</p><p className="break-all text-slate-800">{selectedAccount.accountEmail}</p></div>
                <div><p className="text-xs text-slate-500">Số điện thoại</p><p className="text-slate-800">{selectedAccount.userProfile?.userProfilePhoneNumber || "Chưa cập nhật"}</p></div>
                <div><p className="text-xs text-slate-500">Ngày sinh</p><p className="text-slate-800">{selectedAccount.userProfile?.userProfileDateOfBirth ? new Date(selectedAccount.userProfile.userProfileDateOfBirth).toLocaleDateString("vi-VN") : "Chưa cập nhật"}</p></div>
                <div><p className="text-xs text-slate-500">Giới tính</p><p className="text-slate-800">{selectedAccount.userProfile?.userProfileGender || "Chưa cập nhật"}</p></div>
                <div><p className="text-xs text-slate-500">Địa chỉ</p><p className="text-slate-800">{selectedAccount.userProfile?.userProfileAddress || "Chưa cập nhật"}</p></div>
            </div> : <p className="text-sm text-slate-500">Chọn account để xem hồ sơ khách hàng.</p>}
        </aside>}
        <div>
        <div className="grid gap-x-4 sm:grid-cols-2">
            <FormField label="Account ID">{edit ? <input className={inputCls} required value={form.accountId}
                                                 onChange={(event) => setForm({...form, accountId: event.target.value})}
                                                 placeholder="GUID tài khoản"/> : <select className={selectCls} required value={form.accountId}
                                                 onChange={(event) => setForm({...form, accountId: event.target.value})}
                                                 title={selectedAccountName || undefined}>
                <option value="">Chọn khách hàng</option>{customerAccounts.map((account) => {
                    const name = formatProfileFullName(account.userProfile) || account.accountEmail
                    return <option key={account.accountId} value={account.accountId} title={name}>{account.accountId}</option>
                })}</select>}</FormField>
            <FormField label="Ngày hoàn trả mặt bằng"><input className={inputCls} required type="date"
                                                             value={form.premiseReturnDate}
                                                             onChange={(event) => setForm({
                                                                 ...form,
                                                                 premiseReturnDate: event.target.value
                                                             })}/></FormField>
            <FormField label="Tiền đặt cọc (VND)"><input className={inputCls} required type="number" min="0.01"
                                                         value={form.deposit} onChange={(event) => setForm({
                ...form,
                deposit: event.target.value
            })}/></FormField>
            <FormField label="Giá thuê (VND)"><input className={inputCls} required type="number" min="0.01"
                                                     value={form.rentalPrice} onChange={(event) => setForm({
                ...form,
                rentalPrice: event.target.value
            })}/></FormField>
        </div>
        {edit &&
            <div className="grid gap-x-4 sm:grid-cols-2"><FormField label="Trạng thái"><select className={selectCls}
                                                                                               value={form.status}
                                                                                               onChange={(event) => setForm({
                                                                                                   ...form,
                                                                                                   status: event.target.value as ContractStatus
                                                                                               })}>{statuses.map((status) =>
                <option key={status} value={status}>{recordStatuses[status]}</option>)}</select></FormField><FormField
                label="Ngày chấm dứt"><input className={inputCls} type="date" value={form.terminationDate}
                                             onChange={(event) => setForm({
                                                 ...form,
                                                 terminationDate: event.target.value
                                             })}/></FormField></div>}
        <FormField label="Quy định hợp đồng (Regulations)">
            <div
                className="max-h-48 space-y-1 overflow-y-auto rounded-lg border border-slate-200 p-2">{regulations.filter((item) => item.regulationIsActive).map((item) =>
                <label key={item.regulationId}
                       className="flex cursor-pointer items-start gap-2 rounded-md p-2 hover:bg-slate-50">
                    <input
                    type="checkbox" className="mt-1 accent-blue-600"
                    checked={form.regulationIds.includes(item.regulationId)}
                    onChange={() => toggleRegulation(item.regulationId)}/><span><span
                    className="block text-sm text-slate-800">{item.regulationName}</span><span
                    className="text-xs text-slate-500">{item.regulationDescription || item.regulationId}</span></span></label>)}{regulations.length === 0 &&
                <p className="p-2 text-xs text-slate-500">Không có regulation active để chọn.</p>}</div>
        </FormField>
        </div>
        </div>

    return <div>
        <div className="mb-5 flex items-center justify-between">
            <div><h1
                className="text-xl font-bold text-slate-900">{readOnly ? "Hợp đồng của tôi" : "Quản lý hợp đồng"}</h1><p
                className="mt-1 text-sm text-slate-500">{visibleContracts.length} hợp đồng</p>
            </div>
            {!readOnly &&
                <button className={`${button} bg-blue-600 text-white hover:bg-blue-700`} onClick={openCreate}>+ Tạo hợp
                    đồng</button>}</div>
        {error && <div role="alert"
                       className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}{notice &&
        <div role="status"
             className="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{notice}</div>}
        <div
            className="mb-4 flex flex-wrap gap-1 rounded-lg bg-slate-100 p-1 w-fit">{(["all", ...statuses] as const).map((status) =>
            <button key={status}
                    className={`rounded-md px-3 py-1.5 text-xs ${filter === status ? "bg-white text-slate-900 shadow-sm" : "text-slate-500"}`}
                    onClick={() => setFilter(status)}>{status === "all" ? "Tất cả" : recordStatuses[status]}</button>)}</div>
        <BaseTable
            rows={filtered}
            getRowKey={(item) => item.contractId}
            loading={loading}
            emptyMessage="Không có hợp đồng."
            tableClassName="w-full min-w-225 text-sm"
            paginationResetKey={filter}
            onRowClick={openDetail}
            rowClassName={() => "hover:bg-blue-50/50"}
            columns={[
                {key: "contractId", header: "Contract ID", cellClassName: "max-w-48 truncate px-4 py-3 font-mono text-xs text-slate-500", render: (item) => item.contractId},
                {key: "accountId", header: "Account ID", cellClassName: "max-w-48 truncate px-4 py-3 font-mono text-xs text-slate-500", render: (item) => item.contractAccountId},
                {key: "deposit", header: "Đặt cọc", headerClassName: "px-4 py-3 text-right font-medium", cellClassName: "px-4 py-3 text-right", render: (item) => fmt(item.contractDeposit)},
                {key: "rentalPrice", header: "Giá thuê", headerClassName: "px-4 py-3 text-right font-medium", cellClassName: "px-4 py-3 text-right font-semibold", render: (item) => fmt(item.contractRentalPrice)},
                {key: "regulations", header: "Quy định", headerClassName: "px-4 py-3 text-center font-medium", cellClassName: "px-4 py-3 text-center", render: (item) => item.contractRegulations.length},
                {key: "returnDate", header: "Ngày hoàn trả", render: (item) => new Date(item.contractPremiseReturnDate).toLocaleDateString("vi-VN")},
                {key: "status", header: "Trạng thái", render: (item) => <ContractStatusBadge status={item.contractStatus}/>},
            ]}
        />
        {modal === "add" && <Modal title="Tạo hợp đồng mới" onClose={() => setModal(null)} wide>
            <ProcessingForm processing={saving} onSubmit={save}>{renderForm(false)}
                <div className="flex justify-end gap-2">
                    <button type="button" className={`${button} border border-slate-200`}
                            onClick={() => setModal(null)}>Hủy
                    </button>
                    <button className={`${button} bg-blue-600 text-white`}
                            disabled={!valid || saving}>{saving ? "Đang lưu..." : "Tạo hợp đồng"}</button>
                </div>
            </ProcessingForm>
        </Modal>}
        {modal === "detail" && selected &&
            <Modal title={editing ? "Chỉnh sửa hợp đồng" : "Chi tiết hợp đồng"} onClose={() => setModal(null)}
                   wide>{editing ? <ProcessingForm processing={saving} onSubmit={save}>{renderForm(true)}
                <div className="flex justify-end gap-2">
                    <button type="button" className={`${button} border border-slate-200`}
                            onClick={() => setEditing(false)}>Hủy
                    </button>
                    <button className={`${button} bg-blue-600 text-white`} disabled={!valid || saving}>Lưu thay đổi
                    </button>
                </div>
            </ProcessingForm> : <div>
                <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
                    <div><p className="text-xs text-slate-500">Contract ID</p><p
                        className="break-all font-mono text-sm">{selected.contractId}</p></div>
                    <ContractStatusBadge status={selected.contractStatus}/></div>
                <div className="grid gap-3 sm:grid-cols-2">
                    <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Account ID</p><p
                        className="mt-1 break-all font-mono text-xs">{selected.contractAccountId}</p></div>
                    <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Ngày hoàn trả</p>
                        <p className="mt-1 text-sm">{new Date(selected.contractPremiseReturnDate).toLocaleDateString("vi-VN")}</p>
                    </div>
                    <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Ngày tạo</p><p
                        className="mt-1 text-sm">{new Date(selected.contractCreatedAt).toLocaleString("vi-VN")}</p>
                    </div>
                    <div className="rounded-lg bg-slate-50 p-3"><p className="text-xs text-slate-500">Ngày cập nhật</p>
                        <p className="mt-1 text-sm">{new Date(selected.contractUpdatedAt).toLocaleString("vi-VN")}</p>
                    </div>
                </div>
                <div className="mt-5 grid gap-4 md:grid-cols-2">
                    <section><h3 className="mb-2 text-sm font-semibold">Regulations</h3>
                        <ul className="space-y-2 text-sm text-slate-600">{selected.contractRegulations.map((item) => <li
                            key={item.regulationId} className="rounded-lg bg-slate-50 p-3"><span
                            className="font-medium text-slate-800">{item.regulationName}</span>{item.regulationDescription &&
                            <span className="mt-1 block text-xs text-slate-500">{item.regulationDescription}</span>}
                        </li>)}{selected.contractRegulations.length === 0 &&
                            <li className="text-xs text-slate-400">Chưa gắn quy định.</li>}</ul>
                    </section>
                    <section><h3
                        className="mb-2 flex items-center gap-2 text-sm font-semibold text-red-700">Violations <span
                        className="rounded-full bg-red-100 px-2 py-0.5 text-xs">{selected.contractViolations.length}</span>
                    </h3>
                        <ul className="max-h-64 space-y-2 overflow-y-auto">{selected.contractViolations.map((item) =>
                            <li key={item.violationId}
                                className={`rounded-lg border p-3 text-sm ${item.violationIsResolved ? "border-emerald-100 bg-emerald-50" : "border-red-100 bg-red-50"}`}>
                                <p className={`font-medium ${item.violationIsResolved ? "text-emerald-800" : "text-red-800"}`}>{item.violationContent}</p>
                                <div
                                    className={`mt-1 flex flex-wrap justify-between gap-2 text-xs ${item.violationIsResolved ? "text-emerald-700" : "text-red-700"}`}>
                                    <span>{new Date(item.violationDate).toLocaleDateString("vi-VN")} · Phạt {fmt(item.violationPenaltyAmount ?? 0)}</span><span>{item.violationIsResolved ? "Đã xử lý" : "Chưa xử lý"}</span>
                                </div>
                            </li>)}{selected.contractViolations.length === 0 &&
                            <li className="text-xs text-slate-400">Chưa có vi phạm.</li>}</ul>
                    </section>
                </div>
                <div className="mt-5 flex justify-end gap-2">
                    <button className={`${button} border border-slate-200`} onClick={() => setModal(null)}>Đóng</button>
                    {!readOnly &&
                        <button className={`${button} bg-blue-600 text-white`} onClick={startEdit}>Chỉnh sửa</button>}
                </div>
            </div>}</Modal>}
    </div>
}
