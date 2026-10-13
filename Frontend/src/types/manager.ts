export type ManagerModule =
  | "dashboard"
  | "statistics"
  | "premises"
  | "contracts"
  | "invoices"
  | "regulations"
  | "violations"
  | "tickets"
  | "notifications"
  | "accounts"
  | "contacts"

export interface ManagerWorkspaceProps {
  accountId: string
  email: string
  roleCodes: string[]
  onLogout: () => void
}
