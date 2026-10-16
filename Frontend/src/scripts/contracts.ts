import type { ContractStatus as ApiContractStatus } from "../api/contracts"
import type { ContractStatus } from "../types/modules"

export const toUiContractStatus = (status: ApiContractStatus): ContractStatus => ({
  PendingApproval: "pending_approval",
  PendingSignature: "pending_signature",
  Signed: "signed",
  Expired: "expired",
  Terminated: "terminated",
  Cancelled: "canceled",
})[status]

export const toDateInput = (value: string) =>
  value ? new Date(value).toISOString().slice(0, 10) : ""
