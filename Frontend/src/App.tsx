import { useState } from "react"
import { AccountsModule } from "./components/modules/AccountsModule"
import { ContractsModule } from "./components/modules/ContractsModule"
import { Dashboard } from "./components/modules/Dashboard"
import { InvoicesModule } from "./components/modules/InvoicesModule"
import { PremisesModule } from "./components/modules/PremisesModule"
import { RegulationsModule } from "./components/modules/RegulationsModule"
import { StatisticsModule } from "./components/modules/StatisticsModule"
import { TicketsModule } from "./components/modules/TicketsModule"
import { ViolationsModule } from "./components/modules/ViolationsModule"

// ─── Nav items ───────────────────────────────────────────────────────────────

type Module = "dashboard" | "statistics" | "premises" | "contracts" | "invoices" | "regulations" | "violations" | "tickets" | "accounts"

const NAV: { id: Module; label: string; icon: string }[] = [
  { id: "dashboard", label: "Tổng quan", icon: "⊞" },
  { id: "statistics", label: "Thống kê", icon: "▦" },
  { id: "accounts", label: "Tài khoản", icon: "◉" },
  { id: "contracts", label: "Hợp đồng", icon: "⚿" },
  { id: "invoices", label: "Hóa đơn", icon: "◈" },
  { id: "regulations", label: "Quy định", icon: "§" },
  { id: "violations", label: "Vi phạm", icon: "⚠" },
  { id: "tickets", label: "Yêu cầu", icon: "✉" },
  { id: "premises", label: "Mặt bằng", icon: "⬡" },
]

// ─── Root App ────────────────────────────────────────────────────────────────

export default function App() {
  const [active, setActive] = useState<Module>("dashboard")

  return (
    <div
      className="flex min-h-screen"
      style={{ fontFamily: "Inter, sans-serif" }}
    >
      {/* Sidebar */}
      <aside
        className="w-56 shrink-0 flex flex-col"
        style={{ background: "#0f1629", minHeight: "100vh" }}
      >
        {/* Logo */}
        <div
          className="px-5 py-5 border-b"
          style={{ borderColor: "rgba(255,255,255,0.06)" }}
        >
          <div className="flex items-center gap-2.5">
            <div
              className="w-7 h-7 rounded-lg bg-blue-500 flex items-center justify-center text-white text-xs font-bold"
              style={{ fontFamily: "DM Sans" }}
            >
              M
            </div>
            <div>
              <div
                className="text-white text-sm font-semibold leading-tight"
                style={{ fontFamily: "DM Sans" }}
              >
                ManagerSpace
              </div>
              <div
                className="text-xs"
                style={{ color: "rgba(255,255,255,0.35)" }}
              >
                Quản lý mặt bằng
              </div>
            </div>
          </div>
        </div>

        {/* Nav */}
        <nav className="flex-1 px-3 py-4 overflow-y-auto">
          <div className="space-y-0.5">
            {NAV.map((item) => (
              <button
                key={item.id}
                onClick={() => setActive(item.id)}
                className={`w-full flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-all ${
                  active === item.id
                    ? "bg-blue-600 text-white"
                    : "hover:bg-white/5"
                }`}
                style={{
                  color: active === item.id ? "#fff" : "rgba(255,255,255,0.55)",
                }}
              >
                <span className="text-base w-4 text-center leading-none">
                  {item.icon}
                </span>
                <span style={{ fontFamily: "DM Sans" }}>{item.label}</span>
              </button>
            ))}
          </div>
        </nav>

        {/* User */}
        <div
          className="px-3 py-4 border-t"
          style={{ borderColor: "rgba(255,255,255,0.06)" }}
        >
          <div className="flex items-center gap-3 px-2">
            <div className="w-7 h-7 rounded-full bg-gradient-to-br from-blue-400 to-violet-500 flex items-center justify-center text-white text-xs font-bold shrink-0">
              A
            </div>
            <div className="min-w-0">
              <div className="text-xs font-medium text-white truncate">
                Admin Hệ Thống
              </div>
              <div
                className="text-xs truncate"
                style={{ color: "rgba(255,255,255,0.35)" }}
              >
                SUPER_ADMIN
              </div>
            </div>
          </div>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 min-w-0 flex flex-col">
        {/* Top bar */}
        <header className="bg-white border-b border-gray-100 px-8 py-3 flex items-center justify-between shrink-0">
          <div className="text-xs text-gray-400">
            {NAV.find((n) => n.id === active)?.label}
          </div>
          <div className="flex items-center gap-4">
            <div
              className="text-xs text-gray-400"
              style={{ fontFamily: "JetBrains Mono, monospace" }}
            >
              29/09/2026
            </div>
            <div
              className="w-2 h-2 rounded-full bg-emerald-400"
              title="Hệ thống hoạt động bình thường"
            />
          </div>
        </header>

        {/* Page content */}
        <div className="flex-1 p-8 overflow-y-auto">
          {active === "dashboard" && <Dashboard />}
          {active === "statistics" && <StatisticsModule />}
          {active === "premises" && <PremisesModule />}
          {active === "contracts" && <ContractsModule />}
          {active === "invoices" && <InvoicesModule />}
          {active === "regulations" && <RegulationsModule />}
          {active === "violations" && <ViolationsModule />}
          {active === "tickets" && <TicketsModule />}
          {active === "accounts" && <AccountsModule />}
        </div>
      </main>
    </div>
  )
}
