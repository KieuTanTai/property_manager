import type {PremisesTabProps as Props, PremiseStatus} from "./types"
import {BaseTable} from "../modules/shared"

function StatusBadge({status}: { status: PremiseStatus }) {
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
            <BaseTable
                rows={filteredPremises}
                getRowKey={(premise) => premise.id}
                paginationResetKey={filter}
                rowClassName={() => "hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none transition-colors"}
                rowTitle="Bấm để xem chi tiết"
                onRowClick={onView}
                columns={[
                    {key: "image", header: "Hình ảnh", render: (premise) => {
                        const media = premiseMedia.filter((item) => item.premiseMediaPremiseId === premise.id)
                        return media.length ? <div className="relative h-12 w-16">
                            <img src={media[0].premiseMediaImageUrl} alt={`Mặt bằng ${premise.name}`} className="h-full w-full rounded-lg bg-gray-100 object-cover" />
                            {media.length > 1 && <span className="absolute -right-1 -bottom-1 rounded bg-gray-900 px-1.5 py-0.5 text-xs font-semibold text-white">+{media.length - 1}</span>}
                        </div> : <div className="flex h-12 w-16 items-center justify-center rounded-lg bg-gray-100 text-xs text-gray-400">Chưa có</div>
                    }},
                    {key: "name", header: "ID / Tên", render: (premise) => <><div className="font-semibold text-gray-900">{premise.name}</div><div className="font-mono text-xs text-gray-400">{premise.id}</div></>},
                    {key: "position", header: "Vị trí", cellClassName: "px-4 py-3 text-gray-600", render: (premise) => `Tầng ${premise.floor} - Vị trí ${premise.position}`},
                    {key: "area", header: "Diện tích", cellClassName: "px-4 py-3 font-mono text-xs text-gray-700", render: (premise) => premise.area},
                    {key: "location", header: "Địa chỉ", cellClassName: "max-w-48 truncate px-4 py-3 text-xs text-gray-500", render: (premise) => premise.location},
                    {key: "status", header: "Trạng thái", render: (premise) => <StatusBadge status={premise.status}/>},
                ]}
            />
        </>
    )
}
