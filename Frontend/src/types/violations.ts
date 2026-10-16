import type { Violation } from "./modules"

export type ViolationFilter = "all" | "unresolved" | "resolved"
export type ViolationForm = Pick<Violation, "contractId" | "tenant" | "content" | "penaltyAmount" | "date" | "dueDate" | "resolved">

export interface ViolationsModuleProps {
  accountId?: string
  readOnly?: boolean
}
