import type {LocationsTabProps as Props} from "./types"
import {formatDateTime} from "../../scripts/format"
import {BaseTable} from "../modules/shared"

export function LocationsTab({locations, onEdit, onDelete, canEdit = true, canDelete = true}: Props) {
    return (
        <BaseTable
            rows={locations}
            getRowKey={(location) => location.locationId}
            emptyMessage="Chưa có địa chỉ. Hãy tạo địa chỉ trước khi thêm mặt bằng."
            rowClassName={() => "hover:bg-gray-50/60 transition-colors"}
            columns={[
                {key: "id", header: "Location ID (UUID v7)", cellClassName: "px-4 py-3 font-mono text-xs text-gray-500", render: (location) => location.locationId},
                {key: "address", header: "Địa chỉ", cellClassName: "px-4 py-3 font-medium text-gray-800", render: (location) => location.address},
                {key: "created", header: "Ngày tạo", cellClassName: "px-4 py-3 text-xs text-gray-500", render: (location) => formatDateTime(location.createdAt)},
                {key: "updated", header: "Ngày cập nhật", cellClassName: "px-4 py-3 text-xs text-gray-500", render: (location) => formatDateTime(location.updatedAt)},
                {key: "actions", header: "Thao tác", headerClassName: "px-4 py-3 text-right font-medium", cellClassName: "px-4 py-3 text-right", render: (location) => <>
                    {canEdit && <button onClick={() => onEdit(location)} className="rounded px-2 py-1 text-xs text-gray-400 transition-colors hover:bg-emerald-50 hover:text-emerald-600">Sửa</button>}
                    {canDelete && <button onClick={() => onDelete(location)} className="rounded px-2 py-1 text-xs text-gray-400 transition-colors hover:bg-red-50 hover:text-red-600">Xóa</button>}
                </>},
            ]}
        />
    )
}
