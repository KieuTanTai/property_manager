import type { PremisesTabProps as Props, PremiseStatus } from "./types"

function StatusBadge({ status }: { status: PremiseStatus }) {
  const map: Record<PremiseStatus, { label: string; cls: string }> = {
    available: {
      label: "Còn trống",
      cls: "bg-emerald-100 text-emerald-700 border border-emerald-200",
    },
    rented: {
      label: "Đang thuê",
      cls: "bg-blue-100 text-blue-700 border border-blue-200",
    },
    reserved: {
      label: "Đã đặt cọc",
      cls: "bg-amber-100 text-amber-700 border border-amber-200",
    },
    maintenance: {
      label: "Bảo trì",
      cls: "bg-red-100 text-red-700 border border-red-200",
    },
  }
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${map[status].cls}`}
    >
      {map[status].label}
    </span>
  )
}

export function PremisesTab({
  premises,
  premiseMedia,
  filteredPremises,
  filter,
  onFilterChange,
  onView,
}: Props) {
  return (
    <>
      <div className="flex gap-1 bg-gray-100 rounded-lg p-1 mb-5 w-fit">
        {([
          "all",
          "available",
          "rented",
          "reserved",
          "maintenance",
        ] as const).map((status) => (
          <button
            key={status}
            onClick={() => onFilterChange(status)}
            className={`px-3 py-1.5 rounded-md text-xs font-medium transition-all ${
              filter === status
                ? "bg-white text-gray-900 shadow-sm"
                : "text-gray-500 hover:text-gray-700"
            }`}
          >
            {status === "all"
              ? "Tất cả"
              : status === "available"
                ? "Còn trống"
                : status === "rented"
                  ? "Đang thuê"
                  : status === "reserved"
                    ? "Đã đặt cọc"
                    : "Bảo trì"}
            <span className="ml-1.5 text-gray-400">
              {status === "all"
                ? premises.length
                : premises.filter((premise) => premise.status === status)
                    .length}
            </span>
          </button>
        ))}
      </div>
      <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-100">
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Hình ảnh
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                ID / Tên
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Vị trí
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Diện tích
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Địa chỉ
              </th>
              <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                Trạng thái
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {filteredPremises.map((premise) => (
              <tr
                key={premise.id}
                onClick={() => onView(premise)}
                onKeyDown={(event) => {
                  if (event.key === "Enter" || event.key === " ")
                    onView(premise)
                }}
                tabIndex={0}
                className="hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none transition-colors cursor-pointer"
                title="Bấm để xem chi tiết"
              >
                <td className="px-4 py-3">
                  {(() => {
                    const media = premiseMedia.filter(
                      (item) => item.premiseMediaPremiseId === premise.id,
                    )
                    return media.length ? (
                      <div className="relative w-16 h-12">
                        <img
                          src={media[0].premiseMediaImageUrl}
                          alt={`Mặt bằng ${premise.name}`}
                          className="w-full h-full rounded-lg object-cover bg-gray-100"
                        />
                        {media.length > 1 && (
                          <span className="absolute -right-1 -bottom-1 rounded bg-gray-900 px-1.5 py-0.5 text-xs font-semibold text-white">
                            +{media.length - 1}
                          </span>
                        )}
                      </div>
                    ) : (
                      <div className="w-16 h-12 rounded-lg bg-gray-100 flex items-center justify-center text-xs text-gray-400">
                        Chưa có
                      </div>
                    )
                  })()}
                </td>
                <td className="px-4 py-3">
                  <div className="font-semibold text-gray-900">
                    {premise.name}
                  </div>
                  <div className="text-xs text-gray-400 font-mono">
                    {premise.id}
                  </div>
                </td>
                <td className="px-4 py-3 text-gray-600">
                  Tầng {premise.floor} - Vị trí {premise.position}
                </td>
                <td className="px-4 py-3 font-mono text-gray-700 text-xs">
                  {premise.area}
                </td>
                <td className="px-4 py-3 text-gray-500 max-w-48 truncate text-xs">
                  {premise.location}
                </td>
                <td className="px-4 py-3">
                  <StatusBadge status={premise.status} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  )
}
