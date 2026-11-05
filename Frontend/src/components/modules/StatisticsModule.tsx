import {useEffect, useState} from "react"
import {
    MOCK_ACCOUNT_MONTHLY,
    MOCK_CONTRACT_MONTHLY,
    MOCK_MONTHLY_REVENUE,
    MOCK_VIOLATION_MONTHLY
} from "../../mocks/statistics"
import {BaseTable, ContractStatusBadge, InvoiceStatusBadge, PremiseStatusBadge} from "./shared"
import {ACCOUNTS, CONTRACTS, INVOICES, VIOLATIONS} from "../../mocks/shared"
import {fmt} from "../../scripts/format"
import {premiseApi} from "@/api/premise"
import type {LocationModel, PremiseModel, PremiseStatus} from "../premises/types"

// ─── Statistics Module ────────────────────────────────────────────────────────

const fmtM = (n: number) =>
    n >= 1000000
        ? `${(n / 1000000).toFixed(1)}tr`
        : n >= 1000
            ? `${(n / 1000).toFixed(0)}k`
            : String(n)

function KPICard({
                     label,
                     value,
                     sub,
                     trend,
                     color,
                 }: {
    label: string
    value: string
    sub?: string
    trend?: { val: string; up: boolean }
    color: string
}) {
    return (
        <div
            className="bg-white rounded-2xl border border-gray-100 p-5 flex flex-col gap-3 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
            <div className="flex items-center justify-between">
        <span className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
          {label}
        </span>
                <div className={`w-2 h-2 rounded-full ${color}`}/>
            </div>
            <div
                className="text-2xl font-bold text-gray-900"
                style={{fontFamily: "DM Sans"}}
            >
                {value}
            </div>
            <div className="flex items-center justify-between">
                {sub && <span className="text-xs text-gray-400">{sub}</span>}
                {trend && (
                    <span
                        className={`text-xs font-semibold ${
                            trend.up ? "text-emerald-600" : "text-red-500"
                        }`}
                    >
            {trend.up ? "↑" : "↓"} {trend.val}
          </span>
                )}
            </div>
        </div>
    )
}

function SectionHeading({title, sub}: { title: string; sub?: string }) {
    return (
        <div className="flex items-end justify-between mb-5">
            <div>
                <h2
                    className="text-base font-bold text-gray-900"
                    style={{fontFamily: "DM Sans"}}
                >
                    {title}
                </h2>
                {sub && <p className="text-xs text-gray-400 mt-0.5">{sub}</p>}
            </div>
            <div className="h-px flex-1 mx-6 bg-gray-100"/>
        </div>
    )
}

function DistributionList({
                              items,
                          }: {
    items: { name: string; value: number; color: string }[]
}) {
    const total = items.reduce((sum, item) => sum + item.value, 0)
    return (
        <div className="divide-y divide-gray-100">
            {items.map((item, index) => (
                <div
                    key={item.name}
                    className="flex items-center gap-3 py-3 first:pt-0 last:pb-0"
                >
          <span className="w-5 text-xs font-mono text-gray-300">
            {String(index + 1).padStart(2, "0")}
          </span>
                    <div className={`h-2.5 w-2.5 rounded-full ${item.color}`}/>
                    <span className="flex-1 text-sm font-medium text-gray-700">
            {item.name}
          </span>
                    <span className="text-xs text-gray-400">
            {total ? Math.round((item.value / total) * 100) : 0}%
          </span>
                    <span className="w-8 text-right text-sm font-mono font-semibold text-gray-900">
            {item.value}
          </span>
                </div>
            ))}
        </div>
    )
}

function toPremiseStatus(status: PremiseModel["premiseStatus"]): PremiseStatus {
    switch (status) {
        case "Available":
            return "available"
        case "Rented":
            return "rented"
        case "Reserved":
            return "reserved"
        case "Maintenance":
            return "maintenance"
    }
}

