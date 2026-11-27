import {useEffect, useState} from "react"
import {contractApi} from "@/api/contracts"
import type {ContractModel, ContractViolationModel, MonthlyInvoiceModel} from "@/types/contracts"
import {recordStatuses, statuses as contractStatuses} from "@/types/contracts"
import {BaseTable, ContractStatusBadge, formatProfileFullName, InvoiceStatusBadge, ModulePageHeader, PremiseStatusBadge} from "./shared"
import {fmt} from "../../scripts/format"
import {identityApi} from "@/api/identity"
import type {AccountModel} from "@/types/identity"
import type {AccountProfileOption} from "@/types/identity"
import type {Contract, Invoice, Violation} from "@/types/modules"
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
                style={{fontFamily: "Open Sans, sans-serif"}}
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
                    style={{fontFamily: "Open Sans, sans-serif"}}
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
        default:
            throw new Error(`Trạng thái mặt bằng không được hỗ trợ: ${status}`)
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
    const [invoiceFilter, setInvoiceFilter] = useState<Invoice["status"] | "all">("all")
    const [accountStatusFilter, setAccountStatusFilter] = useState<"all" | "active" | "inactive">("all")
    const [accountRoleFilter, setAccountRoleFilter] = useState("all")
    const [contractFilter, setContractFilter] = useState<Contract["status"] | "all">("all")
    const [violationFilter, setViolationFilter] = useState<"all" | "resolved" | "unresolved">("all")
    const [premisesLoading, setPremisesLoading] = useState(false)
    const [premisesError, setPremisesError] = useState<string | null>(null)
    const [premisesReload, setPremisesReload] = useState(0)
    const [accounts, setAccounts] = useState<AccountModel[]>([])
    const [customerAccounts, setCustomerAccounts] = useState<AccountProfileOption[]>([])
    const [accountsLoading, setAccountsLoading] = useState(false)
    const [accountsError, setAccountsError] = useState<string | null>(null)
    const [accountsReload, setAccountsReload] = useState(0)
    const [accountSearch, setAccountSearch] = useState("")
    const [contractModels, setContractModels] = useState<ContractModel[]>([])
    const [invoiceModels, setInvoiceModels] = useState<MonthlyInvoiceModel[]>([])
    const [violationModels, setViolationModels] = useState<ContractViolationModel[]>([])
    const [analyticsLoading, setAnalyticsLoading] = useState(false)
    const [analyticsError, setAnalyticsError] = useState<string | null>(null)

    useEffect(() => {
        if (!["finance", "contracts", "violations"].includes(activeTab)) return
        const controller = new AbortController()
        setAnalyticsLoading(true)
        setAnalyticsError(null)
        const loadPages = async <T,>(loadPage: (cursor: string | null, signal: AbortSignal) => Promise<{
            items: T[]; nextCursor: string | null; isHasMore: boolean
        }>) => {
            const items: T[] = []
            let cursor: string | null = null
            do {
                const page = await loadPage(cursor, controller.signal)
                items.push(...page.items)
                cursor = page.isHasMore ? page.nextCursor : null
            } while (cursor && !controller.signal.aborted)
            return items
        }
        void Promise.all([
            loadPages((cursor, signal) => contractApi.contracts(cursor, signal)),
            loadPages((cursor, signal) => contractApi.invoices(cursor, signal)),
            loadPages((cursor, signal) => contractApi.violations(cursor, undefined, signal)),
            premiseApi.getPremise(controller.signal),
        ]).then(([contractItems, invoiceItems, violationItems, premiseItems]) => {
            if (controller.signal.aborted) return
            setContractModels(contractItems)
            setInvoiceModels(invoiceItems)
            setViolationModels(violationItems)
            setPremises(premiseItems)
        }).catch((cause: unknown) => {
            if (!controller.signal.aborted) {
                setAnalyticsError(cause instanceof Error ? cause.message : "Không thể tải dữ liệu thống kê.")
            }
        }).finally(() => {
            if (!controller.signal.aborted) setAnalyticsLoading(false)
        })
        return () => controller.abort()
    }, [activeTab])

    useEffect(() => {
        if (!["finance", "contracts", "violations"].includes(activeTab)) return

        let isCurrent = true
        void identityApi.getAccountsByRoleCode("Customer")
            .then((items) => {
                if (isCurrent) setCustomerAccounts(items)
            })
            .catch((cause: unknown) => {
                if (isCurrent) {
                    setAnalyticsError(cause instanceof Error ? cause.message : "Không thể tải danh sách khách thuê.")
                }
            })
        return () => {
            isCurrent = false
        }
    }, [activeTab])

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

    useEffect(() => {
        if (activeTab !== "accounts") return

        let isCurrent = true
        setAccountsLoading(true)
        setAccountsError(null)
        void identityApi.getAllAccountsForStatistics()
            .then((items) => {
                if (isCurrent) setAccounts(items)
            })
            .catch((cause: unknown) => {
                if (isCurrent) {
                    setAccountsError(cause instanceof Error ? cause.message : "Không thể tải thống kê tài khoản.")
                }
            })
            .finally(() => {
                if (isCurrent) setAccountsLoading(false)
            })
        return () => {
            isCurrent = false
        }
    }, [activeTab, accountsReload])

    const tabs = [
        {id: "finance" as const, label: "Tài chính"},
        {id: "accounts" as const, label: "Tài khoản"},
        {id: "premises" as const, label: "Mặt bằng"},
        {id: "contracts" as const, label: "Hợp đồng"},
        {id: "violations" as const, label: "Vi phạm"},
    ]

    const tenantAccountsById = new Map(customerAccounts.map((account) => [
        account.accountId.toLowerCase(),
        account,
    ]))
    const tenantName = (accountId: string) => {
        const account = tenantAccountsById.get(accountId.toLowerCase())
        return formatProfileFullName(account?.userProfile) || account?.accountEmail || "Không rõ khách thuê"
    }
    const premiseNamesByContractId = (contractId: string) => {
        const premiseIds = invoiceModels
            .filter((invoice) => invoice.invoiceContractId === contractId)
            .flatMap((invoice) => (invoice.invoiceDetails ?? []).map((detail) => detail.invoiceDetailPremiseId))
        return [...new Set(premiseIds.map((premiseId) =>
            premises.find((premise) => premise.premiseId === premiseId)?.premiseName ?? "Mặt bằng không còn hoạt động",
        ))].join(", ") || "—"
    }
    const contractById = new Map(contractModels.map((contract) => [contract.contractId, contract]))
    const CONTRACTS: Contract[] = contractModels.map((contract) => ({
        id: contract.contractId,
        accountEmail: tenantAccountsById.get(contract.contractAccountId.toLowerCase())?.accountEmail ?? "",
        tenant: tenantName(contract.contractAccountId),
        premiseName: premiseNamesByContractId(contract.contractId),
        deposit: contract.contractDeposit,
        rentalPrice: contract.contractRentalPrice,
        returnDate: contract.contractPremiseReturnDate,
        status: contract.contractStatus,
        createdAt: contract.contractCreatedAt,
    }))
    const INVOICES: Invoice[] = invoiceModels.map((invoice) => ({
        id: invoice.invoiceId,
        contractId: invoice.invoiceContractId,
        tenant: tenantName(contractById.get(invoice.invoiceContractId)?.contractAccountId ?? ""),
        premiseName: (invoice.invoiceDetails ?? []).map((detail) =>
            premises.find((premise) => premise.premiseId === detail.invoiceDetailPremiseId)?.premiseName ?? "Mặt bằng không còn hoạt động",
        ).join(", ") || "—",
        totalAmount: invoice.invoiceTotalAmount ?? 0,
        paymentDate: invoice.invoicePaymentDate,
        dueDate: invoice.invoiceDueDate,
        status: invoice.invoiceStatus.toLowerCase() as Invoice["status"],
    }))
    const VIOLATIONS: Violation[] = violationModels.map((violation) => ({
        id: violation.violationId,
        contractId: violation.contractId,
        tenant: tenantName(contractById.get(violation.contractId)?.contractAccountId ?? ""),
        content: violation.violationContent,
        penaltyAmount: violation.violationPenaltyAmount ?? 0,
        date: violation.violationDate,
        dueDate: violation.violationDueDate,
        resolved: violation.violationIsResolved,
    }))

    const invoicePaid = INVOICES.filter((invoice) => invoice.status === "paid").reduce((sum, invoice) => sum + invoice.totalAmount, 0)
    const invoiceUnpaid = INVOICES.filter((invoice) => invoice.status !== "paid").reduce((sum, invoice) => sum + invoice.totalAmount, 0)
    const totalRevenue = invoicePaid

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

    const monthKey = (date: string) => {
        const parsed = new Date(date)
        return Number.isNaN(parsed.valueOf()) ? "unknown" : `${parsed.getFullYear()}-${String(parsed.getMonth() + 1).padStart(2, "0")}`
    }
    const monthLabel = (key: string) => key === "unknown" ? "Không rõ ngày" : new Intl.DateTimeFormat("vi-VN", {month: "short", year: "numeric"}).format(new Date(`${key}-01T00:00:00`))
    const monthKeys = (dates: string[]) => [...new Set(dates.map(monthKey))].sort().reverse().slice(0, 9).reverse()
    const monthlyRevenue = monthKeys(invoiceModels.map((invoice) => invoice.invoicePaymentDate)).map((key) => {
        const monthInvoices = invoiceModels.filter((invoice) => monthKey(invoice.invoicePaymentDate) === key)
        return {
            month: monthLabel(key),
            doanhThu: monthInvoices.filter((invoice) => invoice.invoiceStatus === "Paid").reduce((sum, invoice) => sum + (invoice.invoiceTotalAmount ?? 0), 0),
            soHoaDon: monthInvoices.length,
            quaHan: monthInvoices.filter((invoice) => invoice.invoiceStatus === "Overdue").length,
        }
    })
    const monthlyContracts = monthKeys(contractModels.map((contract) => contract.contractCreatedAt)).map((key) => {
        const monthContracts = contractModels.filter((contract) => monthKey(contract.contractCreatedAt) === key)
        return {
            month: monthLabel(key),
            total: monthContracts.length,
            signed: monthContracts.filter((contract) => contract.contractStatus === "Signed").length,
            pending: monthContracts.filter((contract) => contract.contractStatus === "PendingApproval" || contract.contractStatus === "PendingSignature").length,
        }
    })
    const monthlyViolations = monthKeys(violationModels.map((violation) => violation.violationDate)).map((key) => {
        const monthViolations = violationModels.filter((violation) => monthKey(violation.violationDate) === key)
        return {
            month: monthLabel(key),
            soLuong: monthViolations.length,
            daXuLy: monthViolations.filter((violation) => violation.violationIsResolved).length,
            chuaXuLy: monthViolations.filter((violation) => !violation.violationIsResolved).length,
            tongPhat: monthViolations.reduce((sum, violation) => sum + (violation.violationPenaltyAmount ?? 0), 0),
        }
    })

    const totalAccounts = accounts.length
    const activeAccounts = accounts.filter((account) => account.accountIsActive).length
    const accountHasRole = (account: AccountModel, roleCode: string) =>
        account.roles.some((role) => role.roleCode.toLowerCase() === roleCode.toLowerCase())
    const tenantAccounts = accounts.filter((account) => accountHasRole(account, "Customer")).length
    const internalRoleCodes = new Set(["inspector", "manager", "admin"])
    const isInternalHighLevelAccount = (account: AccountModel) =>
        account.roles.some((role) => internalRoleCodes.has(role.roleCode.toLowerCase()))
    const operationalAccounts = accounts.filter(isInternalHighLevelAccount).length
    const roleCounts = new Map<string, { name: string; value: number }>()
    for (const account of accounts) {
        if (account.roles.length === 0) {
            const current = roleCounts.get("UNASSIGNED")
            roleCounts.set("UNASSIGNED", {name: "Chưa gán vai trò", value: (current?.value ?? 0) + 1})
            continue
        }
        for (const role of account.roles) {
            const current = roleCounts.get(role.roleCode)
            roleCounts.set(role.roleCode, {name: role.roleName || role.roleCode, value: (current?.value ?? 0) + 1})
        }
    }
    const roleColors = ["bg-violet-500", "bg-blue-500", "bg-emerald-500", "bg-amber-500", "bg-rose-500", "bg-cyan-500"]
    const accountsByRole = [...roleCounts.values()]
        .sort((left, right) => right.value - left.value)
        .map((item, index) => ({...item, color: roleColors[index % roleColors.length]}))

    const currentMonth = new Date()
    const recentMonths = Array.from({length: 3}, (_, index) => {
        const monthStart = new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 2 + index, 1)
        const count = accounts.filter((account) => {
            const createdAt = new Date(account.accountCreatedAt)
            return !Number.isNaN(createdAt.getTime()) &&
                createdAt.getFullYear() === monthStart.getFullYear() &&
                createdAt.getMonth() === monthStart.getMonth()
        }).length
        return {
            month: monthStart.toLocaleDateString("vi-VN", {month: "long", year: "numeric"}),
            count,
        }
    })
    const normalizedAccountSearch = accountSearch.trim().toLocaleLowerCase("vi")
    const filteredAccounts = accounts.filter((account) => {
        const fullName = [account.userProfile?.userProfileFirstName, account.userProfile?.userProfileLastName]
            .filter(Boolean).join(" ")
        const roles = account.roles.map((role) => `${role.roleName} ${role.roleCode}`).join(" ")
        return !normalizedAccountSearch ||
            `${fullName} ${account.accountEmail} ${roles}`.toLocaleLowerCase("vi").includes(normalizedAccountSearch)
    })
    const allInternalAccounts = accounts.filter(isInternalHighLevelAccount)
    const internalAccounts = filteredAccounts.filter(isInternalHighLevelAccount)
    const internalRoleOptions = [...new Map(allInternalAccounts.flatMap((account) =>
        account.roles
            .filter((role) => internalRoleCodes.has(role.roleCode.toLowerCase()))
            .map((role) => [role.roleCode, role.roleName || role.roleCode] as const),
    ))]
    const filteredInternalAccounts = internalAccounts.filter((account) =>
        (accountStatusFilter === "all" ||
            account.accountIsActive === (accountStatusFilter === "active")) &&
        (accountRoleFilter === "all" ||
            account.roles.some((role) => role.roleCode === accountRoleFilter)),
    )
    const filteredInvoices = invoiceFilter === "all"
        ? INVOICES
        : INVOICES.filter((invoice) => invoice.status === invoiceFilter)
    const filteredContracts = contractFilter === "all"
        ? CONTRACTS
        : CONTRACTS.filter((contract) => contract.status === contractFilter)
    const filteredViolations = violationFilter === "all"
        ? VIOLATIONS
        : VIOLATIONS.filter((violation) => violation.resolved === (violationFilter === "resolved"))
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
    }, new Map<string, { id: string; name: string; total: number; rented: number }>()).values()]
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
            <ModulePageHeader title="Thống kê & Phân tích"
                              description="Tổng hợp số liệu theo từng phân hệ"/>

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
            {analyticsError && <p role="alert" className="mb-4 rounded-lg border border-red-100 bg-red-50 px-4 py-3 text-sm text-red-700">{analyticsError}</p>}

            {/* ── Finance ── */}
            {activeTab === "finance" && (
                <div className="space-y-6">
                    <div className="grid grid-cols-4 gap-4">
                        <KPICard
                            label="Doanh thu năm"
                            value={fmtM(totalRevenue)}
                            sub="Từ hóa đơn đã thanh toán"
                            color="bg-blue-500"
                        />
                        <KPICard
                            label="Số hóa đơn"
                            value={String(invoiceModels.length)}
                            sub="Dữ liệu từ Contract API"
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
                            color="bg-red-400"
                        />
                    </div>

                    <div className="grid grid-cols-3 gap-6">
                        <div
                            className="col-span-2 bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading
                                title="Doanh thu hóa đơn theo tháng"
                                sub="Theo ngày thanh toán trong dữ liệu hiện có"
                            />
                            <div
                                className="grid grid-cols-4 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide">
                                <span>Tháng</span>
                                <span className="text-right">Doanh thu</span>
                                <span className="text-right">Số hóa đơn</span>
                                <span className="text-right">Quá hạn</span>
                            </div>
                            <div className="divide-y divide-gray-100">
                                {monthlyRevenue.map((row) => (
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
                                        <span className="text-xs font-mono text-gray-500 text-right">
                      {row.soHoaDon}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-red-500 text-right">
                      {row.quaHan}
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
                        <div className="mb-4 flex flex-wrap gap-1 rounded-lg bg-gray-100 p-1 w-fit">
                            {([
                                {status: "all" as const, label: "Tất cả", count: INVOICES.length},
                                {status: "unpaid" as const, label: "Chưa thanh toán", count: INVOICES.filter((invoice) => invoice.status === "unpaid").length},
                                {status: "paid" as const, label: "Đã thanh toán", count: INVOICES.filter((invoice) => invoice.status === "paid").length},
                                {status: "overdue" as const, label: "Quá hạn", count: INVOICES.filter((invoice) => invoice.status === "overdue").length},
                            ]).map(({status, label, count}) => (
                                <button
                                    key={status}
                                    type="button"
                                    onClick={() => setInvoiceFilter(status)}
                                    className={`rounded-md px-3 py-1.5 text-xs font-medium transition-all ${
                                        invoiceFilter === status
                                            ? "bg-white text-gray-900 shadow-sm"
                                            : "text-gray-500 hover:text-gray-700"
                                    }`}
                                >
                                    {label}<span className="ml-1.5 text-gray-400">{count}</span>
                                </button>
                            ))}
                        </div>
                        <BaseTable
                            rows={filteredInvoices}
                            getRowKey={(invoice) => invoice.id}
                            isHaveStt={false}
                            paginationResetKey={invoiceFilter}
                            loading={analyticsLoading}
                            emptyMessage={analyticsError ? "Không thể tải danh sách hóa đơn." : "Chưa có hóa đơn phù hợp."}
                            tableClassName="min-w-[900px] w-full text-sm"
                            rowClassName={() => "hover:bg-blue-50/40 transition-colors"}
                            columns={[
                                {
                                    key: "id",
                                    header: "Mã HĐ",
                                    cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-500",
                                    render: (invoice) => invoice.id,
                                },
                                {
                                    key: "tenant",
                                    header: "Khách thuê",
                                    cellClassName: "max-w-48 truncate px-4 py-3 text-xs text-gray-700",
                                    render: (invoice) => invoice.tenant,
                                },
                                {
                                    key: "premise",
                                    header: "Mặt bằng",
                                    cellClassName: "max-w-56 truncate px-4 py-3 text-xs text-gray-700",
                                    render: (invoice) => invoice.premiseName,
                                },
                                {
                                    key: "amount",
                                    header: "Tổng tiền",
                                    headerClassName: "px-4 py-3 text-right font-medium",
                                    cellClassName: "whitespace-nowrap px-4 py-3 text-right font-mono text-xs font-semibold text-gray-800",
                                    render: (invoice) => fmt(invoice.totalAmount),
                                },
                                {
                                    key: "dueDate",
                                    header: "Đến hạn",
                                    cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500",
                                    render: (invoice) => invoice.dueDate,
                                },
                                {
                                    key: "status",
                                    header: "Trạng thái",
                                    render: (invoice) => <InvoiceStatusBadge status={invoice.status}/>,
                                },
                            ]}
                        />
                    </div>
                </div>
            )}

            {/* ── Accounts ── */}
            {activeTab === "accounts" && (
                <div className="space-y-6">
                    <p className="text-xs text-gray-500">Số liệu tài khoản được tải trực tiếp từ Identity API.</p>
                    {accountsError && (
                        <div role="alert"
                             className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                            <span>{accountsError}</span>
                            <button
                                type="button"
                                onClick={() => setAccountsReload((current) => current + 1)}
                                className="rounded-lg border border-red-200 bg-white px-3 py-2 font-medium hover:bg-red-100"
                            >Thử tải lại
                            </button>
                        </div>
                    )}
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
                        <KPICard
                            label="Tổng tài khoản"
                            value={accountsLoading ? "…" : accountsError ? "—" : String(totalAccounts)}
                            sub="Tất cả tài khoản trong hệ thống"
                            color="bg-blue-500"
                        />
                        <KPICard
                            label="Đang hoạt động"
                            value={accountsLoading ? "…" : accountsError ? "—" : String(activeAccounts)}
                            sub={accountsError ? "Không có dữ liệu" : totalAccounts ? `${Math.round((activeAccounts / totalAccounts) * 100)}% tổng số` : "Chưa có tài khoản"}
                            color="bg-emerald-500"
                        />
                        <KPICard
                            label="Khách thuê"
                            value={accountsLoading ? "…" : accountsError ? "—" : String(tenantAccounts)}
                            sub="Có vai trò Customer"
                            color="bg-amber-400"
                        />
                        <KPICard
                            label="Nhân viên vận hành"
                            value={accountsLoading ? "…" : accountsError ? "—" : String(operationalAccounts)}
                            sub="Inspector, Manager hoặc Admin"
                            color="bg-violet-500"
                        />
                    </div>

                    <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
                        <div
                            className="rounded-2xl border border-gray-100 bg-white p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading title="Phân bổ vai trò" sub="Tài khoản được đếm theo từng vai trò"/>
                            {accountsLoading ? (
                                <p className="py-8 text-center text-sm text-gray-400">Đang tải tài khoản...</p>
                            ) : accountsError ? (
                                <p className="py-8 text-center text-sm text-gray-400">Không có dữ liệu để phân bổ.</p>
                            ) : accountsByRole.length ? (
                                <DistributionList items={accountsByRole}/>
                            ) : (
                                <p className="py-8 text-center text-sm text-gray-400">Chưa có dữ liệu tài khoản.</p>
                            )}
                        </div>
                        <div
                            className="rounded-2xl border border-gray-100 bg-white p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)] xl:col-span-2">
                            <SectionHeading
                                title="Đăng ký tài khoản mới"
                                sub="3 tháng gần nhất"
                            />
                            <div
                                className="grid grid-cols-2 rounded-lg bg-gray-50 px-4 py-2.5 text-xs font-semibold uppercase tracking-wide text-gray-400">
                                <span>Tháng</span>
                                <span className="text-right">Đăng ký mới</span>
                            </div>
                            <div className="divide-y divide-gray-100">
                                {recentMonths.map((row) => (
                                    <div key={row.month} className="grid grid-cols-2 px-4 py-3 hover:bg-gray-50/50">
                                        <span className="text-xs font-semibold text-gray-700">{row.month}</span>
                                        <span
                                            className="text-right font-mono text-xs font-semibold text-blue-600">{accountsLoading || accountsError ? "—" : row.count}</span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>

                    <div
                        className="rounded-2xl border border-gray-100 bg-white p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                        <div className="mb-5 flex flex-wrap items-end justify-between gap-3">
                            <div>
                                <h2 className="text-base font-bold text-gray-900"
                                    style={{fontFamily: "Open Sans, sans-serif"}}>Tài khoản nội bộ cấp cao</h2>
                                <p className="mt-0.5 text-xs text-gray-400">{accountsLoading ? "Đang tải..." : `${filteredInternalAccounts.length} tài khoản phù hợp · không gồm Customer và Employee`}</p>
                            </div>
                            <input
                                type="search"
                                value={accountSearch}
                                onChange={(event) => setAccountSearch(event.target.value)}
                                placeholder="Tìm tên, email hoặc vai trò nội bộ"
                                aria-label="Tìm tài khoản nội bộ cấp cao"
                                className="w-full max-w-sm rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-100"
                            />
                        </div>
                        <div className="mb-4 flex flex-wrap items-center gap-3">
                            <div className="flex flex-wrap gap-1 rounded-lg bg-gray-100 p-1">
                                {([
                                    {status: "all" as const, label: "Tất cả", count: internalAccounts.length},
                                    {status: "active" as const, label: "Hoạt động", count: internalAccounts.filter((account) => account.accountIsActive).length},
                                    {status: "inactive" as const, label: "Vô hiệu", count: internalAccounts.filter((account) => !account.accountIsActive).length},
                                ]).map(({status, label, count}) => (
                                    <button
                                        key={status}
                                        type="button"
                                        onClick={() => setAccountStatusFilter(status)}
                                        className={`rounded-md px-3 py-1.5 text-xs font-medium transition-all ${
                                            accountStatusFilter === status
                                                ? "bg-white text-gray-900 shadow-sm"
                                                : "text-gray-500 hover:text-gray-700"
                                        }`}
                                    >
                                        {label}<span className="ml-1.5 text-gray-400">{count}</span>
                                    </button>
                                ))}
                            </div>
                            <select
                                aria-label="Lọc theo vai trò"
                                className="rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm text-gray-700"
                                value={accountRoleFilter}
                                onChange={(event) => setAccountRoleFilter(event.target.value)}
                            >
                                <option value="all">Tất cả vai trò</option>
                                {internalRoleOptions.map(([roleCode, roleName]) => (
                                    <option key={roleCode} value={roleCode}>{roleName}</option>
                                ))}
                            </select>
                        </div>
                        <BaseTable
                            rows={filteredInternalAccounts}
                            getRowKey={(account) => account.accountId}
                            isHaveStt={false}
                            paginationResetKey={`${accountSearch}:${accountStatusFilter}:${accountRoleFilter}`}
                            loading={accountsLoading}
                            emptyMessage={accountsError
                                ? "Không thể tải dữ liệu tài khoản."
                                : accounts.length
                                    ? "Không tìm thấy tài khoản phù hợp."
                                    : "Chưa có tài khoản trong hệ thống."}
                            tableClassName="min-w-[760px] w-full text-sm"
                            rowClassName={(account) => account.accountIsActive
                                ? "hover:bg-blue-50/40 transition-colors"
                                : "bg-gray-50 hover:bg-gray-100"}
                            columns={[
                                {
                                    key: "name",
                                    header: "Họ tên",
                                    cellClassName: "max-w-48 truncate px-4 py-3 text-xs font-semibold text-gray-900",
                                    render: (account) => {
                                        const fullName = formatProfileFullName(account.userProfile)
                                        return <span title={fullName || account.accountEmail}>{fullName || "Chưa có hồ sơ"}</span>
                                    },
                                },
                                {
                                    key: "email",
                                    header: "Email",
                                    cellClassName: "max-w-56 truncate px-4 py-3 text-xs text-gray-500",
                                    render: (account) => account.accountEmail,
                                },
                                {
                                    key: "roles",
                                    header: "Vai trò",
                                    render: (account) => <div className="flex flex-wrap gap-1">
                                        {account.roles.map((role) => (
                                            <span key={role.roleId}
                                                  className="rounded bg-slate-100 px-1.5 py-0.5 text-[11px] font-medium text-slate-600">
                                                {role.roleName || role.roleCode}
                                            </span>
                                        ))}
                                    </div>,
                                },
                                {
                                    key: "createdAt",
                                    header: "Ngày tạo",
                                    cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500",
                                    render: (account) => {
                                        const createdAt = new Date(account.accountCreatedAt)
                                        return Number.isNaN(createdAt.getTime())
                                            ? "—"
                                            : createdAt.toLocaleDateString("vi-VN")
                                    },
                                },
                                {
                                    key: "status",
                                    header: "Trạng thái",
                                    cellClassName: "px-4 py-3 text-xs font-semibold",
                                    render: (account) => <span className={account.accountIsActive
                                        ? "text-emerald-600"
                                        : "text-gray-400"}>
                                        {account.accountIsActive ? "Hoạt động" : "Vô hiệu"}
                                    </span>,
                                },
                            ]}
                        />
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
                                    ? <p className="py-6 text-center text-sm text-gray-400">Không thể tải dữ liệu mặt
                                        bằng.</p>
                                    : <DistributionList items={premisesByStatus}/>}
                        </div>
                        <div
                            className="bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading title="Tỷ lệ lấp đầy theo địa điểm"/>
                            {premisesLoading
                                ? <p className="py-6 text-center text-sm text-gray-400">Đang tải thống kê...</p>
                                : premisesError
                                    ? <p className="py-6 text-center text-sm text-gray-400">Không thể tải dữ liệu mặt
                                        bằng.</p>
                                    : premisesByLocation.length === 0
                                        ? <p className="py-6 text-center text-sm text-gray-400">Chưa có dữ liệu mặt
                                            bằng.</p>
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
                                {
                                    status: "available" as const,
                                    label: "Còn trống",
                                    count: premiseStatusCounts.available
                                },
                                {status: "rented" as const, label: "Đang thuê", count: premiseStatusCounts.rented},
                                {status: "reserved" as const, label: "Đã đặt cọc", count: premiseStatusCounts.reserved},
                                {
                                    status: "maintenance" as const,
                                    label: "Bảo trì",
                                    count: premiseStatusCounts.maintenance
                                },
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
                            sub={analyticsLoading ? "Đang tải…" : "Dữ liệu hiện có"}
                            color="bg-blue-500"
                        />
                        <KPICard
                            label="Đang hiệu lực"
                            value={String(
                                CONTRACTS.filter((c) => c.status === "Signed").length,
                            )}
                            sub="Đã ký kết"
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
                            color="bg-violet-500"
                        />
                    </div>

                    <div className="grid grid-cols-3 gap-6">
                        <div
                            className="col-span-2 bg-white rounded-2xl border border-gray-100 p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]">
                            <SectionHeading
                                title="Hợp đồng tạo theo tháng"
                                sub="Theo ngày tạo trong dữ liệu hiện có"
                            />
                            <div
                                className="grid grid-cols-4 bg-gray-50 px-4 py-2.5 rounded-lg text-xs font-semibold text-gray-400 uppercase tracking-wide">
                                <span>Tháng</span>
                                <span className="text-right">Tổng hợp đồng</span>
                                <span className="text-right">Đã ký</span>
                                <span className="text-right">Chờ xử lý</span>
                            </div>
                            <div className="divide-y divide-gray-100">
                                {monthlyContracts.map((row) => (
                                    <div
                                        key={row.month}
                                        className="grid grid-cols-4 px-4 py-3 hover:bg-gray-50/50 transition-colors"
                                    >
                    <span className="text-xs font-semibold text-gray-700">
                      {row.month}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-blue-600 text-right">
                      {row.total}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-amber-600 text-right">
                      {row.signed}
                    </span>
                                        <span className="text-xs font-mono font-semibold text-violet-600 text-right">
                      {row.pending}
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
                        <div className="mb-4 flex flex-wrap gap-1 rounded-lg bg-gray-100 p-1 w-fit">
                            {([
                                {status: "all" as const, label: "Tất cả", count: CONTRACTS.length},
                                ...contractStatuses.map((status) => ({
                                    status,
                                    label: recordStatuses[status],
                                    count: CONTRACTS.filter((contract) => contract.status === status).length,
                                })),
                            ]).map(({status, label, count}) => (
                                <button
                                    key={status}
                                    type="button"
                                    onClick={() => setContractFilter(status)}
                                    className={`rounded-md px-3 py-1.5 text-xs font-medium transition-all ${
                                        contractFilter === status
                                            ? "bg-white text-gray-900 shadow-sm"
                                            : "text-gray-500 hover:text-gray-700"
                                    }`}
                                >
                                    {label}<span className="ml-1.5 text-gray-400">{count}</span>
                                </button>
                            ))}
                        </div>
                        <BaseTable
                            rows={filteredContracts}
                            getRowKey={(contract) => contract.id}
                            isHaveStt={false}
                            paginationResetKey={contractFilter}
                            loading={analyticsLoading}
                            emptyMessage={analyticsError
                                ? "Không thể tải danh sách hợp đồng."
                                : "Chưa có hợp đồng phù hợp."}
                            tableClassName="min-w-[900px] w-full text-sm"
                            rowClassName={() => "hover:bg-blue-50/40 transition-colors"}
                            columns={[
                                {
                                    key: "id",
                                    header: "Mã HĐ",
                                    cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-500",
                                    render: (contract) => contract.id,
                                },
                                {
                                    key: "tenant",
                                    header: "Khách thuê",
                                    cellClassName: "max-w-48 truncate px-4 py-3 text-xs text-gray-700",
                                    render: (contract) => <div>
                                        <p className="truncate font-medium">{contract.tenant}</p>
                                        {contract.accountEmail && <p className="truncate text-gray-400">{contract.accountEmail}</p>}
                                    </div>,
                                },
                                {
                                    key: "premise",
                                    header: "Mặt bằng",
                                    cellClassName: "max-w-56 truncate px-4 py-3 text-xs font-semibold text-gray-700",
                                    render: (contract) => contract.premiseName,
                                },
                                {
                                    key: "deposit",
                                    header: "Tiền cọc",
                                    headerClassName: "px-4 py-3 text-right font-medium",
                                    cellClassName: "whitespace-nowrap px-4 py-3 text-right font-mono text-xs text-gray-600",
                                    render: (contract) => fmtM(contract.deposit),
                                },
                                {
                                    key: "rentalPrice",
                                    header: "Giá/tháng",
                                    headerClassName: "px-4 py-3 text-right font-medium",
                                    cellClassName: "whitespace-nowrap px-4 py-3 text-right font-mono text-xs font-semibold text-gray-800",
                                    render: (contract) => fmtM(contract.rentalPrice),
                                },
                                {
                                    key: "status",
                                    header: "Trạng thái",
                                    render: (contract) => <ContractStatusBadge status={contract.status}/>,
                                },
                            ]}
                        />
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
                            sub={`${VIOLATIONS.length ? Math.round((violationResolved / VIOLATIONS.length) * 100) : 0}% tỷ lệ xử lý`}
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
                                {monthlyViolations.map((row) => (
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
                                {monthlyViolations.map((row) => (
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
                        <div className="mb-4 flex flex-wrap gap-1 rounded-lg bg-gray-100 p-1 w-fit">
                            {([
                                {status: "all" as const, label: "Tất cả", count: VIOLATIONS.length},
                                {status: "unresolved" as const, label: "Chưa xử lý", count: VIOLATIONS.filter((violation) => !violation.resolved).length},
                                {status: "resolved" as const, label: "Đã xử lý", count: VIOLATIONS.filter((violation) => violation.resolved).length},
                            ]).map(({status, label, count}) => (
                                <button
                                    key={status}
                                    type="button"
                                    onClick={() => setViolationFilter(status)}
                                    className={`rounded-md px-3 py-1.5 text-xs font-medium transition-all ${
                                        violationFilter === status
                                            ? "bg-white text-gray-900 shadow-sm"
                                            : "text-gray-500 hover:text-gray-700"
                                    }`}
                                >
                                    {label}<span className="ml-1.5 text-gray-400">{count}</span>
                                </button>
                            ))}
                        </div>
                        <BaseTable
                            rows={filteredViolations}
                            getRowKey={(violation) => violation.id}
                            isHaveStt={false}
                            paginationResetKey={violationFilter}
                            loading={analyticsLoading}
                            emptyMessage={analyticsError
                                ? "Không thể tải danh sách vi phạm."
                                : "Chưa có vi phạm phù hợp."}
                            tableClassName="min-w-[760px] w-full text-sm"
                            rowClassName={(violation) => violation.resolved
                                ? "hover:bg-emerald-50/40 transition-colors"
                                : "hover:bg-orange-50/40 transition-colors"}
                            columns={[
                                {
                                    key: "id",
                                    header: "Mã VP",
                                    cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-500",
                                    render: (violation) => violation.id,
                                },
                                {
                                    key: "contractId",
                                    header: "HĐ",
                                    cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-500",
                                    render: (violation) => violation.contractId,
                                },
                                {
                                    key: "content",
                                    header: "Nội dung",
                                    cellClassName: "max-w-96 truncate px-4 py-3 text-xs text-gray-700",
                                    render: (violation) => violation.content,
                                },
                                {
                                    key: "penalty",
                                    header: "Tiền phạt",
                                    headerClassName: "px-4 py-3 text-right font-medium",
                                    cellClassName: "whitespace-nowrap px-4 py-3 text-right font-mono text-xs font-semibold text-red-600",
                                    render: (violation) => fmt(violation.penaltyAmount),
                                },
                                {
                                    key: "status",
                                    header: "Trạng thái",
                                    cellClassName: "px-4 py-3 text-xs font-semibold",
                                    render: (violation) => <span className={violation.resolved
                                        ? "text-emerald-600"
                                        : "text-orange-500"}>
                                        {violation.resolved ? "Đã xử lý" : "Chưa xử lý"}
                                    </span>,
                                },
                            ]}
                        />
                    </div>
                </div>
            )}
        </div>
    )
}
