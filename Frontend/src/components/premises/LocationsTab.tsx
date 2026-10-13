import type { LocationsTabProps as Props } from "./types"
import { formatDateTime } from "../../scripts/format"

export function LocationsTab({ locations, onEdit, onDelete }: Props) {
  return (
    <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-gray-100">
            <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
              Location ID (UUID v7)
            </th>
            <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
              Địa chỉ
            </th>
            <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
              Ngày tạo
            </th>
            <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
              Ngày cập nhật
            </th>
            <th className="text-right px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
              Thao tác
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-50">
          {locations.map((location) => (
            <tr
              key={location.locationId}
              className="hover:bg-gray-50/60 transition-colors"
            >
              <td className="px-4 py-3 text-xs font-mono text-gray-500">
                {location.locationId}
              </td>
              <td className="px-4 py-3 font-medium text-gray-800">
                {location.address}
              </td>
              <td className="px-4 py-3 text-xs text-gray-500">
                {formatDateTime(location.createdAt)}
              </td>
              <td className="px-4 py-3 text-xs text-gray-500">
                {formatDateTime(location.updatedAt)}
              </td>
              <td className="px-4 py-3 text-right">
                <button
                  onClick={() => onEdit(location)}
                  className="text-xs text-gray-400 hover:text-emerald-600 px-2 py-1 rounded hover:bg-emerald-50 transition-colors"
                >
                  Sửa
                </button>
                <button
                  onClick={() => onDelete(location)}
                  className="text-xs text-gray-400 hover:text-red-600 px-2 py-1 rounded hover:bg-red-50 transition-colors"
                >
                  Xóa
                </button>
              </td>
            </tr>
          ))}
          {!locations.length && (
            <tr>
              <td
                colSpan={5}
                className="px-4 py-12 text-center text-sm text-gray-400"
              >
                Chưa có địa chỉ. Hãy tạo địa chỉ trước khi thêm mặt bằng.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}