export function StatisticsModule() {
    const [activeTab, setActiveTab] =
        useState<"accounts" | "finance" | "premises" | "contracts" | "violations">(
            "finance",
        )
    const [premises, setPremises] = useState<PremiseModel[]>([])
    const [locations, setLocations] = useState<LocationModel[]>([])
    const [premiseFilter, setPremiseFilter] = useState<PremiseStatus | "all">("all")
    const [premisesLoading, setPremisesLoading] = useState(false)
    const [premisesError, setPremisesError] = useState<string | null>(null)
    const [premisesReload, setPremisesReload] = useState(0)

    useEffect(() => {
        if (activeTab !== "premises") return

        let isCurrent = true
        setPremisesLoading(true)
        setPremisesError(null)
        void Promise.all([premiseApi.getPremise(), premiseApi.getLocations()])
            .then(([premiseItems, locationItems]) => {
                if (!isCurrent) return
                setPremises(premiseItems)
                setLocations(locationItems)
            })
            .catch((cause: unknown) => {
                if (!isCurrent) return
                setPremisesError(cause instanceof Error ? cause.message : "Không thể tải thống kê mặt bằng.")
            })
            .finally(() => {
                if (isCurrent) setPremisesLoading(false)
            })
        return () => {
            isCurrent = false
        }
    }, [activeTab, premisesReload])

    const tabs = [
        {id: "finance" as const, label: "Tài chính"},
        {id: "accounts" as const, label: "Tài khoản"},
        {id: "premises" as const, label: "Mặt bằng"},
        {id: "contracts" as const, label: "Hợp đồng"},
        {id: "violations" as const, label: "Vi phạm"},
    ]

    // Derived stats
    const totalRevenue = MOCK_MONTHLY_REVENUE.reduce((s, m) => s + m.doanhThu, 0)
    const totalProfit = MOCK_MONTHLY_REVENUE.reduce((s, m) => s + m.loiNhuan, 0)
    const invoicePaid = INVOICES.filter((i) => i.status === "paid").reduce(
        (s, i) => s + i.totalAmount,
        0,
    )
    const invoiceUnpaid = INVOICES.filter((i) => i.status !== "paid").reduce(
        (s, i) => s + i.totalAmount,
        0,
    )

    const contractsByStatus = [
        {
            name: "Đã ký",
            value: CONTRACTS.filter((c) => c.status === "Signed").length,
            color: "bg-emerald-500",
        },
        {
            name: "Chờ duyệt",
            value: CONTRACTS.filter((c) => c.status === "PendingApproval").length,
            color: "bg-amber-500",
        },
        {
            name: "Chờ ký",
            value: CONTRACTS.filter((c) => c.status === "PendingSignature").length,
            color: "bg-blue-500",
        },
        {
            name: "Hết hạn",
            value: CONTRACTS.filter((c) => c.status === "Expired").length,
            color: "bg-red-500",
        },
        {
            name: "Chấm dứt",
            value: CONTRACTS.filter((c) => c.status === "Terminated").length,
            color: "bg-slate-400",
        },
    ]

    const invoiceByStatus = [
        {
            name: "Đã thanh toán",
            value: INVOICES.filter((i) => i.status === "paid").length,
            color: "bg-emerald-500",
        },
        {
            name: "Chưa thanh toán",
            value: INVOICES.filter((i) => i.status === "unpaid").length,
            color: "bg-amber-500",
        },
        {
            name: "Quá hạn",
            value: INVOICES.filter((i) => i.status === "overdue").length,
            color: "bg-red-500",
        },
    ]

    const violationResolved = VIOLATIONS.filter((v) => v.resolved).length
    const violationUnresolved = VIOLATIONS.filter((v) => !v.resolved).length
    const totalPenalty = VIOLATIONS.reduce((s, v) => s + v.penaltyAmount, 0)

    const accountsByRole = [
        {
            name: "Super Admin",
            value: ACCOUNTS.filter((a) => a.role === "SUPER_ADMIN").length,
            color: "bg-violet-500",
        },
        {
            name: "Manager",
            value: ACCOUNTS.filter((a) => a.role === "MANAGER").length,
            color: "bg-blue-500",
        },
        {
            name: "Staff",
            value: ACCOUNTS.filter((a) => a.role === "STAFF").length,
            color: "bg-emerald-500",
        },
        {
            name: "Tenant",
            value: ACCOUNTS.filter((a) => a.role === "TENANT").length,
            color: "bg-amber-500",
        },
    ]

    const totalAccounts = ACCOUNTS.length
    const activeAccounts = ACCOUNTS.filter((a) => a.active).length
    const premiseStatusCounts: Record<PremiseStatus, number> = {
        rented: premises.filter((premise) => premise.premiseStatus === "Rented").length,
        available: premises.filter((premise) => premise.premiseStatus === "Available").length,
        reserved: premises.filter((premise) => premise.premiseStatus === "Reserved").length,
        maintenance: premises.filter((premise) => premise.premiseStatus === "Maintenance").length,
    }
    const filteredPremises = premiseFilter === "all"
        ? premises
        : premises.filter((premise) => toPremiseStatus(premise.premiseStatus) === premiseFilter)
    const premisesByStatus = [
        {name: "Đang thuê", value: premiseStatusCounts.rented, color: "bg-blue-500"},
        {name: "Còn trống", value: premiseStatusCounts.available, color: "bg-emerald-500"},
        {name: "Đặt cọc", value: premiseStatusCounts.reserved, color: "bg-amber-500"},
        {name: "Bảo trì", value: premiseStatusCounts.maintenance, color: "bg-red-500"},
    ]
    const premisesByLocation = [...premises.reduce((groups, premise) => {
        const address = locations.find((location) => location.locationId === premise.premiseLocationId)
            ?.locationAddress ?? premise.premiseLocation?.locationAddress ?? "Chưa xác định địa chỉ"
        const group = groups.get(premise.premiseLocationId) ?? {
            id: premise.premiseLocationId,
            name: address,
            total: 0,
            rented: 0
        }
        group.total += 1
        if (premise.premiseStatus === "Rented") group.rented += 1
        groups.set(premise.premiseLocationId, group)
        return groups
    }, new Map<string, {id: string; name: string; total: number; rented: number}>()).values()]
    const totalPremiseArea = premises.reduce((total, premise) => {
        const area = Number.parseFloat(premise.premiseArea)
        return Number.isFinite(area) ? total + area : total
    }, 0)
    const occupancyRate = premises.length
        ? Math.round((premiseStatusCounts.rented / premises.length) * 100)
        : 0
    const formatArea = (area: number) =>
        `${area.toLocaleString("vi-VN", {maximumFractionDigits: 2})} m²`

    return (
        <div>
            <div className="mb-6">
                <h1
                    className="text-xl font-bold text-gray-900"
                    style={{fontFamily: "DM Sans"}}
                >
                    Thống kê & Phân tích
                </h1>
                <p className="text-sm text-gray-400 mt-0.5">
                    Dữ liệu tổng hợp theo thời gian thực — năm 2024
                </p>
            </div>

            {/* Tab navigation */}
            <div className="flex gap-1 bg-gray-100 rounded-xl p-1 mb-6 w-fit">
                {tabs.map((t) => (
                    <button
                        key={t.id}
                        onClick={() => setActiveTab(t.id)}
                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                            activeTab === t.id
                                ? "bg-white text-gray-900 shadow-sm"
                                : "text-gray-500 hover:text-gray-700"
                        }`}
                    >
                        {t.label}
                    </button>
                ))}
            </div>

            {/* ── Finance ── */}
            {activeTab === "finance" && (
                <div className="space-y-6">
                    <div className="grid grid-cols-4 gap-4">
                        <KPICard
                            label="Doanh thu năm"
                            value={fmtM(totalRevenue)}
                            sub="Tổng 9 tháng"
                            trend={{val: "12.4%", up: true}}
                            color="bg-blue-500"
                        />
                        <KPICard
                            label="Lợi nhuận"
                            value={fmtM(totalProfit)}
                            sub="Sau chi phí"
                            trend={{val: "8.1%", up: true}}
                            color="bg-emerald-500"
                        />
                        <KPICard
                            label="Đã thu"
                            value={fmt(invoicePaid)}
                            sub={`${INVOICES.filter((i) => i.status === "paid").length} hóa đơn`}
                            color="bg-violet-500"
                        />
                        <KPICard
                            label="Chưa thu"
                            value={fmt(invoiceUnpaid)}
                            sub={`${INVOICES.filter((i) => i.status !== "paid").length} hóa đơn`}
                            trend={{val: "", up: false}}
                            color="bg-red-400"
                        />
                    </div>

                    <div className="grid grid-cols-3 gap-6">
                        <div
                            className="col-span-2 bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading
                                title="Doanh thu & Lợi nhuận theo tháng"
                                sub="Tháng 1 → 9 năm 2024"
                            />
                            <div
                                className="grid grid-cols-4 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide">
                                <span>Tháng</span>
                                <span className="text-right">Doanh thu</span>
                                <span className="text-right">Chi phí</span>
                                <span className="text-right">Lợi nhuận</span>
                            </div>
                            <div className="divide-y divide-gray-100">
                                {MOCK_MONTHLY_REVENUE.slice(0, 9).map((row) => (
                                    <div
                                        key={row.month}
                                        className="grid grid-cols-4 px-4 py-3 hover:bg-gray-50/50 transition-colors"
                                    >
                    <span className="text-xs font-semibold text-gray-700">
                      {row.month}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-blue-600 text-right">
                      {fmtM(row.doanhThu)}
                    </span>
                                        <span className="text-xs font-mono text-gray-400 text-right">
                      {fmtM(row.chiPhi)}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-emerald-600 text-right">
                      {fmtM(row.loiNhuan)}
                    </span>
                                    </div>
                                ))}
                            </div>
                        </div>
                        <div
                            className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading title="Trạng thái hóa đơn"/>
                            <DistributionList items={invoiceByStatus}/>
                        </div>
                    </div>

                    {/* Revenue breakdown table */}
                    <div
                        className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                        <SectionHeading title="Chi tiết hóa đơn" sub="Theo hợp đồng"/>
                        <div className="overflow-hidden">
                            <div
                                className="grid grid-cols-6 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide">
                                <span className="col-span-1">Mã HD</span>
                                <span className="col-span-1">Khách thuê</span>
                                <span className="col-span-1">Mặt bằng</span>
                                <span className="col-span-1">Tổng tiền</span>
                                <span className="col-span-1">Đến hạn</span>
                                <span className="col-span-1">Trạng thái</span>
                            </div>
                            {INVOICES.map((inv) => (
                                <div
                                    key={inv.id}
                                    className="grid grid-cols-6 px-4 py-3 border-b border-gray-50 hover:bg-gray-50/50 transition-colors"
                                >
                  <span className="text-xs font-mono text-gray-400">
                    {inv.id}
                  </span>
                                    <span className="text-xs font-medium text-gray-800">
                    {inv.tenant}
                  </span>
                                    <span className="text-xs font-semibold text-gray-700">
                    {inv.premiseName}
                  </span>
                                    <span className="text-xs font-mono font-semibold text-gray-800">
                    {fmt(inv.totalAmount)}
                  </span>
                                    <span className="text-xs text-gray-400">{inv.dueDate}</span>
                                    <InvoiceStatusBadge status={inv.status}/>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            )}

            {/* ── Accounts ── */}
            {activeTab === "accounts" && (
                <div className="space-y-6">
                    <div className="grid grid-cols-4 gap-4">
                        <KPICard
                            label="Tổng tài khoản"
                            value={String(totalAccounts)}
                            sub="Tất cả vai trò"
                            color="bg-blue-500"
                        />
                        <KPICard
                            label="Đang hoạt động"
                            value={String(activeAccounts)}
                            sub={`${Math.round((activeAccounts / totalAccounts) * 100)}% tổng số`}
                            trend={{val: "2 mới tháng này", up: true}}
                            color="bg-emerald-500"
                        />
                        <KPICard
                            label="Khách thuê"
                            value={String(ACCOUNTS.filter((a) => a.role === "TENANT").length)}
                            sub="Tài khoản thuê"
                            color="bg-amber-400"
                        />
                        <KPICard
                            label="Nhân viên vận hành"
                            value="4"
                            sub="Kỹ thuật + Thanh tra"
                            color="bg-violet-500"
                        />
                    </div>

                    <div className="grid grid-cols-3 gap-6">
                        <div
                            className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading title="Phân bổ vai trò"/>
                            <DistributionList items={accountsByRole}/>
                        </div>
                        <div
                            className="col-span-2 bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading
                                title="Đăng ký tài khoản mới theo tháng"
                                sub="Tháng 1 → 9 năm 2024"
                            />
                            <div
                                className="grid grid-cols-3 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide">
                                <span>Tháng</span>
                                <span className="text-right">Đăng ký mới</span>
                                <span className="text-right">Hủy khoản</span>
                            </div>
                            <div className="divide-y divide-gray-100">
                                {MOCK_ACCOUNT_MONTHLY.map((row) => (
                                    <div
                                        key={row.month}
                                        className="grid grid-cols-3 px-4 py-3 hover:bg-gray-50/50 transition-colors"
                                    >
                    <span className="text-xs font-semibold text-gray-700">
                      {row.month}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-blue-600 text-right">
                      {row.moiDangKy}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-red-500 text-right">
                      {row.huyKhoan}
                    </span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>

                    <div
                        className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                        <SectionHeading title="Danh sách tài khoản" sub="Tất cả vai trò"/>
                        <div className="overflow-hidden">
                            <div
                                className="grid grid-cols-5 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">
                                <span className="col-span-1">Họ tên</span>
                                <span className="col-span-1">Email</span>
                                <span className="col-span-1">Vai trò</span>
                                <span className="col-span-1">Ngày tạo</span>
                                <span className="col-span-1">Trạng thái</span>
                            </div>
                            {ACCOUNTS.map((a) => (
                                <div
                                    key={a.id}
                                    className="grid grid-cols-5 px-4 py-3 border-b border-gray-50 hover:bg-gray-50/50 transition-colors"
                                >
                  <span className="text-xs font-semibold text-gray-900">
                    {a.name}
                  </span>
                                    <span className="text-xs text-gray-500">{a.email}</span>
                                    <span className="text-xs font-medium text-gray-700">
                    {a.role}
                  </span>
                                    <span className="text-xs text-gray-400">{a.createdAt}</span>
                                    <span
                                        className={`text-xs font-semibold ${
                                            a.active ? "text-emerald-600" : "text-gray-400"
                                        }`}
                                    >
                    {a.active ? "Hoạt động" : "Vô hiệu"}
                  </span>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            )}

            {/* ── Premises ── */}
            {activeTab === "premises" && (
                <div className="space-y-6">
                    {premisesError && <div role="alert"
                                           className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                        <span>{premisesError}</span>
                        <button type="button"
                                onClick={() => setPremisesReload((current) => current + 1)}
                                className="rounded-lg border border-red-200 bg-white px-3 py-2 font-medium hover:bg-red-100">
                            Thử tải lại
                        </button>
                    </div>}
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
                        <KPICard
                            label="Tổng mặt bằng"
                            value={premisesLoading ? "…" : String(premises.length)}
                            sub="Từ dữ liệu hệ thống"
                            color="bg-blue-500"
                        />
                        <KPICard
                            label="Đang cho thuê"
                            value={premisesLoading ? "…" : String(premiseStatusCounts.rented)}
                            sub={`${occupancyRate}% lấp đầy`}
                            color="bg-emerald-500"
                        />
                        <KPICard
                            label="Còn trống"
                            value={premisesLoading ? "…" : String(premiseStatusCounts.available)}
                            sub="Sẵn sàng cho thuê"
                            color="bg-amber-400"
                        />
                        <KPICard
                            label="Tổng diện tích"
                            value={premisesLoading ? "…" : formatArea(totalPremiseArea)}
                            sub="Diện tích mặt bằng"
                            color="bg-violet-500"
                        />
                    </div>

                    <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                        <div
                            className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading title="Phân bổ trạng thái mặt bằng"/>
                            {premisesLoading
                                ? <p className="py-6 text-center text-sm text-gray-400">Đang tải thống kê...</p>
                                : premisesError
                                    ? <p className="py-6 text-center text-sm text-gray-400">Không thể tải dữ liệu mặt bằng.</p>
                                    : <DistributionList items={premisesByStatus}/>}
                        </div>
                        <div
                            className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading title="Tỷ lệ lấp đầy theo địa điểm"/>
                            {premisesLoading
                                ? <p className="py-6 text-center text-sm text-gray-400">Đang tải thống kê...</p>
                                : premisesError
                                    ? <p className="py-6 text-center text-sm text-gray-400">Không thể tải dữ liệu mặt bằng.</p>
                                    : premisesByLocation.length === 0
                                        ? <p className="py-6 text-center text-sm text-gray-400">Chưa có dữ liệu mặt bằng.</p>
                                        : <div className="mt-2 max-h-80 space-y-4 overflow-y-auto">
                                {premisesByLocation.map((b) => (
                                    <div key={b.id}>
                                        <div className="flex justify-between items-center mb-1.5">
                      <span className="text-xs font-medium text-gray-700 truncate max-w-[200px]">
                        {b.name}
                      </span>
                                            <span className="text-xs font-mono font-semibold text-gray-500">
                        {b.rented}/{b.total}
                      </span>
                                        </div>
                                        <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                                            <div
                                                className="h-full rounded-full bg-blue-500 transition-all"
                                                style={{width: `${(b.rented / b.total) * 100}%`}}
                                            />
                                        </div>
                                        <p className="text-xs text-gray-400 mt-1">
                                            {Math.round((b.rented / b.total) * 100)}% lấp đầy
                                        </p>
                                    </div>
                                ))}
                            </div>}
                        </div>
                    </div>

                    <div
                        className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                        <SectionHeading title="Danh sách mặt bằng" sub="Dữ liệu lấy từ hệ thống"/>
                        <div className="mb-4 flex flex-wrap gap-1 rounded-lg bg-gray-100 p-1 w-fit">
                            {([
                                {status: "all" as const, label: "Tất cả", count: premises.length},
                                {status: "available" as const, label: "Còn trống", count: premiseStatusCounts.available},
                                {status: "rented" as const, label: "Đang thuê", count: premiseStatusCounts.rented},
                                {status: "reserved" as const, label: "Đã đặt cọc", count: premiseStatusCounts.reserved},
                                {status: "maintenance" as const, label: "Bảo trì", count: premiseStatusCounts.maintenance},
                            ]).map(({status, label, count}) => (
                                <button
                                    key={status}
                                    type="button"
                                    onClick={() => setPremiseFilter(status)}
                                    className={`rounded-md px-3 py-1.5 text-xs font-medium transition-all ${
                                        premiseFilter === status
                                            ? "bg-white text-gray-900 shadow-sm"
                                            : "text-gray-500 hover:text-gray-700"
                                    }`}
                                >
                                    {label}<span className="ml-1.5 text-gray-400">{count}</span>
                                </button>
                            ))}
                        </div>
                        <BaseTable
                            rows={filteredPremises}
                            getRowKey={(premise) => premise.premiseId}
                            isHaveStt={false}
                            paginationResetKey={premiseFilter}
                            loading={premisesLoading}
                            emptyMessage={premisesError
                                ? "Không thể tải danh sách mặt bằng."
                                : premiseFilter === "all"
                                    ? "Chưa có mặt bằng nào."
                                    : "Không có mặt bằng ở trạng thái này."}
                            tableClassName="min-w-[760px] w-full text-sm"
                            columns={[
                                {
                                    key: "id",
                                    header: "ID",
                                    cellClassName: "font-mono text-xs text-gray-400",
                                    render: (premise) => premise.premiseId,
                                },
                                {
                                    key: "name",
                                    header: "Tên",
                                    cellClassName: "text-xs font-semibold text-gray-900",
                                    render: (premise) => premise.premiseName,
                                },
                                {
                                    key: "position",
                                    header: "Tầng/Vị trí",
                                    cellClassName: "text-xs text-gray-500",
                                    render: (premise) => `T${premise.premiseFloor} - ${premise.premisePosition}`,
                                },
                                {
                                    key: "area",
                                    header: "Diện tích",
                                    cellClassName: "font-mono text-xs text-gray-700",
                                    render: (premise) => `${premise.premiseArea} m²`,
                                },
                                {
                                    key: "location",
                                    header: "Địa chỉ",
                                    cellClassName: "max-w-48 truncate text-xs text-gray-400",
                                    render: (premise) => (locations.find((location) =>
                                        location.locationId === premise.premiseLocationId
                                    )?.locationAddress ?? premise.premiseLocation?.locationAddress
                                        ?? "Chưa xác định địa chỉ").split(",")[0],
                                },
                                {
                                    key: "status",
                                    header: "Trạng thái",
                                    render: (premise) =>
                                        <PremiseStatusBadge status={toPremiseStatus(premise.premiseStatus)}/>,
                                },
                            ]}
                        />
                    </div>
                </div>
            )}

            {/* ── Contracts ── */}
            {activeTab === "contracts" && (
                <div className="space-y-6">
                    <div className="grid grid-cols-4 gap-4">
                        <KPICard
                            label="Tổng hợp đồng"
                            value={String(CONTRACTS.length)}
                            sub="Từ đầu năm"
                            color="bg-blue-500"
                        />
                        <KPICard
                            label="Đang hiệu lực"
                            value={String(
                                CONTRACTS.filter((c) => c.status === "Signed").length,
                            )}
                            sub="Đã ký kết"
                            trend={{val: "2 mới tháng 9", up: true}}
                            color="bg-emerald-500"
                        />
                        <KPICard
                            label="Chờ xử lý"
                            value={String(
                                CONTRACTS.filter(
                                    (c) =>
                                        c.status === "PendingApproval" ||
                                        c.status === "PendingSignature",
                                ).length,
                            )}
                            sub="Cần hành động"
                            color="bg-amber-400"
                        />
                        <KPICard
                            label="Tổng giá trị"
                            value={fmtM(CONTRACTS.reduce((s, c) => s + c.rentalPrice, 0))}
                            sub="Giá thuê/tháng"
                            trend={{val: "5.2%", up: true}}
                            color="bg-violet-500"
                        />
                    </div>

                    <div className="grid grid-cols-3 gap-6">
                        <div
                            className="col-span-2 bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading
                                title="Hợp đồng mới ký theo tháng"
                                sub="Tháng 1 → 9 năm 2024"
                            />
                            <div
                                className="grid grid-cols-4 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide">
                                <span>Tháng</span>
                                <span className="text-right">Mới ký</span>
                                <span className="text-right">Hết hạn</span>
                                <span className="text-right">Chuyển nhượng</span>
                            </div>
                            <div className="divide-y divide-gray-100">
                                {MOCK_CONTRACT_MONTHLY.map((row) => (
                                    <div
                                        key={row.month}
                                        className="grid grid-cols-4 px-4 py-3 hover:bg-gray-50/50 transition-colors"
                                    >
                    <span className="text-xs font-semibold text-gray-700">
                      {row.month}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-blue-600 text-right">
                      {row.moiKy}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-amber-600 text-right">
                      {row.hetHan}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-violet-600 text-right">
                      {row.chuyenGia}
                    </span>
                                    </div>
                                ))}
                            </div>
                        </div>
                        <div
                            className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading title="Theo trạng thái"/>
                            <DistributionList
                                items={contractsByStatus.filter((c) => c.value > 0)}
                            />
                        </div>
                    </div>

                    <div
                        className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                        <SectionHeading title="Chi tiết hợp đồng"/>
                        <div>
                            <div
                                className="grid grid-cols-7 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">
                                <span>Mã HĐ</span>
                                <span className="col-span-2">Khách thuê</span>
                                <span>Mặt bằng</span>
                                <span>Tiền cọc</span>
                                <span>Giá/tháng</span>
                                <span>Trạng thái</span>
                            </div>
                            {CONTRACTS.map((c) => (
                                <div
                                    key={c.id}
                                    className="grid grid-cols-7 px-4 py-3 border-b border-gray-50 hover:bg-gray-50/50 transition-colors items-center"
                                >
                  <span className="text-xs font-mono text-gray-400">
                    {c.id.replace("CTR-", "")}
                  </span>
                                    <div className="col-span-2">
                                        <div className="text-xs font-semibold text-gray-900">
                                            {c.tenant}
                                        </div>
                                        <div className="text-xs text-gray-400">
                                            {c.accountEmail}
                                        </div>
                                    </div>
                                    <span className="text-xs font-semibold text-gray-700">
                    {c.premiseName}
                  </span>
                                    <span className="text-xs font-mono text-gray-600">
                    {fmtM(c.deposit)}
                  </span>
                                    <span className="text-xs font-mono font-semibold text-gray-800">
                    {fmtM(c.rentalPrice)}
                  </span>
                                    <ContractStatusBadge status={c.status}/>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            )}

            {/* ── Violations ── */}
            {activeTab === "violations" && (
                <div className="space-y-6">
                    <div className="grid grid-cols-4 gap-4">
                        <KPICard
                            label="Tổng vi phạm"
                            value={String(VIOLATIONS.length)}
                            sub="Từ đầu năm"
                            color="bg-red-400"
                        />
                        <KPICard
                            label="Chưa xử lý"
                            value={String(violationUnresolved)}
                            sub="Cần hành động"
                            trend={{val: `${violationUnresolved} đang chờ`, up: false}}
                            color="bg-orange-400"
                        />
                        <KPICard
                            label="Đã xử lý"
                            value={String(violationResolved)}
                            sub={`${Math.round((violationResolved / VIOLATIONS.length) * 100)}% tỷ lệ xử lý`}
                            color="bg-emerald-500"
                        />
                        <KPICard
                            label="Tổng tiền phạt"
                            value={fmt(totalPenalty)}
                            sub="Đã áp dụng"
                            color="bg-violet-500"
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-6">
                        <div
                            className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading
                                title="Vi phạm theo tháng"
                                sub="Số lượng & xử lý"
                            />
                            <div
                                className="grid grid-cols-4 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide">
                                <span>Tháng</span>
                                <span className="text-right">Tổng</span>
                                <span className="text-right">Đã xử lý</span>
                                <span className="text-right">Chưa xử lý</span>
                            </div>
                            <div className="divide-y divide-gray-100">
                                {MOCK_VIOLATION_MONTHLY.map((row) => (
                                    <div
                                        key={row.month}
                                        className="grid grid-cols-4 px-4 py-3 hover:bg-gray-50/50 transition-colors"
                                    >
                    <span className="text-xs font-semibold text-gray-700">
                      {row.month}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-gray-800 text-right">
                      {row.soLuong}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-emerald-600 text-right">
                      {row.daXuLy}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-red-500 text-right">
                      {row.chuaXuLy}
                    </span>
                                    </div>
                                ))}
                            </div>
                        </div>
                        <div
                            className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading title="Tiền phạt theo tháng" sub="Đơn vị: VNĐ"/>
                            <div
                                className="grid grid-cols-3 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide">
                                <span>Tháng</span>
                                <span className="text-right">Số vụ</span>
                                <span className="text-right">Tổng tiền phạt</span>
                            </div>
                            <div className="divide-y divide-gray-100">
                                {MOCK_VIOLATION_MONTHLY.map((row) => (
                                    <div
                                        key={row.month}
                                        className="grid grid-cols-3 px-4 py-3 hover:bg-gray-50/50 transition-colors"
                                    >
                    <span className="text-xs font-semibold text-gray-700">
                      {row.month}
                    </span>
                                        <span className="text-xs font-mono text-gray-500 text-right">
                      {row.soLuong}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-red-600 text-right">
                      {fmt(row.tongPhat)}
                    </span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>

                    <div
                        className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                        <SectionHeading title="Danh sách vi phạm" sub="Lịch sử ghi nhận"/>
                        <div>
                            <div
                                className="grid grid-cols-6 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">
                                <span>Mã VP</span>
                                <span>HĐ</span>
                                <span className="col-span-2">Nội dung</span>
                                <span>Tiền phạt</span>
                                <span>Trạng thái</span>
                            </div>
                            {VIOLATIONS.map((v) => (
                                <div
                                    key={v.id}
                                    className="grid grid-cols-6 px-4 py-3 border-b border-gray-50 hover:bg-gray-50/50 transition-colors items-center"
                                >
                  <span className="text-xs font-mono text-gray-400">
                    {v.id}
                  </span>
                                    <span className="text-xs font-mono text-gray-500">
                    {v.contractId.replace("CTR-2024-", "")}
                  </span>
                                    <span className="col-span-2 text-xs text-gray-700 truncate">
                    {v.content}
                  </span>
                                    <span className="text-xs font-mono font-semibold text-red-600">
                    {fmt(v.penaltyAmount)}
                  </span>
                                    <span
                                        className={`text-xs font-semibold ${
                                            v.resolved ? "text-emerald-600" : "text-orange-500"
                                        }`}
                                    >
                    {v.resolved ? "Đã xử lý" : "Chưa xử lý"}
                  </span>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}
