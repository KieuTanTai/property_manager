import type { InvoiceStatus } from "./modules"

export type InvoiceTab = "invoices" | "receipts"
export type ReceiptPaymentMethod = "cash" | "bank_transfer" | "card" | "e_wallet"

export interface MonthlyInvoice {
  invoiceId: string
  invoiceContractId: string
  invoicePaymentDate: string
  invoiceDueDate: string
  invoiceTotalAmount: number | null
  invoiceStatus: InvoiceStatus
  invoiceCreatedAt: string
  invoiceUpdatedAt: string
}

export interface InvoiceDetail {
  invoiceDetailId: number
  invoiceDetailInvoiceId: string
  invoiceDetailPremiseId: string
  invoiceDetailRentalPrice: number
  invoiceDetailElectricityFee: number
  invoiceDetailWaterFee: number
  invoiceDetailGarbageFee: number
  invoiceDetailTotalAmount: number
}

export interface Receipt {
  receiptId: string
  receiptInvoiceId: string
  receiptCreatedByAccountId: string
  receiptPaymentMethod: ReceiptPaymentMethod
  receiptAmount: number
  receiptPaymentDate: string
  receiptTransactionReference: string | null
  receiptGatewayTransactionNumber: string | null
  receiptTransactionInfo: string | null
  receiptBankName: string | null
  receiptCreatedAt: string
  receiptUpdatedAt: string
}

