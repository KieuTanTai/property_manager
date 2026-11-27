import {useEffect, useState} from "react"
import {contractApi} from "@/api/contracts"
import {premiseApi} from "@/api/premise"
import {ticketApi} from "@/api/ticketAndNotification"
import type {ContractModel, MonthlyInvoiceModel} from "@/types/contracts"
import type {PremiseModel} from "@/components/premises/types"
import type {TicketModel} from "@/api/ticketAndNotification"
import {fmt} from "@/scripts/format.ts"
import type {PremiseStatus} from "../premises/types"
import {ModulePageHeader} from "./shared"

type DashboardData = {
    premises: PremiseModel[]
    contracts: ContractModel[]
    invoices: MonthlyInvoiceModel[]
    tickets: TicketModel[]
}

function StatCard({label, value, sub, color}: {label: string; value: string | number; sub?: string; color: string}) {
    return <div className="flex flex-col gap-2 rounded-xl border border-gray-100 bg-white p-5">
        <div className="flex items-center gap-2">
            <div className={`h-8 w-1.5 rounded-full ${color}`}/>
            <span className="text-xs font-medium uppercase tracking-wider text-gray-400">{label}</span>
        </div>
        <div className="text-2xl font-bold text-gray-900" style={{fontFamily: "Open Sans, sans-serif"}}>{value}</div>
        {sub && <div className="text-xs text-gray-400">{sub}</div>}
    </div>
}

