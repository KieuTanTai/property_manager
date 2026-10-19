import type {BusinessTypesTabProps as Props} from "./types"
import {formatDateTime} from "../../scripts/format"
import {BaseTable} from "../modules/shared"

export function BusinessTypesTab({businessTypes, onView}: Props) {
    return (
        <BaseTable
            rows={businessTypes}
            getRowKey={(item) => item.businessTypeId}
            rowClassName={() => "hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none transition-colors"}
            rowTitle="Bấm để xem chi tiết"
            onRowClick={onView}
            columns={[
                {key: "id", header: "Business Type ID (UUID v7)", cellClassName: "max-w-52 truncate px-4 py-3 font-mono text-xs text-gray-500", render: (item) => <span title={item.businessTypeId}>{item.businessTypeId}</span>},
                {key: "name", header: "Tên mặt hàng", cellClassName: "px-4 py-3 font-semibold text-gray-900", render: (item) => item.name},
                {key: "description", header: "Mô tả", cellClassName: "max-w-56 truncate px-4 py-3 text-xs text-gray-500", render: (item) => item.description || "—"},
                {key: "active", header: "Trạng thái", render: (item) => <span className={`inline-flex items-center gap-1.5 rounded px-2 py-1 text-xs font-semibold ${item.active ? "bg-emerald-50 text-emerald-700" : "bg-gray-100 text-gray-500"}`}>
                    <span className={`h-1.5 w-1.5 rounded-full ${item.active ? "bg-emerald-500" : "bg-gray-400"}`} />{item.active ? "Active" : "Inactive"}</span>},
                {key: "created", header: "Ngày tạo", cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500", render: (item) => formatDateTime(item.createdAt)},
                {key: "updated", header: "Ngày cập nhật", cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500", render: (item) => formatDateTime(item.updatedAt)},
            ]}
        />
    )
}
