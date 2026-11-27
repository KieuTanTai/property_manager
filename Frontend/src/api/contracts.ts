import type {CursorPage} from "@/types/identity"
import type {
    ConsultationRequest,
    ContactInfoModel,
    ContactInfoRequest,
    ContractModel,
    ContractViolationModel,
    CreateContractRequest,
    MonthlyInvoiceModel,
    ReceiptModel,
    RegulationModel,
    UpdateContractRequest
} from "@/types/contracts"
import {APP_SETTINGS} from "@/config/appSettings"
import {createApiHeaders} from "@/api/authHeaders"

const base = (import.meta.env.VITE_API_BASE_URL || "/api").replace(/\/$/, "")
const baseReqPageItemsLoaded = String(APP_SETTINGS.pagination.defaultPageLoaded * APP_SETTINGS.pagination.defaultPageSize)
const maxPageLoaded = String(APP_SETTINGS.pagination.maxPageLoaded * APP_SETTINGS.pagination.defaultPageSize);

async function request<T>(path: string, init?: RequestInit, includeAccessToken = true): Promise<T> {
    let response: Response
    try {
        response = await fetch(`${base}${path}`, {
            ...init,
            headers: createApiHeaders(
                { ...(init?.body ? {"Content-Type": "application/json"} : {}), ...init?.headers },
                includeAccessToken,
            ),
        })
    } catch {
        throw new Error(`Không thể kết nối Gateway API tại ${base}; kiểm tra Gateway và HTTPS.`)
    }
    if (!response.ok) {
        const detail = await response.text()
        if (response.status === 404 && /no (contracts|regulations|violations|invoices|receipts) found/i.test(detail)) {
            if (path.startsWith("/Contract") || path.startsWith("/Violation") || path.startsWith("/Regulation") || path.startsWith("/Invoice")) {
                return {items: [], nextCursor: null, isHasMore: false} as T
            }
            return [] as T
        }
        throw new Error(detail || `Gateway API trả về HTTP ${response.status}.`)
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
        const query = new URLSearchParams({
            pageSize: baseReqPageItemsLoaded,
            isGetViolations: "true",
            isGetInvoices: "true"
        })
        if (cursor) query.set("cursor", cursor)
        return request<CursorPage<ContractModel>>(`/Contract?${query}`)
    },
    regulations(cursor: string | null = null) {
        const query = new URLSearchParams({pageSize: maxPageLoaded})
        if (cursor) query.set("cursor", cursor)
        return request<CursorPage<RegulationModel>>(`/Regulation?${query}`)
    },
    createRegulation(body: Omit<RegulationModel, "regulationId" | "regulationCreatedAt" | "regulationUpdatedAt">) {
        return request<RegulationModel>("/Regulation", {method: "POST", body: JSON.stringify(body)})
    },
    updateRegulation(id: string, body: Omit<RegulationModel, "regulationId" | "regulationCreatedAt" | "regulationUpdatedAt">) {
        return request<RegulationModel>(`/Regulation/${encodeURIComponent(id)}`, {
            method: "PUT",
            body: JSON.stringify(body)
        })
    },
    deleteRegulation(id: string) {
        return request<void>(`/Regulation/${encodeURIComponent(id)}`, {method: "DELETE"})
    },
    violations(cursor: string | null = null, pageSize = APP_SETTINGS.pagination.defaultPageSize) {
        const query = new URLSearchParams({pageSize: String(pageSize)})
        if (cursor) query.set("cursor", cursor)
        return request<CursorPage<ContractViolationModel>>(`/Violation?${query}`)
    },
    createViolation(body: {
        contractId: string;
        violationContent: string;
        violationPenaltyAmount: number;
        violationDate: string;
        violationDueDate: string
    }) {
        return request<ContractViolationModel>("/Violation", {method: "POST", body: JSON.stringify(body)})
    },
    updateViolation(id: string, body: {
        contractId: string;
        violationContent: string;
        violationPenaltyAmount: number;
        violationDate: string;
        violationDueDate: string;
        violationIsResolved: boolean
    }) {
        return request<ContractViolationModel>(`/Violation/${id}`, {method: "PUT", body: JSON.stringify(body)})
    },
    deleteViolation(id: string) {
        return request<void>(`/Violation/${id}`, {method: "DELETE"})
    },
    contactInfos() {
        return request<ContactInfoModel[]>("/contact")
    },
    sendConsultationRequest(body: ConsultationRequest) {
        return request<{ recipientEmail: string }>("/contact/consultation", {
            method: "POST",
            body: JSON.stringify(body),
        }, false)
    },
    createContactInfo(body: ContactInfoRequest) {
        return request<ContactInfoModel>("/contact", {method: "POST", body: JSON.stringify(body)})
    },
    updateContactInfo(id: string, body: ContactInfoRequest) {
        return request<ContactInfoModel>(`/contact/${id}`, {method: "PUT", body: JSON.stringify(body)})
    },
    deleteContactInfo(id: string) {
        return request<void>(`/contact/${id}`, {method: "DELETE"})
    },
    createContract(body: CreateContractRequest) {
        return request<ContractModel>("/Contract", {method: "POST", body: JSON.stringify(body)})
    },
    updateContract(id: string, body: UpdateContractRequest) {
        return request<ContractModel>(`/Contract/${id}`, {method: "PUT", body: JSON.stringify(body)})
    },
    invoices(cursor: string | null = null) {
        const query = new URLSearchParams({pageSize: maxPageLoaded, isGetDetails: "true", isGetReceipt: "true"})
        if (cursor) query.set("cursor", cursor)
        return request<CursorPage<MonthlyInvoiceModel>>(`/Invoice?${query}`)
    },
    createInvoice(body: {
        invoiceId: string
        contractId: string
        paymentDate: string
        dueDate: string
        totalAmount: number
        status: MonthlyInvoiceModel["invoiceStatus"]
        details: Array<{
            premiseId: string
            rentalPrice: number
            electricityFee: number
            waterFee: number
            garbageFee: number
        }>
    }) {
        return request<MonthlyInvoiceModel>("/Invoice", {method: "POST", body: JSON.stringify(body)})
    },
    updateInvoice(invoiceId: string, body: {
        contractId: string
        paymentDate: string
        dueDate: string
        totalAmount: number
        status: MonthlyInvoiceModel["invoiceStatus"]
        details?: Array<{
            detailId: number
            premiseId: string
            rentalPrice: number
            electricityFee: number
            waterFee: number
            garbageFee: number
        }>
    }) {
        return request<MonthlyInvoiceModel>(`/Invoice/${encodeURIComponent(invoiceId)}`, {
            method: "PUT",
            body: JSON.stringify(body),
        })
    },
    receipts() {
        return request<ReceiptModel[]>("/Receipt")
    },
    createReceipt(body: {
        invoiceId: string
        createdByAccountId: string
        paymentMethod: "VnPay" | "Bank" | "Cash"
        amount: number
        paymentDate: string
        transactionReference: string | null
        gatewayTransactionNumber: string | null
        transactionInfo: string | null
        bankName: string | null
    }) {
        return request<ReceiptModel>("/Receipt", {method: "POST", body: JSON.stringify(body)})
    },
    updateReceipt(receiptId: string, invoiceId: string, body: {
        paymentDate: string
        amount: number
        transactionInfo: string | null
    }) {
        return request<ReceiptModel>(
            `/Receipt/${encodeURIComponent(receiptId)}/invoice/${encodeURIComponent(invoiceId)}`,
            {method: "PUT", body: JSON.stringify(body)},
        )
    },
}
