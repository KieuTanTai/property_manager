import type { CursorPage } from "./identity"

export type ContractStatus = "PendingApproval" | "PendingSignature" | "Cancelled" | "Expired" | "Signed" | "Terminated"
export type InvoiceStatus = "Unpaid" | "Paid" | "Overdue"

export type ContractViolationModel = {
  violationId: string
  contractId: string
  violationContent: string
  violationPenaltyAmount: number | null
  violationDate: string
  violationDueDate: string
  violationIsResolved: boolean
}

export type ContractModel = {
  contractId: string
  contractAccountId: string
  contractDeposit: number
  contractRentalPrice: number
  contractPremiseReturnDate: string
  contractStatus: ContractStatus
  contractTerminationDate: string | null
  contractCreatedAt: string
  contractUpdatedAt: string
  contractViolations: ContractViolationModel[]
  contractInvoices: MonthlyInvoiceModel[]
  contractRegulations: RegulationModel[]
}

export type InvoiceDetailModel = {
  invoiceDetailId: number
  invoiceDetailInvoiceId: string
  invoiceDetailPremiseId: string
  invoiceDetailRentalPrice: number
  invoiceDetailElectricityFee: number
  invoiceDetailWaterFee: number
  invoiceDetailGarbageFee: number
  invoiceDetailTotalAmount: number
}

export type ReceiptModel = {
  receiptId: string
  receiptInvoiceId: string
  receiptCreatedByAccountId: string
  receiptPaymentMethod: "VnPay" | "Bank" | "Cash"
  receiptAmount: number
  receiptPaymentDate: string
  receiptTransactionReference: string | null
  receiptGatewayTransactionNumber: string | null
  receiptTransactionInfo: string | null
  receiptBankName: string | null
  receiptCreatedAt: string
  receiptUpdatedAt: string
}

export type MonthlyInvoiceModel = {
  invoiceId: string
  invoiceContractId: string
  invoicePaymentDate: string
  invoiceDueDate: string
  invoiceTotalAmount: number | null
  invoiceStatus: InvoiceStatus
  invoiceCreatedAt: string
  invoiceUpdatedAt: string
  invoiceDetails: InvoiceDetailModel[]
  invoiceReceipt: ReceiptModel | null
}

export type RegulationModel = {
  regulationId: string
  regulationName: string
  regulationDescription: string | null
  regulationFineAmount: number | null
  regulationIsActive: boolean
  regulationCreatedAt: string
  regulationUpdatedAt: string
}

const base = (import.meta.env.VITE_CONTRACT_API_BASE_URL || "/api").replace(/\/$/, "")

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${base}${path}`, {
      ...init,
      headers: { ...(init?.body ? { "Content-Type": "application/json" } : {}), ...init?.headers },
    })
  } catch {
    throw new Error(`Không thể kết nối Contract API tại ${base}; kiểm tra Backend, HTTPS và CORS.`)
  }
  if (!response.ok) {
    const detail = await response.text()
    if (response.status === 404 && /no (contracts|regulations) found/i.test(detail)) {
      return (path.startsWith("/Contract") ? { items: [], nextCursor: null, isHasMore: false } : []) as T
    }
    throw new Error(detail || `Contract API trả về HTTP ${response.status}.`)
  }
  return response.json() as Promise<T>
}

export const contractApi = {
  contracts(cursor: string | null) {
    const query = new URLSearchParams({ pageSize: "50", isGetViolations: "true", isGetInvoices: "true" })
    if (cursor) query.set("cursor", cursor)
    return request<CursorPage<ContractModel>>(`/Contract?${query}`)
  },
  regulations(cursor: string | null = null) {
    const query = new URLSearchParams({ pageSize: "100" })
    if (cursor) query.set("cursor", cursor)
    return request<CursorPage<RegulationModel>>(`/Regulation?${query}`)
  },
  createContract(body: { accountId: string; deposit: number; rentalPrice: number; premiseReturnDate: string; regulationIds: string[] }) {
    return request<ContractModel>("/Contract", { method: "POST", body: JSON.stringify(body) })
  },
  updateContract(id: string, body: { accountId: string; deposit: number; rentalPrice: number; premiseReturnDate: string; status: ContractStatus; terminationDate: string | null; regulationIds: string[] }) {
    return request<ContractModel>(`/Contract/${id}`, { method: "PUT", body: JSON.stringify(body) })
  },
}
