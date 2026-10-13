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

export type ContactInfoModel = {
  contactInfoId: string
  contactInfoEmail: string
  contactInfoPhone: string
  contactInfoAddress: string
  contactSocialLink: string | null
  contactInfoCreatedAt: string
  contactInfoUpdatedAt: string
}

export type ContactInfoRequest = Pick<ContactInfoModel,
  "contactInfoEmail" | "contactInfoPhone" | "contactInfoAddress" | "contactSocialLink">

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
    if (response.status === 404 && /no (contracts|regulations|violations) found/i.test(detail)) {
      if (path.startsWith("/Contract") || path.startsWith("/Violation")) {
        return { items: [], nextCursor: null, isHasMore: false } as T
      }
      return [] as T
    }
    throw new Error(detail || `Contract API trả về HTTP ${response.status}.`)
  }
  if (response.status === 204) return undefined as T
  const contentType = response.headers.get("content-type") ?? ""
  if (!contentType.toLowerCase().includes("application/json")) {
    const detail = (await response.text()).slice(0, 200)
    throw new Error(`Contract API trả về nội dung không phải JSON (HTTP ${response.status}). Kiểm tra route/proxy ContactInfo. ${detail}`)
  }
  try {
    return await response.json() as T
  } catch {
    throw new Error("Contract API trả về JSON không hợp lệ.")
  }
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
  violations(cursor: string | null = null, pageSize = 50) {
    const query = new URLSearchParams({ pageSize: String(pageSize) })
    if (cursor) query.set("cursor", cursor)
    return request<CursorPage<ContractViolationModel>>(`/Violation?${query}`)
  },
  createViolation(body: { contractId: string; violationContent: string; violationPenaltyAmount: number; violationDate: string; violationDueDate: string }) {
    return request<ContractViolationModel>("/Violation", { method: "POST", body: JSON.stringify(body) })
  },
  updateViolation(id: string, body: { contractId: string; violationContent: string; violationPenaltyAmount: number; violationDate: string; violationDueDate: string; violationIsResolved: boolean }) {
    return request<ContractViolationModel>(`/Violation/${id}`, { method: "PUT", body: JSON.stringify(body) })
  },
  deleteViolation(id: string) {
    return request<void>(`/Violation/${id}`, { method: "DELETE" })
  },
  contactInfos() {
    return request<ContactInfoModel[]>("/ContactInfo")
  },
  createContactInfo(body: ContactInfoRequest) {
    return request<ContactInfoModel>("/ContactInfo", { method: "POST", body: JSON.stringify(body) })
  },
  updateContactInfo(id: string, body: ContactInfoRequest) {
    return request<ContactInfoModel>(`/ContactInfo/${id}`, { method: "PUT", body: JSON.stringify(body) })
  },
  deleteContactInfo(id: string) {
    return request<void>(`/ContactInfo/${id}`, { method: "DELETE" })
  },
  createContract(body: { accountId: string; deposit: number; rentalPrice: number; premiseReturnDate: string; regulationIds: string[] }) {
    return request<ContractModel>("/Contract", { method: "POST", body: JSON.stringify(body) })
  },
  updateContract(id: string, body: { accountId: string; deposit: number; rentalPrice: number; premiseReturnDate: string; status: ContractStatus; terminationDate: string | null; regulationIds: string[] }) {
    return request<ContractModel>(`/Contract/${id}`, { method: "PUT", body: JSON.stringify(body) })
  },
}
