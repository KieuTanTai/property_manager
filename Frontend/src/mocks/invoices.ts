import type { InvoiceDetail, MonthlyInvoice, Receipt } from "../types/invoices"

export const MOCK_MONTHLY_INVOICES: MonthlyInvoice[] = [
  {
    invoiceId: "0195d8a1-1a00-7111-8111-100000000001",
    invoiceContractId: "0195d7f0-1000-7111-8111-100000000001",
    invoicePaymentDate: "2026-01-01",
    invoiceDueDate: "2026-01-31",
    invoiceTotalAmount: 15700000,
    invoiceStatus: "unpaid",
    invoiceCreatedAt: "2026-01-01T08:00:00",
    invoiceUpdatedAt: "2026-01-18T10:15:00",
  },
  {
    invoiceId: "0195d8a1-1a01-7222-8222-200000000002",
    invoiceContractId: "0195d7f0-1001-7222-8222-200000000002",
    invoicePaymentDate: "2026-02-01",
    invoiceDueDate: "2026-03-03",
    invoiceTotalAmount: 13100000,
    invoiceStatus: "unpaid",
    invoiceCreatedAt: "2026-02-01T08:30:00",
    invoiceUpdatedAt: "2026-02-01T08:30:00",
  },
  {
    invoiceId: "0195d8a1-1a02-7333-8333-300000000003",
    invoiceContractId: "0195d7f0-1002-7333-8333-300000000003",
    invoicePaymentDate: "2026-01-01",
    invoiceDueDate: "2026-01-31",
    invoiceTotalAmount: 10200000,
    invoiceStatus: "overdue",
    invoiceCreatedAt: "2026-01-01T09:00:00",
    invoiceUpdatedAt: "2026-02-01T00:00:00",
  },
]

export const MOCK_INVOICE_DETAILS: InvoiceDetail[] = [
  {
    invoiceDetailId: 1,
    invoiceDetailInvoiceId: MOCK_MONTHLY_INVOICES[0].invoiceId,
    invoiceDetailPremiseId: "0195c8f6-cd50-7111-8111-100000000001",
    invoiceDetailRentalPrice: 7500000,
    invoiceDetailElectricityFee: 150000,
    invoiceDetailWaterFee: 100000,
    invoiceDetailGarbageFee: 50000,
    invoiceDetailTotalAmount: 7800000,
  },
  {
    invoiceDetailId: 2,
    invoiceDetailInvoiceId: MOCK_MONTHLY_INVOICES[0].invoiceId,
    invoiceDetailPremiseId: "0195c8f6-cd51-7222-8222-200000000002",
    invoiceDetailRentalPrice: 7500000,
    invoiceDetailElectricityFee: 200000,
    invoiceDetailWaterFee: 100000,
    invoiceDetailGarbageFee: 100000,
    invoiceDetailTotalAmount: 7900000,
  },
  {
    invoiceDetailId: 3,
    invoiceDetailInvoiceId: MOCK_MONTHLY_INVOICES[1].invoiceId,
    invoiceDetailPremiseId: "0195c8f6-cd52-7333-8333-300000000003",
    invoiceDetailRentalPrice: 12000000,
    invoiceDetailElectricityFee: 600000,
    invoiceDetailWaterFee: 300000,
    invoiceDetailGarbageFee: 200000,
    invoiceDetailTotalAmount: 13100000,
  },
  {
    invoiceDetailId: 4,
    invoiceDetailInvoiceId: MOCK_MONTHLY_INVOICES[2].invoiceId,
    invoiceDetailPremiseId: "0195c8f6-cd53-7444-8444-400000000004",
    invoiceDetailRentalPrice: 9000000,
    invoiceDetailElectricityFee: 700000,
    invoiceDetailWaterFee: 300000,
    invoiceDetailGarbageFee: 200000,
    invoiceDetailTotalAmount: 10200000,
  },
]

export const MOCK_CONTRACT_BILLING: Record<string, {
  rentalPrice: number
  premiseIds: string[]
}> = {
  "0195d7f0-1000-7111-8111-100000000001": {
    rentalPrice: 7500000,
    premiseIds: [
      "0195c8f6-cd50-7111-8111-100000000001",
      "0195c8f6-cd51-7222-8222-200000000002",
    ],
  },
  "0195d7f0-1001-7222-8222-200000000002": {
    rentalPrice: 12000000,
    premiseIds: ["0195c8f6-cd52-7333-8333-300000000003"],
  },
  "0195d7f0-1002-7333-8333-300000000003": {
    rentalPrice: 9000000,
    premiseIds: ["0195c8f6-cd53-7444-8444-400000000004"],
  },
}

export const MOCK_RECEIPTS: Receipt[] = [
  {
    receiptId: "0195d8c3-3c00-7111-8111-100000000001",
    receiptInvoiceId: MOCK_MONTHLY_INVOICES[0].invoiceId,
    receiptCreatedByAccountId: "0195d6e0-0000-7111-8111-100000000001",
    receiptPaymentMethod: "bank_transfer",
    receiptAmount: 8200000,
    receiptPaymentDate: "2026-01-18",
    receiptTransactionReference: "MS-012026-001",
    receiptGatewayTransactionNumber: "VCB260118001",
    receiptTransactionInfo: "Thanh toán hóa đơn tháng 01/2026",
    receiptBankName: "Vietcombank",
    receiptCreatedAt: "2026-01-18T10:15:00",
    receiptUpdatedAt: "2026-01-18T10:15:00",
  },
]
