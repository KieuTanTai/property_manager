import { useState } from "react"
import type { ManagerModule as Module, ManagerWorkspaceProps as Props } from "../../types/manager"
import { AccountsModule } from "./AccountsModule"
import { ContractsModule } from "./ContractsModule"
import { ContactsModule } from "./ContactsModule"
import { Dashboard } from "./Dashboard"
import { InvoicesModule } from "./InvoicesModule"
import { PremisesModule } from "./PremisesModule"
import { RegulationsModule } from "./RegulationsModule"
import { StatisticsModule } from "./StatisticsModule"
import { TicketsModule } from "./TicketsModule"
import { ViolationsModule } from "./ViolationsModule"

const FULL_NAV: { id: Module; label: string; icon: string }[] = [
  { id: "dashboard", label: "Tổng quan", icon: "dashboard" },
  { id: "statistics", label: "Thống kê", icon: "monitoring" },
  { id: "accounts", label: "Tài khoản", icon: "group" },
  { id: "contracts", label: "Hợp đồng", icon: "description" },
  { id: "invoices", label: "Hóa đơn", icon: "receipt_long" },
  { id: "regulations", label: "Quy định", icon: "gavel" },
  { id: "violations", label: "Vi phạm", icon: "warning" },
  { id: "tickets", label: "Yêu cầu", icon: "confirmation_number" },
  { id: "premises", label: "Mặt bằng", icon: "storefront" },
  { id: "contacts", label: "Thông tin liên hệ", icon: "contact_phone" },
]

const LIMITED_NAV: { id: Module; label: string; icon: string }[] = [
  { id: "notifications", label: "Thông báo", icon: "notifications" },
  { id: "contracts", label: "Hợp đồng của tôi", icon: "description" },
  { id: "violations", label: "Vi phạm của tôi", icon: "warning" },
]

export function ManagerWorkspace({ accountId, email, roleCodes, onLogout }: Props) {
  const [active, setActive] = useState<Module>("dashboard")
  const hasFullAccess = roleCodes.some((role) => ["ADMIN", "MANAGER"].includes(role.toUpperCase()))
  const nav = hasFullAccess ? FULL_NAV : LIMITED_NAV
  const [restrictedActive, setRestrictedActive] = useState<Module>("notifications")
  const currentActive = hasFullAccess ? active : restrictedActive
  const accountInitial = email.charAt(0).toUpperCase()

  return (
    <div className="flex min-h-screen" style={{ fontFamily: "Inter, sans-serif" }}>
      <aside className="flex w-56 shrink-0 flex-col" style={{ background: "#0f1629", minHeight: "100vh" }}>
        <div className="border-b px-5 py-5" style={{ borderColor: "rgba(255,255,255,0.06)" }}>
          <div className="flex items-center gap-2.5">
            <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-blue-500 text-xs font-bold text-white" style={{ fontFamily: "DM Sans" }}>M</div>
            <div>
              <div className="text-sm font-semibold leading-tight text-white" style={{ fontFamily: "DM Sans" }}>ManagerSpace</div>
              <div className="text-xs" style={{ color: "rgba(255,255,255,0.35)" }}>Quản lý mặt bằng</div>
            </div>
          </div>
        </div>

        <nav className="flex-1 overflow-y-auto px-3 py-4">
          <div className="space-y-0.5">
            {nav.map((item) => (
              <button
                key={item.id}
                onClick={() => hasFullAccess ? setActive(item.id) : setRestrictedActive(item.id)}
                className={`flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm transition-all ${currentActive === item.id ? "bg-blue-600 text-white" : "hover:bg-white/5"}`}
                style={{ color: currentActive === item.id ? "#fff" : "rgba(255,255,255,0.55)" }}
              >
                <span className="material-symbols-outlined w-4 text-center text-[18px] leading-none" aria-hidden="true">{item.icon}</span>
                <span style={{ fontFamily: "DM Sans" }}>{item.label}</span>
              </button>
            ))}
          </div>
        </nav>

        <div className="border-t px-3 py-4" style={{ borderColor: "rgba(255,255,255,0.06)" }}>
          <div className="flex items-center gap-3 px-2">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-400 to-violet-500 text-xs font-bold text-white">{accountInitial}</div>
            <div className="min-w-0 flex-1">
              <div className="truncate text-xs font-medium text-white">{email}</div>
          <div className="truncate text-xs" style={{ color: "rgba(255,255,255,0.35)" }}>{hasFullAccess ? "Quản lý" : roleCodes.join(", ") || "Tài khoản"}</div>
            </div>
            <button onClick={onLogout} className="rounded-lg px-2 py-1.5 text-xs text-slate-400 transition hover:bg-white/10 hover:text-white" title="Đăng xuất">Thoát</button>
          </div>
        </div>
      </aside>

      <main className="flex min-w-0 flex-1 flex-col">
        <header className="flex shrink-0 items-center justify-between border-b border-gray-100 bg-white px-8 py-3">
          <div className="text-xs text-gray-400">{nav.find((item) => item.id === currentActive)?.label}</div>
          <div className="flex items-center gap-4">
            <div className="text-xs text-gray-400" style={{ fontFamily: "JetBrains Mono, monospace" }}>{new Intl.DateTimeFormat("vi-VN").format(new Date())}</div>
            <div className="h-2 w-2 rounded-full bg-emerald-400" title="Hệ thống hoạt động bình thường" />
          </div>
        </header>
        <div className="flex-1 overflow-y-auto p-8">
          {currentActive === "dashboard" && hasFullAccess && <Dashboard />}
          {currentActive === "statistics" && hasFullAccess && <StatisticsModule />}
          {currentActive === "premises" && hasFullAccess && <PremisesModule />}
          {currentActive === "contracts" && <ContractsModule accountId={accountId} readOnly={!hasFullAccess} />}
          {currentActive === "invoices" && hasFullAccess && <InvoicesModule />}
          {currentActive === "regulations" && hasFullAccess && <RegulationsModule />}
          {currentActive === "violations" && <ViolationsModule accountId={accountId} readOnly={!hasFullAccess} />}
          {currentActive === "tickets" && hasFullAccess && <TicketsModule />}
          {currentActive === "notifications" && !hasFullAccess && <TicketsModule accountId={accountId} customerMode />}
          {currentActive === "accounts" && hasFullAccess && <AccountsModule />}
          {currentActive === "contacts" && hasFullAccess && <ContactsModule />}
        </div>
      </main>
    </div>
  )
}
