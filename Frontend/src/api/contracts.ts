import type {CursorPage} from "@/types/identity"
import type {ContactInfoModel, ContactInfoRequest, ConsultationRequest, ContractModel,
    ContractViolationModel, CreateContractRequest, RegulationModel, UpdateContractRequest} from "@/types/contracts"
import {APP_SETTINGS} from "@/config/appSettings"

const base = (import.meta.env.VITE_CONTRACT_API_BASE_URL || "/api").replace(/\/$/, "")
const baseReqPageItemsLoaded = String(APP_SETTINGS.pagination.defaultPageLoaded * APP_SETTINGS.pagination.defaultPageSize)
const maxPageLoaded = String(APP_SETTINGS.pagination.maxPageLoaded * APP_SETTINGS.pagination.defaultPageSize);

async function request<T>(path: string, init?: RequestInit): Promise<T> {
    let response: Response
    try {
        response = await fetch(`${base}${path}`, {
            ...init,
            headers: {...(init?.body ? {"Content-Type": "application/json"} : {}), ...init?.headers},
        })
    } catch {
        throw new Error(`Không thể kết nối Contract API tại ${base}; kiểm tra Backend, HTTPS và CORS.`)
    }
    if (!response.ok) {
        const detail = await response.text()
        if (response.status === 404 && /no (contracts|regulations|violations) found/i.test(detail)) {
            if (path.startsWith("/Contract") || path.startsWith("/Violation") || path.startsWith("/Regulation")) {
                return {items: [], nextCursor: null, isHasMore: false} as T
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
        const query = new URLSearchParams({pageSize: baseReqPageItemsLoaded, isGetViolations: "true", isGetInvoices: "true"})
        if (cursor) query.set("cursor", cursor)
        return request<CursorPage<ContractModel>>(`/Contract?${query}`)
    },
    regulations(cursor: string | null = null) {
        const query = new URLSearchParams({pageSize: maxPageLoaded})
        if (cursor) query.set("cursor", cursor)
        return request<CursorPage<RegulationModel>>(`/Regulation?${query}`)
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
        return request<ContactInfoModel[]>("/ContactInfo")
    },
    sendConsultationRequest(body: ConsultationRequest) {
        return request<{ recipientEmail: string }>("/ContactInfo/consultation", {
            method: "POST",
            body: JSON.stringify(body),
        })
    },
    createContactInfo(body: ContactInfoRequest) {
        return request<ContactInfoModel>("/ContactInfo", {method: "POST", body: JSON.stringify(body)})
    },
    updateContactInfo(id: string, body: ContactInfoRequest) {
        return request<ContactInfoModel>(`/ContactInfo/${id}`, {method: "PUT", body: JSON.stringify(body)})
    },
    deleteContactInfo(id: string) {
        return request<void>(`/ContactInfo/${id}`, {method: "DELETE"})
    },
    createContract(body: CreateContractRequest) {
        return request<ContractModel>("/Contract", {method: "POST", body: JSON.stringify(body)})
    },
    updateContract(id: string, body: UpdateContractRequest) {
        return request<ContractModel>(`/Contract/${id}`, {method: "PUT", body: JSON.stringify(body)})
    },
}
