import {Contract} from "@/types/modules.ts";

export type ContractStatus = "PendingApproval" | "PendingSignature" | "Canceled" | "Expired" | "Signed" | "Terminated"
export type InvoiceStatus = "Unpaid" | "Paid" | "Overdue"

export const statuses: ContractStatus[] = ["PendingApproval", "PendingSignature", "Canceled", "Expired", "Signed", "Terminated"]
export const recordStatuses: Record<ContractStatus, string> = {
    PendingApproval: "Chờ duyệt",
    PendingSignature: "Chờ ký",
    Canceled: "Đã hủy",
    Expired: "Hết hạn",
    Signed: "Đã ký",
    Terminated: "Đã chấm dứt"
}

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
    receiptPaymentMethod: "vnpay" | "bank" | "cash"
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

export type ConsultationRequest = {
    name: string
    phone: string
    senderEmail: string
    premiseName: string | null
    message: string
}

export type CreateContractRequest = {
    accountId: string
    deposit: number
    rentalPrice: number
    premiseReturnDate: string
    regulationIds: string[]
}

export type UpdateContractRequest = CreateContractRequest & {
    status: ContractStatus
    terminationDate: string | null
}

export type ContractForm = {
    accountId: string
    deposit: string
    rentalPrice: string
    premiseReturnDate: string
    status: ContractStatus
    terminationDate: string
    regulationIds: string[]
}

export interface ContractsModuleProps {
    accountId?: string
    readOnly?: boolean
}