export function Dashboard() {
    const [data, setData] = useState<DashboardData>({premises: [], contracts: [], invoices: [], tickets: []})
    const [error, setError] = useState<string | null>(null)
    const [isLoading, setIsLoading] = useState(true)

    useEffect(() => {
        const controller = new AbortController()
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
            premiseApi.getPremise(controller.signal),
            loadPages((cursor, signal) => contractApi.contracts(cursor, signal)),
            loadPages((cursor, signal) => contractApi.invoices(cursor, signal)),
            ticketApi.tickets(controller.signal),
        ]).then(([premises, contracts, invoices, tickets]) => {
            if (!controller.signal.aborted) setData({premises, contracts, invoices, tickets})
        }).catch((cause: unknown) => {
            if (!controller.signal.aborted) setError(cause instanceof Error ? cause.message : "Không tải được tổng quan.")
        }).finally(() => {
            if (!controller.signal.aborted) setIsLoading(false)
        })
        return () => controller.abort()
    }, [])

    const rented = data.premises.filter((premise) => premise.premiseStatus === "Rented").length
    const available = data.premises.filter((premise) => premise.premiseStatus === "Available").length
    const activeContracts = data.contracts.filter((contract) => contract.contractStatus === "Signed").length
    const overdueInvoices = data.invoices.filter((invoice) => invoice.invoiceStatus === "Overdue").length
    const violations = data.contracts.flatMap((contract) => contract.contractViolations ?? [])
    const unresolvedViolations = violations.filter((violation) => !violation.violationIsResolved).length
    const openTickets = data.tickets.filter((ticket) => !ticket.ticketIsResolved).length
    const totalRevenue = data.invoices
        .filter((invoice) => invoice.invoiceStatus === "Paid")
        .reduce((sum, invoice) => sum + (invoice.invoiceTotalAmount ?? 0), 0)

    const premiseStatuses: PremiseStatus[] = ["available", "rented", "reserved", "maintenance"]
    const statusValues: Record<PremiseStatus, PremiseModel["premiseStatus"]> = {
        available: "Available", rented: "Rented", reserved: "Reserved", maintenance: "Maintenance",
    }
    const statusLabels: Record<PremiseStatus, string> = {
        available: "Còn trống", rented: "Đang thuê", reserved: "Đã đặt cọc", maintenance: "Bảo trì",
    }
    const statusColors: Record<PremiseStatus, string> = {
        available: "bg-emerald-500", rented: "bg-blue-500", reserved: "bg-amber-500", maintenance: "bg-red-400",
    }

    const recentActivity = [
        ...data.contracts.map((contract) => ({
            id: `contract-${contract.contractId}`, date: contract.contractCreatedAt,
            text: `Hợp đồng ${contract.contractId} · ${contract.contractStatus}`,
            color: "text-blue-600", icon: "edit_note",
        })),
        ...data.invoices.map((invoice) => ({
            id: `invoice-${invoice.invoiceId}`, date: invoice.invoiceCreatedAt,
            text: `Hóa đơn ${invoice.invoiceId} · ${invoice.invoiceStatus}`,
            color: invoice.invoiceStatus === "Overdue" ? "text-red-600" : "text-violet-600", icon: "credit_card",
        })),
        ...data.tickets.map((ticket) => ({
            id: `ticket-${ticket.ticketId}`, date: ticket.ticketCreatedAt,
            text: `Yêu cầu hỗ trợ: ${ticket.ticketContent}`,
            color: "text-amber-600", icon: "confirmation_number",
        })),
    ].sort((a, b) => Date.parse(b.date) - Date.parse(a.date)).slice(0, 5)

    return <div>
        <ModulePageHeader title="Tổng quan hệ thống" description="Số liệu được tải trực tiếp từ các API"/>
        {error && <p role="alert" className="mb-4 rounded-lg border border-red-100 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</p>}
        <div className="mb-6 grid grid-cols-2 gap-4 md:grid-cols-4">
            <StatCard label="Mặt bằng đang thuê" value={isLoading ? "—" : rented} sub={isLoading ? undefined : `${available} còn trống`} color="bg-blue-500"/>
            <StatCard label="Hợp đồng hiệu lực" value={isLoading ? "—" : activeContracts} sub="Đã ký kết" color="bg-emerald-500"/>
            <StatCard label="Doanh thu đã thu" value={isLoading ? "—" : fmt(totalRevenue)} sub="Từ hóa đơn đã thanh toán" color="bg-violet-500"/>
            <StatCard label="Hóa đơn quá hạn" value={isLoading ? "—" : overdueInvoices} sub={`${unresolvedViolations} vi phạm chưa xử lý`} color="bg-red-500"/>
        </div>

        <div className="mb-6 grid grid-cols-1 gap-6 md:grid-cols-2">
            <div className="rounded-xl border border-gray-100 bg-white p-5">
                <h2 className="mb-4 text-sm font-semibold text-gray-700" style={{fontFamily: "Open Sans, sans-serif"}}>Trạng thái mặt bằng</h2>
                <div className="space-y-3">
                    {premiseStatuses.map((status) => {
                        const count = data.premises.filter((premise) => premise.premiseStatus === statusValues[status]).length
                        const percentage = data.premises.length ? Math.round(count / data.premises.length * 100) : 0
                        return <div key={status}>
                            <div className="mb-1 flex items-center justify-between">
                                <span className="text-xs text-gray-500">{statusLabels[status]}</span>
                                <span className="text-xs font-medium text-gray-700">{isLoading ? "—" : `${count} / ${data.premises.length}`}</span>
                            </div>
                            <div className="h-1.5 overflow-hidden rounded-full bg-gray-100">
                                <div className={`h-full rounded-full transition-all ${statusColors[status]}`} style={{width: `${percentage}%`}}/>
                            </div>
                        </div>
                    })}
                </div>
            </div>
            <div className="rounded-xl border border-gray-100 bg-white p-5">
                <h2 className="mb-4 text-sm font-semibold text-gray-700" style={{fontFamily: "Open Sans, sans-serif"}}>Hoạt động gần đây</h2>
                {recentActivity.length ? <div className="space-y-3">
                    {recentActivity.map((activity) => <div key={activity.id} className="flex items-center gap-3 border-b border-gray-50 py-1.5 last:border-0">
                        <span className="material-symbols-outlined text-base" aria-hidden="true">{activity.icon}</span>
                        <p className={`min-w-0 flex-1 truncate text-xs font-medium ${activity.color}`} title={activity.text}>{activity.text}</p>
                        <time className="whitespace-nowrap text-xs text-gray-300" dateTime={activity.date}>{new Date(activity.date).toLocaleDateString("vi-VN")}</time>
                    </div>)}
                </div> : <p className="text-sm text-gray-400">{isLoading ? "Đang tải dữ liệu…" : "Chưa có hoạt động."}</p>}
            </div>
        </div>

        {!isLoading && (overdueInvoices > 0 || unresolvedViolations > 0 || openTickets > 0) && <div className="rounded-xl border border-gray-100 bg-white p-5">
            <h2 className="mb-3 text-sm font-semibold text-gray-700" style={{fontFamily: "Open Sans, sans-serif"}}>Cần xử lý</h2>
            <div className="flex flex-wrap gap-3">
                {overdueInvoices > 0 && <span className="rounded-lg border border-red-100 bg-red-50 px-3 py-2 text-xs text-red-700">{overdueInvoices} hóa đơn quá hạn thanh toán</span>}
                {unresolvedViolations > 0 && <span className="rounded-lg border border-orange-100 bg-orange-50 px-3 py-2 text-xs text-orange-700">{unresolvedViolations} vi phạm chưa xử lý</span>}
                {openTickets > 0 && <span className="rounded-lg border border-amber-100 bg-amber-50 px-3 py-2 text-xs text-amber-700">{openTickets} yêu cầu chờ phản hồi</span>}
            </div>
        </div>}
    </div>
}
