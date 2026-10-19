import {useState} from "react"
import {BaseTable, FormField, inputCls, Modal} from "./shared"
import {MOCK_REGULATIONS} from "../../mocks/regulations"
import {fmt, formatDateTime} from "../../scripts/format"
import type {Regulation, RegulationForm} from "../../types/regulations"
import {createUuidV7} from "../../scripts/ids"

const EMPTY_FORM: RegulationForm = {
    name: "",
    description: "",
    fineAmount: "",
    active: true,
}

export function RegulationsModule() {
    const [regulations, setRegulations] = useState<Regulation[]>(MOCK_REGULATIONS)
    const [modal, setModal] = useState<"add" | "view" | null>(null)
    const [selected, setSelected] = useState<Regulation | null>(null)
    const [form, setForm] = useState<RegulationForm>(EMPTY_FORM)
    const [isEditing, setIsEditing] = useState(false)

    const openAdd = () => {
        setSelected(null)
        setForm(EMPTY_FORM)
        setModal("add")
    }

    const startEditing = () => {
        if (!selected) return
        setForm({
            name: selected.regulationName,
            description: selected.regulationDescription ?? "",
            fineAmount:
                selected.regulationFineAmount === null
                    ? ""
                    : String(selected.regulationFineAmount),
            active: selected.regulationIsActive,
        })
        setIsEditing(true)
    }

    const openView = (regulation: Regulation) => {
        setSelected(regulation)
        setIsEditing(false)
        setModal("view")
    }

    const closeModal = () => {
        setModal(null)
        setSelected(null)
        setForm(EMPTY_FORM)
        setIsEditing(false)
    }

    const saveRegulation = () => {
        const name = form.name.trim()
        if (!name) return

        const now = new Date().toISOString()
        const description = form.description.trim() || null
        const fineAmount = form.fineAmount === "" ? null : Number(form.fineAmount)

        if (modal === "view" && selected) {
            const updated: Regulation = {
                ...selected,
                regulationName: name,
                regulationDescription: description,
                regulationFineAmount: fineAmount,
                regulationIsActive: form.active,
                regulationUpdatedAt: now,
            }
            setRegulations(
                regulations.map((regulation) =>
                    regulation.regulationId === selected.regulationId
                        ? updated
                        : regulation,
                ),
            )
            setSelected(updated)
            setIsEditing(false)
        } else {
            setRegulations([
                ...regulations,
                {
                    regulationId: createUuidV7(),
                    regulationName: name,
                    regulationDescription: description,
                    regulationFineAmount: fineAmount,
                    regulationIsActive: true,
                    regulationCreatedAt: now,
                    regulationUpdatedAt: now,
                },
            ])
            closeModal()
        }
    }

    const isValid =
        form.name.trim().length > 0 &&
        (form.fineAmount === "" || Number(form.fineAmount) >= 0)

    return (
        <div>
            <div className="flex items-center justify-between mb-6">
                <div>
                    <h1
                        className="text-xl font-bold text-gray-900"
                        style={{fontFamily: "DM Sans"}}
                    >
                        Quản lý quy định
                    </h1>
                    <p className="text-sm text-gray-400 mt-0.5">
                        {
                            regulations.filter((regulation) => regulation.regulationIsActive)
                                .length
                        }{" "}
                        quy định đang áp dụng
                    </p>
                </div>
                <button
                    onClick={openAdd}
                    className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors flex items-center gap-2"
                >
                    <span className="text-base leading-none">+</span>
                    Thêm quy định
                </button>
            </div>

            <BaseTable
                rows={regulations}
                getRowKey={(item) => item.regulationId}
                emptyMessage="Chưa có quy định nào."
                tableClassName="w-full min-w-5xl text-sm"
                rowClassName={() => "hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none transition-colors"}
                rowTitle="Bấm để xem chi tiết"
                onRowClick={openView}
                columns={[
                    {key: "id", header: "Regulation ID", cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-500", render: (item) => <span title={item.regulationId}>{item.regulationId}</span>},
                    {key: "name", header: "Tên quy định", cellClassName: "max-w-48 px-4 py-3 font-semibold text-gray-900", render: (item) => item.regulationName},
                    {key: "description", header: "Mô tả", cellClassName: "max-w-64 px-4 py-3 text-xs text-gray-500", render: (item) => <span title={item.regulationDescription ?? undefined}>{item.regulationDescription || "—"}</span>},
                    {key: "fine", header: "Mức phạt", headerClassName: "px-4 py-3 text-right font-medium", cellClassName: "whitespace-nowrap px-4 py-3 text-right font-mono text-xs font-semibold text-gray-700", render: (item) => item.regulationFineAmount === null ? "—" : fmt(item.regulationFineAmount)},
                    {key: "active", header: "Trạng thái", render: (item) => <span className={`inline-flex items-center gap-1.5 rounded px-2 py-1 text-xs font-semibold ${item.regulationIsActive ? "bg-emerald-50 text-emerald-700" : "bg-gray-100 text-gray-500"}`}>
                        <span className={`h-1.5 w-1.5 rounded-full ${item.regulationIsActive ? "bg-emerald-500" : "bg-gray-400"}`} />{item.regulationIsActive ? "Đang áp dụng" : "Ngừng áp dụng"}</span>},
                    {key: "created", header: "Ngày tạo", cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500", render: (item) => formatDateTime(item.regulationCreatedAt)},
                    {key: "updated", header: "Ngày cập nhật", cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500", render: (item) => formatDateTime(item.regulationUpdatedAt)},
                ]}
            />

            {modal && (
                <Modal
                    title={
                        modal === "add"
                            ? "Thêm quy định mới"
                            : isEditing
                                ? "Chỉnh sửa quy định"
                                : "Chi tiết quy định"
                    }
                    onClose={closeModal}
                >
                    {modal === "view" && selected && !isEditing ? (
                        <div className="space-y-5">
                            <div>
                                <div className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                                    Regulation ID
                                </div>
                                <div className="font-mono text-xs text-gray-600 break-all">
                                    {selected.regulationId}
                                </div>
                            </div>
                            <div>
                                <div className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                                    Tên quy định
                                </div>
                                <div className="text-sm font-semibold text-gray-900">
                                    {selected.regulationName}
                                </div>
                            </div>
                            <div>
                                <div className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                                    Mô tả
                                </div>
                                <div className="text-sm text-gray-600 leading-relaxed">
                                    {selected.regulationDescription || "—"}
                                </div>
                            </div>
                            <div className="grid grid-cols-2 gap-5">
                                <div>
                                    <div className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                                        Mức phạt
                                    </div>
                                    <div className="text-sm font-mono font-semibold text-gray-800">
                                        {selected.regulationFineAmount === null
                                            ? "—"
                                            : fmt(selected.regulationFineAmount)}
                                    </div>
                                </div>
                                <div>
                                    <div className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                                        Trạng thái
                                    </div>
                                    <span
                                        className={`inline-flex items-center gap-1.5 px-2 py-1 rounded text-xs font-semibold ${
                                            selected.regulationIsActive
                                                ? "bg-emerald-50 text-emerald-700"
                                                : "bg-gray-100 text-gray-500"
                                        }`}
                                    >
                    <span
                        className={`w-1.5 h-1.5 rounded-full ${
                            selected.regulationIsActive
                                ? "bg-emerald-500"
                                : "bg-gray-400"
                        }`}
                    />
                                        {selected.regulationIsActive
                                            ? "Đang áp dụng"
                                            : "Ngừng áp dụng"}
                  </span>
                                </div>
                                <div>
                                    <div className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                                        Ngày tạo
                                    </div>
                                    <div className="text-sm text-gray-600">
                                        {formatDateTime(selected.regulationCreatedAt)}
                                    </div>
                                </div>
                                <div>
                                    <div className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-1">
                                        Ngày cập nhật
                                    </div>
                                    <div className="text-sm text-gray-600">
                                        {formatDateTime(selected.regulationUpdatedAt)}
                                    </div>
                                </div>
                            </div>
                            <div className="flex justify-end gap-2 pt-1">
                                <button
                                    onClick={closeModal}
                                    className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
                                >
                                    Đóng
                                </button>
                                <button
                                    onClick={startEditing}
                                    className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                                >
                                    Chỉnh sửa
                                </button>
                            </div>
                        </div>
                    ) : (
                        <>
                            <FormField label="Tên quy định">
                                <input
                                    className={inputCls}
                                    value={form.name}
                                    maxLength={50}
                                    onChange={(event) =>
                                        setForm({...form, name: event.target.value})
                                    }
                                    placeholder="Nhập tên quy định"
                                    autoFocus
                                />
                                <div className="text-right text-xs text-gray-400 mt-1">
                                    {form.name.length}/50
                                </div>
                            </FormField>
                            <FormField label="Mô tả (không bắt buộc)">
                <textarea
                    className={inputCls}
                    rows={3}
                    value={form.description}
                    maxLength={255}
                    onChange={(event) =>
                        setForm({...form, description: event.target.value})
                    }
                    placeholder="Nhập nội dung mô tả"
                />
                                <div className="text-right text-xs text-gray-400 mt-1">
                                    {form.description.length}/255
                                </div>
                            </FormField>
                            <FormField label="Mức phạt (không bắt buộc)">
                                <div className="relative">
                                    <input
                                        className={`${inputCls} pr-14`}
                                        type="number"
                                        min="0"
                                        step="1000"
                                        value={form.fineAmount}
                                        onChange={(event) =>
                                            setForm({...form, fineAmount: event.target.value})
                                        }
                                        placeholder="Để trống nếu không áp dụng mức phạt"
                                    />
                                    <span
                                        className="absolute inset-y-0 right-3 flex items-center text-xs font-semibold text-gray-400 pointer-events-none">
                    VND
                  </span>
                                </div>
                            </FormField>
                            {modal === "view" && isEditing && (
                                <FormField label="Trạng thái">
                                    <label
                                        className="flex items-center justify-between rounded-lg border border-gray-200 px-3 py-2.5 cursor-pointer hover:bg-gray-50 transition-colors">
                    <span>
                      <span className="block text-sm font-medium text-gray-800">
                        Đang áp dụng
                      </span>
                      <span className="block text-xs text-gray-400 mt-0.5">
                        Cho phép sử dụng quy định này trong hệ thống
                      </span>
                    </span>
                                        <input
                                            type="checkbox"
                                            checked={form.active}
                                            onChange={(event) =>
                                                setForm({...form, active: event.target.checked})
                                            }
                                            className="w-4 h-4 accent-blue-600"
                                        />
                                    </label>
                                </FormField>
                            )}
                            {modal === "add" && (
                                <div
                                    className="mb-4 rounded-lg border border-emerald-100 bg-emerald-50 px-3 py-2 text-xs text-emerald-700">
                                    Quy định mới sẽ được kích hoạt mặc định.
                                </div>
                            )}
                            <div className="flex justify-end gap-2 mt-2">
                                <button
                                    onClick={
                                        modal === "view" ? () => setIsEditing(false) : closeModal
                                    }
                                    className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
                                >
                                    Hủy
                                </button>
                                <button
                                    onClick={saveRegulation}
                                    disabled={!isValid}
                                    className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-200 disabled:text-gray-400 disabled:cursor-not-allowed"
                                >
                                    {modal === "add" ? "Thêm quy định" : "Lưu thay đổi"}
                                </button>
                            </div>
                        </>
                    )}
                </Modal>
            )}
        </div>
    )
}
