import { useState } from "react"
import { FormField, Modal, fmt, inputCls } from "./shared"

interface Regulation {
  regulationId: string
  regulationName: string
  regulationDescription: string | null
  regulationFineAmount: number | null
  regulationIsActive: boolean
  regulationCreatedAt: string
  regulationUpdatedAt: string
}

interface RegulationForm {
  name: string
  description: string
  fineAmount: string
  active: boolean
}

const REGULATIONS: Regulation[] = [
  {
    regulationId: "0195c8f4-ad30-7111-8111-100000000001",
    regulationName: "Giữ gìn vệ sinh chung",
    regulationDescription:
      "Không xả rác tại hành lang, lối đi và khu vực sinh hoạt chung.",
    regulationFineAmount: 200000,
    regulationIsActive: true,
    regulationCreatedAt: "2024-01-08T08:30:00",
    regulationUpdatedAt: "2024-01-08T08:30:00",
  },
  {
    regulationId: "0195c8f4-ad31-7222-8222-200000000002",
    regulationName: "Quy định tiếng ồn",
    regulationDescription:
      "Không gây tiếng ồn ảnh hưởng đến các mặt bằng khác sau 22:00.",
    regulationFineAmount: 500000,
    regulationIsActive: true,
    regulationCreatedAt: "2024-01-10T09:15:00",
    regulationUpdatedAt: "2024-06-15T11:20:00",
  },
  {
    regulationId: "0195c8f4-ad32-7333-8333-300000000003",
    regulationName: "Cải tạo mặt bằng",
    regulationDescription:
      "Mọi hoạt động sửa chữa, cải tạo phải được ban quản lý chấp thuận trước.",
    regulationFineAmount: null,
    regulationIsActive: false,
    regulationCreatedAt: "2024-02-05T13:45:00",
    regulationUpdatedAt: "2024-09-01T16:10:00",
  },
]

const EMPTY_FORM: RegulationForm = {
  name: "",
  description: "",
  fineAmount: "",
  active: true,
}

const formatDateTime = (value: string) =>
  new Intl.DateTimeFormat("vi-VN", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))

const createUuidV7 = () => {
  const bytes = crypto.getRandomValues(new Uint8Array(16))
  const timestamp = Date.now()
  for (let index = 5; index >= 0; index--)
    bytes[index] = Math.floor(timestamp / 2 ** ((5 - index) * 8)) & 0xff
  bytes[6] = (bytes[6] & 0x0f) | 0x70
  bytes[8] = (bytes[8] & 0x3f) | 0x80
  const hex = Array.from(bytes, (byte) =>
    byte.toString(16).padStart(2, "0"),
  ).join("")
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`
}

export function RegulationsModule() {
  const [regulations, setRegulations] = useState<Regulation[]>(REGULATIONS)
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
            style={{ fontFamily: "DM Sans" }}
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

      <div className="bg-white rounded-xl border border-gray-100 overflow-x-auto">
        <table className="w-full min-w-5xl text-sm">
          <thead>
            <tr className="border-b border-gray-100">
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Regulation ID
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Tên quy định
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Mô tả
              </th>
              <th className="text-right px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Mức phạt
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Trạng thái
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Ngày tạo
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Ngày cập nhật
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {regulations.map((regulation) => (
              <tr
                key={regulation.regulationId}
                onClick={() => openView(regulation)}
                onKeyDown={(event) => {
                  if (event.key === "Enter" || event.key === " ")
                    openView(regulation)
                }}
                tabIndex={0}
                className="hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none transition-colors cursor-pointer"
                title="Bấm để xem chi tiết"
              >
                <td
                  className="px-4 py-3 text-xs font-mono text-gray-500 max-w-44 truncate"
                  title={regulation.regulationId}
                >
                  {regulation.regulationId}
                </td>
                <td className="px-4 py-3 font-semibold text-gray-900 max-w-48">
                  {regulation.regulationName}
                </td>
                <td
                  className="px-4 py-3 text-xs text-gray-500 max-w-64 truncate"
                  title={regulation.regulationDescription ?? undefined}
                >
                  {regulation.regulationDescription || "—"}
                </td>
                <td className="px-4 py-3 text-right font-mono text-xs font-semibold text-gray-700 whitespace-nowrap">
                  {regulation.regulationFineAmount === null
                    ? "—"
                    : fmt(regulation.regulationFineAmount)}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center gap-1.5 px-2 py-1 rounded text-xs font-semibold ${
                      regulation.regulationIsActive
                        ? "bg-emerald-50 text-emerald-700"
                        : "bg-gray-100 text-gray-500"
                    }`}
                  >
                    <span
                      className={`w-1.5 h-1.5 rounded-full ${
                        regulation.regulationIsActive
                          ? "bg-emerald-500"
                          : "bg-gray-400"
                      }`}
                    />
                    {regulation.regulationIsActive
                      ? "Đang áp dụng"
                      : "Ngừng áp dụng"}
                  </span>
                </td>
                <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                  {formatDateTime(regulation.regulationCreatedAt)}
                </td>
                <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                  {formatDateTime(regulation.regulationUpdatedAt)}
                </td>
              </tr>
            ))}
            {!regulations.length && (
              <tr>
                <td
                  colSpan={7}
                  className="px-4 py-12 text-center text-sm text-gray-400"
                >
                  Chưa có quy định nào.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

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
                    setForm({ ...form, name: event.target.value })
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
                    setForm({ ...form, description: event.target.value })
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
                      setForm({ ...form, fineAmount: event.target.value })
                    }
                    placeholder="Để trống nếu không áp dụng mức phạt"
                  />
                  <span className="absolute inset-y-0 right-3 flex items-center text-xs font-semibold text-gray-400 pointer-events-none">
                    VND
                  </span>
                </div>
              </FormField>
              {modal === "view" && isEditing && (
                <FormField label="Trạng thái">
                  <label className="flex items-center justify-between rounded-lg border border-gray-200 px-3 py-2.5 cursor-pointer hover:bg-gray-50 transition-colors">
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
                        setForm({ ...form, active: event.target.checked })
                      }
                      className="w-4 h-4 accent-blue-600"
                    />
                  </label>
                </FormField>
              )}
              {modal === "add" && (
                <div className="mb-4 rounded-lg border border-emerald-100 bg-emerald-50 px-3 py-2 text-xs text-emerald-700">
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
