import type { BusinessTypesTabProps as Props } from "./types"
import { formatDateTime } from "../../scripts/format"

export function BusinessTypesTab({ businessTypes, onView }: Props) {
  return (
    <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-gray-100">
            <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
              Business Type ID (UUID v7)
            </th>
            <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
              Tên mặt hàng
            </th>
            <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
              Mô tả
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
          {businessTypes.map((businessType) => (
            <tr
              key={businessType.businessTypeId}
              onClick={() => onView(businessType)}
              onKeyDown={(event) => {
                if (event.key === "Enter" || event.key === " ")
                  onView(businessType)
              }}
              tabIndex={0}
              className="hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none transition-colors cursor-pointer"
              title="Bấm để xem chi tiết"
            >
              <td
                className="px-4 py-3 text-xs font-mono text-gray-500 max-w-52 truncate"
                title={businessType.businessTypeId}
              >
                {businessType.businessTypeId}
              </td>
              <td className="px-4 py-3 font-semibold text-gray-900">
                {businessType.name}
              </td>
              <td className="px-4 py-3 text-xs text-gray-500 max-w-56 truncate">
                {businessType.description || "—"}
              </td>
              <td className="px-4 py-3">
                <span
                  className={`inline-flex items-center gap-1.5 px-2 py-1 rounded text-xs font-semibold ${
                    businessType.active
                      ? "bg-emerald-50 text-emerald-700"
                      : "bg-gray-100 text-gray-500"
                  }`}
                >
                  <span
                    className={`w-1.5 h-1.5 rounded-full ${
                      businessType.active ? "bg-emerald-500" : "bg-gray-400"
                    }`}
                  />
                  {businessType.active ? "Active" : "Inactive"}
                </span>
              </td>
              <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                {formatDateTime(businessType.createdAt)}
              </td>
              <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                {formatDateTime(businessType.updatedAt)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
