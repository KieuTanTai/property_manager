export type { PremiseStatus, Premise, PremiseMedia, Location, BusinessType, WhitelistProduct } from "../components/premises/types"

export type ContractStatus = "pending_approval" | "pending_signature" | "canceled" | "expired" | "signed" | "terminated"
export type InvoiceStatus = "unpaid" | "paid" | "overdue"
export type TicketType = "review" | "complaint" | "violation" | "feedback"

export interface Contract {
  id: string
  accountEmail: string
  tenant: string
  premiseName: string
  deposit: number
  rentalPrice: number
  returnDate: string
  status: ContractStatus
  createdAt: string
}

export interface Invoice {
  id: string
  contractId: string
  tenant: string
  premiseName: string
  totalAmount: number
  paymentDate: string
  dueDate: string
  status: InvoiceStatus
}

export interface Violation {
  id: string
  contractId: string
  tenant: string
  content: string
  penaltyAmount: number
  date: string
  dueDate: string
  resolved: boolean
}

export interface Ticket {
  id: string
  accountEmail: string
  content: string
  type: TicketType
  resolved: boolean
  createdAt: string
}

export interface Account {
  id: string
  email: string
  role: string
  active: boolean
  createdAt: string
  name: string
}
