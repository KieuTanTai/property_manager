import { useState } from "react"
import {
  FormField,
  InvoiceStatusBadge,
  Modal,
  inputCls,
  selectCls,
} from "./shared"
import type { InvoiceStatus } from "../../types/modules"
import type { InvoiceTab, ReceiptPaymentMethod, MonthlyInvoice, InvoiceDetail, Receipt } from "../../types/invoices"
import { MOCK_MONTHLY_INVOICES, MOCK_INVOICE_DETAILS, MOCK_CONTRACT_BILLING, MOCK_RECEIPTS } from "../../mocks/invoices"
import { createUuidV7 } from "../../scripts/ids"
import { fmt, today } from "../../scripts/format"

const paymentMethodLabels: Record<ReceiptPaymentMethod, string> = {
  cash: "Tiền mặt",
  bank_transfer: "Chuyển khoản",
  card: "Thẻ",
  e_wallet: "Ví điện tử",
}

export function InvoicesModule() {
  const [invoices, setInvoices] = useState<MonthlyInvoice[]>(MOCK_MONTHLY_INVOICES)
  const [details, setDetails] = useState<InvoiceDetail[]>(MOCK_INVOICE_DETAILS)
  const [receipts, setReceipts] = useState<Receipt[]>(MOCK_RECEIPTS)
  const [activeTab, setActiveTab] = useState<InvoiceTab>("invoices")
  const [filter, setFilter] = useState<InvoiceStatus | "all">("all")
  const [selected, setSelected] = useState<MonthlyInvoice | null>(null)
  const [selectedReceipt, setSelectedReceipt] = useState<Receipt | null>(null)
  const [invoiceModal, setInvoiceModal] = useState<"add" | "detail" | null>(
    null,
  )
  const [receiptModal, setReceiptModal] = useState(false)
  const [isEditing, setIsEditing] = useState(false)
  const [receiptIsEditing, setReceiptIsEditing] = useState(false)
  const [invoiceForm, setInvoiceForm] = useState({
    contractId: "",
    paymentDate: today(),
    dueDate: "",
    totalAmount: "",
    status: "unpaid" as InvoiceStatus,
  })
  const [receiptForm, setReceiptForm] = useState({
    invoiceId: "",
    createdByAccountId: "0195d6e0-0000-7111-8111-100000000001",
    paymentMethod: "cash" as ReceiptPaymentMethod,
    amount: "",
    paymentDate: today(),
    transactionReference: "",
    gatewayTransactionNumber: "",
    transactionInfo: "",
    bankName: "",
  })
  const [detailForm, setDetailForm] = useState({
    premiseId: "",
    electricityFee: "",
    waterFee: "",
    garbageFee: "",
  })
  const [createDetailFees, setCreateDetailFees] = useState<Record<string, {
    electricityFee: string
    waterFee: string
    garbageFee: string
  }>>({})
  const [receiptEditForm, setReceiptEditForm] = useState({
    paymentDate: "",
    amount: "",
    transactionInfo: "",
  })

  const filtered =
    filter === "all"
      ? invoices
      : invoices.filter((invoice) => invoice.invoiceStatus === filter)
  const totalUnpaid = invoices
    .filter((invoice) => invoice.invoiceStatus !== "paid")
    .reduce((sum, invoice) => sum + (invoice.invoiceTotalAmount ?? 0), 0)

  const openAddInvoice = () => {
    const dueDate = new Date()
    dueDate.setDate(dueDate.getDate() + 30)
    const contractId = Object.keys(MOCK_CONTRACT_BILLING)[0] ?? ""
    setInvoiceForm({
      contractId,
      paymentDate: today(),
      dueDate: dueDate.toISOString().slice(0, 10),
      totalAmount: "",
      status: "unpaid",
    })
    setCreateDetailFees(
      Object.fromEntries(
        (MOCK_CONTRACT_BILLING[contractId]?.premiseIds ?? []).map((premiseId) => [
          premiseId,
          { electricityFee: "", waterFee: "", garbageFee: "" },
        ]),
      ),
    )
    setInvoiceModal("add")
  }

  const selectInvoiceContract = (contractId: string) => {
    setInvoiceForm({ ...invoiceForm, contractId })
    setCreateDetailFees(
      Object.fromEntries(
        (MOCK_CONTRACT_BILLING[contractId]?.premiseIds ?? []).map((premiseId) => [
          premiseId,
          { electricityFee: "", waterFee: "", garbageFee: "" },
        ]),
      ),
    )
  }

  const openInvoice = (invoice: MonthlyInvoice) => {
    setSelected(invoice)
    setIsEditing(false)
    const availablePremise =
      MOCK_CONTRACT_BILLING[invoice.invoiceContractId]?.premiseIds.find(
        (premiseId) =>
          !details.some(
            (detail) =>
              detail.invoiceDetailInvoiceId === invoice.invoiceId &&
              detail.invoiceDetailPremiseId === premiseId,
          ),
      ) ?? ""
    setDetailForm({
      premiseId: availablePremise,
      electricityFee: "",
      waterFee: "",
      garbageFee: "",
    })
    setInvoiceModal("detail")
  }

  const addInvoice = () => {
    const now = new Date().toISOString()
    const invoiceId = createUuidV7()
    const billing = MOCK_CONTRACT_BILLING[invoiceForm.contractId]
    if (!billing) return
    const firstDetailId =
      Math.max(0, ...details.map((detail) => detail.invoiceDetailId)) + 1
    const newDetails: InvoiceDetail[] = billing.premiseIds.map(
      (premiseId, index) => {
        const fees = createDetailFees[premiseId]
        const electricityFee = Number(fees.electricityFee)
        const waterFee = Number(fees.waterFee)
        const garbageFee = Number(fees.garbageFee)
        return {
          invoiceDetailId: firstDetailId + index,
          invoiceDetailInvoiceId: invoiceId,
          invoiceDetailPremiseId: premiseId,
          invoiceDetailRentalPrice: billing.rentalPrice,
          invoiceDetailElectricityFee: electricityFee,
          invoiceDetailWaterFee: waterFee,
          invoiceDetailGarbageFee: garbageFee,
          invoiceDetailTotalAmount:
            billing.rentalPrice + electricityFee + waterFee + garbageFee,
        }
      },
    )
    const totalAmount = newDetails.reduce(
      (sum, detail) => sum + detail.invoiceDetailTotalAmount,
      0,
    )
    const invoice: MonthlyInvoice = {
      invoiceId,
      invoiceContractId: invoiceForm.contractId,
      invoicePaymentDate: invoiceForm.paymentDate,
      invoiceDueDate: invoiceForm.dueDate,
      invoiceTotalAmount: totalAmount,
      invoiceStatus: "unpaid",
      invoiceCreatedAt: now,
      invoiceUpdatedAt: now,
    }
    setInvoices([...invoices, invoice])
    setDetails([...details, ...newDetails])
    setSelected(invoice)
    setInvoiceModal("detail")
  }

  const startEditing = () => {
    if (!selected) return
    setInvoiceForm({
      contractId: selected.invoiceContractId,
      paymentDate: selected.invoicePaymentDate,
      dueDate: selected.invoiceDueDate,
      totalAmount:
        selected.invoiceTotalAmount === null
          ? ""
          : String(selected.invoiceTotalAmount),
      status: selected.invoiceStatus,
    })
    setIsEditing(true)
  }

  const updateInvoice = () => {
    if (!selected) return
    const updated: MonthlyInvoice = {
      ...selected,
      invoiceContractId: invoiceForm.contractId,
      invoicePaymentDate: invoiceForm.paymentDate,
      invoiceDueDate: invoiceForm.dueDate,
      invoiceTotalAmount: selected.invoiceTotalAmount,
      invoiceStatus: invoiceForm.status,
      invoiceUpdatedAt: new Date().toISOString(),
    }
    setInvoices(
      invoices.map((invoice) =>
        invoice.invoiceId === selected.invoiceId ? updated : invoice,
      ),
    )
    setSelected(updated)
    setIsEditing(false)
  }

  const openAddReceipt = (invoice = selected) => {
    setReceiptForm({
      invoiceId: invoice?.invoiceId ?? invoices[0]?.invoiceId ?? "",
      createdByAccountId: "0195d6e0-0000-7111-8111-100000000001",
      paymentMethod: "cash",
      amount:
        invoice?.invoiceTotalAmount === null ||
        invoice?.invoiceTotalAmount === undefined
          ? ""
          : String(invoice.invoiceTotalAmount),
      paymentDate: today(),
      transactionReference: "",
      gatewayTransactionNumber: "",
      transactionInfo: "",
      bankName: "",
    })
    setReceiptModal(true)
  }

  const addReceipt = () => {
    const now = new Date().toISOString()
    const receipt: Receipt = {
      receiptId: createUuidV7(),
      receiptInvoiceId: receiptForm.invoiceId,
      receiptCreatedByAccountId: receiptForm.createdByAccountId,
      receiptPaymentMethod: receiptForm.paymentMethod,
      receiptAmount: Number(receiptForm.amount),
      receiptPaymentDate: receiptForm.paymentDate,
      receiptTransactionReference:
        receiptForm.transactionReference.trim() || null,
      receiptGatewayTransactionNumber:
        receiptForm.gatewayTransactionNumber.trim() || null,
      receiptTransactionInfo: receiptForm.transactionInfo.trim() || null,
      receiptBankName: receiptForm.bankName.trim() || null,
      receiptCreatedAt: now,
      receiptUpdatedAt: now,
    }
    const nextReceipts = [...receipts, receipt]
    const relatedInvoice = invoices.find(
      (invoice) => invoice.invoiceId === receipt.receiptInvoiceId,
    )
    const received = nextReceipts
      .filter((item) => item.receiptInvoiceId === receipt.receiptInvoiceId)
      .reduce((sum, item) => sum + item.receiptAmount, 0)
    const nextInvoices = invoices.map((invoice) =>
      invoice.invoiceId === receipt.receiptInvoiceId &&
      relatedInvoice?.invoiceTotalAmount !== null &&
      relatedInvoice?.invoiceTotalAmount !== undefined &&
      received >= relatedInvoice.invoiceTotalAmount
        ? {
            ...invoice,
            invoiceStatus: "paid" as InvoiceStatus,
            invoiceUpdatedAt: now,
          }
        : invoice,
    )
    setReceipts(nextReceipts)
    setInvoices(nextInvoices)
    setReceiptModal(false)
  }

  const addInvoiceDetail = () => {
    if (!selected) return
    const billing = MOCK_CONTRACT_BILLING[selected.invoiceContractId]
    const electricityFee = Number(detailForm.electricityFee)
    const waterFee = Number(detailForm.waterFee)
    const garbageFee = Number(detailForm.garbageFee)
    if (
      !billing ||
      !detailForm.premiseId ||
      [
        detailForm.electricityFee,
        detailForm.waterFee,
        detailForm.garbageFee,
      ].some((value) => value === "") ||
      [electricityFee, waterFee, garbageFee].some((value) => value < 0)
    )
      return

    const detail: InvoiceDetail = {
      invoiceDetailId:
        Math.max(0, ...details.map((item) => item.invoiceDetailId)) + 1,
      invoiceDetailInvoiceId: selected.invoiceId,
      invoiceDetailPremiseId: detailForm.premiseId,
      invoiceDetailRentalPrice: billing.rentalPrice,
      invoiceDetailElectricityFee: electricityFee,
      invoiceDetailWaterFee: waterFee,
      invoiceDetailGarbageFee: garbageFee,
      invoiceDetailTotalAmount:
        billing.rentalPrice + electricityFee + waterFee + garbageFee,
    }
    const now = new Date().toISOString()
    const nextDetails = [...details, detail]
    const total = nextDetails
      .filter((item) => item.invoiceDetailInvoiceId === selected.invoiceId)
      .reduce((sum, item) => sum + item.invoiceDetailTotalAmount, 0)
    const updated = {
      ...selected,
      invoiceTotalAmount: total,
      invoiceUpdatedAt: now,
    }
    const nextPremise =
      billing.premiseIds.find(
        (premiseId) =>
          !nextDetails.some(
            (item) =>
              item.invoiceDetailInvoiceId === selected.invoiceId &&
              item.invoiceDetailPremiseId === premiseId,
          ),
      ) ?? ""

    setDetails(nextDetails)
    setInvoices(
      invoices.map((invoice) =>
        invoice.invoiceId === selected.invoiceId ? updated : invoice,
      ),
    )
    setSelected(updated)
    setDetailForm({
      premiseId: nextPremise,
      electricityFee: "",
      waterFee: "",
      garbageFee: "",
    })
  }

  const openReceipt = (receipt: Receipt) => {
    setSelectedReceipt(receipt)
    setReceiptIsEditing(false)
    setReceiptEditForm({
      paymentDate: receipt.receiptPaymentDate.slice(0, 10),
      amount: String(receipt.receiptAmount),
      transactionInfo: receipt.receiptTransactionInfo ?? "",
    })
  }

  const startEditingReceipt = () => {
    if (!selectedReceipt) return
    setReceiptEditForm({
      paymentDate: selectedReceipt.receiptPaymentDate.slice(0, 10),
      amount: String(selectedReceipt.receiptAmount),
      transactionInfo: selectedReceipt.receiptTransactionInfo ?? "",
    })
    setReceiptIsEditing(true)
  }

  const updateReceipt = () => {
    if (!selectedReceipt || Number(receiptEditForm.amount) <= 0) return
    const now = new Date().toISOString()
    const updated: Receipt = {
      ...selectedReceipt,
      receiptPaymentDate: receiptEditForm.paymentDate,
      receiptAmount: Number(receiptEditForm.amount),
      receiptTransactionInfo: receiptEditForm.transactionInfo.trim() || null,
      receiptUpdatedAt: now,
    }
    const nextReceipts = receipts.map((receipt) =>
      receipt.receiptId === updated.receiptId ? updated : receipt,
    )
    const received = nextReceipts
      .filter(
        (receipt) => receipt.receiptInvoiceId === updated.receiptInvoiceId,
      )
      .reduce((sum, receipt) => sum + receipt.receiptAmount, 0)
    const nextInvoices = invoices.map((invoice) => {
      if (
        invoice.invoiceId !== updated.receiptInvoiceId ||
        invoice.invoiceTotalAmount === null
      )
        return invoice
      return {
        ...invoice,
        invoiceStatus:
          received >= invoice.invoiceTotalAmount
            ? "paid" as InvoiceStatus
            : invoice.invoiceStatus === "overdue"
              ? "overdue" as InvoiceStatus
              : "unpaid" as InvoiceStatus,
        invoiceUpdatedAt: now,
      }
    })

    setReceipts(nextReceipts)
    setInvoices(nextInvoices)
    setSelectedReceipt(updated)
    setReceiptIsEditing(false)
  }

  const invoiceDetails = selected
    ? details.filter(
        (detail) => detail.invoiceDetailInvoiceId === selected.invoiceId,
      )
    : []
  const selectedBilling = selected
    ? MOCK_CONTRACT_BILLING[selected.invoiceContractId]
    : undefined
  const availablePremiseIds =
    selectedBilling?.premiseIds.filter(
      (premiseId) =>
        !invoiceDetails.some(
          (detail) => detail.invoiceDetailPremiseId === premiseId,
        ),
    ) ?? []
  const detailTotal =
    (selectedBilling?.rentalPrice ?? 0) +
    (Number(detailForm.electricityFee) || 0) +
    (Number(detailForm.waterFee) || 0) +
    (Number(detailForm.garbageFee) || 0)
  const createInvoiceDetailsValid =
    Object.keys(createDetailFees).length > 0 &&
    Object.values(createDetailFees).every((fees) =>
      [fees.electricityFee, fees.waterFee, fees.garbageFee].every(
        (value) => value !== "" && Number(value) >= 0,
      ),
    )
  const createInvoiceTotal = (
    MOCK_CONTRACT_BILLING[invoiceForm.contractId]?.premiseIds ?? []
  ).reduce((sum, premiseId) => {
    const fees = createDetailFees[premiseId]
    return (
      sum +
      (MOCK_CONTRACT_BILLING[invoiceForm.contractId]?.rentalPrice ?? 0) +
      (Number(fees?.electricityFee) || 0) +
      (Number(fees?.waterFee) || 0) +
      (Number(fees?.garbageFee) || 0)
    )
  }, 0)
  const receivedFor = (invoiceId: string) =>
    receipts
      .filter((receipt) => receipt.receiptInvoiceId === invoiceId)
      .reduce((sum, receipt) => sum + receipt.receiptAmount, 0)

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1
            className="text-xl font-bold text-gray-900"
            style={{ fontFamily: "DM Sans" }}
          >
            Hóa đơn hàng tháng
          </h1>
          <p className="text-sm text-gray-400 mt-0.5">
            Tổng chưa thu:{" "}
            <span className="font-mono text-red-500">{fmt(totalUnpaid)}</span>
          </p>
        </div>
        <button
          onClick={
            activeTab === "invoices"
              ? openAddInvoice
              : () => openAddReceipt(null)
          }
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors flex items-center gap-2"
        >
          <span>+</span>{" "}
          {activeTab === "invoices" ? "Tạo hóa đơn" : "Tạo phiếu thu"}
        </button>
      </div>

      <div className="border-b border-gray-200 mb-5">
        <div className="flex gap-6">
          {([
            { id: "invoices", label: "Hóa đơn", count: invoices.length },
            { id: "receipts", label: "Phiếu thu", count: receipts.length },
          ] as const).map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`pb-3 text-sm font-semibold border-b-2 transition-colors ${
                activeTab === tab.id
                  ? "border-blue-600 text-blue-600"
                  : "border-transparent text-gray-400 hover:text-gray-700"
              }`}
            >
              {tab.label}{" "}
              <span className="ml-1 text-xs font-medium">{tab.count}</span>
            </button>
          ))}
        </div>
      </div>

      {activeTab === "invoices" ? (
        <>
          <div className="flex gap-1 bg-gray-100 rounded-lg p-1 mb-5 w-fit">
            {(["all", "unpaid", "paid", "overdue"] as const).map((status) => {
              const labels = {
                all: "Tất cả",
                unpaid: "Chưa thanh toán",
                paid: "Đã thanh toán",
                overdue: "Quá hạn",
              }
              const count =
                status === "all"
                  ? invoices.length
                  : invoices.filter(
                      (invoice) => invoice.invoiceStatus === status,
                    ).length
              return (
                <button
                  key={status}
                  onClick={() => setFilter(status)}
                  className={`px-3 py-1.5 rounded-md text-xs font-medium transition-all ${
                    filter === status
                      ? "bg-white text-gray-900 shadow-sm"
                      : "text-gray-500 hover:text-gray-700"
                  }`}
                >
                  {labels[status]}{" "}
                  <span className="ml-1 text-gray-400">{count}</span>
                </button>
              )
            })}
          </div>
          <div className="bg-white rounded-xl border border-gray-100 overflow-x-auto">
            <table className="w-full min-w-5xl text-sm">
              <thead>
                <tr className="border-b border-gray-100">
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                    Invoice ID
                  </th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                    Contract ID
                  </th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                    Ngày lập
                  </th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                    Ngày đến hạn
                  </th>
                  <th className="text-right px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                    Tổng tiền
                  </th>
                  <th className="text-center px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                    Chi tiết
                  </th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                    Trạng thái
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {filtered.map((invoice) => (
                  <tr
                    key={invoice.invoiceId}
                    onClick={() => openInvoice(invoice)}
                    onKeyDown={(event) => {
                      if (event.key === "Enter" || event.key === " ")
                        openInvoice(invoice)
                    }}
                    tabIndex={0}
                    className="hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none transition-colors cursor-pointer"
                    title="Bấm để xem các chi tiết hóa đơn"
                  >
                    <td className="px-4 py-3 text-xs font-mono text-gray-500 max-w-44 truncate">
                      {invoice.invoiceId}
                    </td>
                    <td className="px-4 py-3 text-xs font-mono text-gray-500 max-w-44 truncate">
                      {invoice.invoiceContractId}
                    </td>
                    <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                      {invoice.invoicePaymentDate}
                    </td>
                    <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                      {invoice.invoiceDueDate}
                    </td>
                    <td className="px-4 py-3 text-right text-xs font-mono font-semibold text-gray-800 whitespace-nowrap">
                      {invoice.invoiceTotalAmount === null
                        ? "—"
                        : fmt(invoice.invoiceTotalAmount)}
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2 py-1 rounded">
                        {
                          details.filter(
                            (detail) =>
                              detail.invoiceDetailInvoiceId ===
                              invoice.invoiceId,
                          ).length
                        }
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <InvoiceStatusBadge status={invoice.invoiceStatus} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      ) : (
        <div className="bg-white rounded-xl border border-gray-100 overflow-x-auto">
          <table className="w-full min-w-5xl text-sm">
            <thead>
              <tr className="border-b border-gray-100">
                <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                  Receipt ID
                </th>
                <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                  Invoice ID
                </th>
                <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                  Phương thức
                </th>
                <th className="text-right px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                  Số tiền
                </th>
                <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                  Ngày thanh toán
                </th>
                <th className="text-left px-4 py-3 text-xs font-medium text-gray-400 uppercase tracking-wide">
                  Ngân hàng
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {receipts.map((receipt) => (
                <tr
                  key={receipt.receiptId}
                  onClick={() => openReceipt(receipt)}
                  className="hover:bg-blue-50/50 transition-colors cursor-pointer"
                >
                  <td className="px-4 py-3 text-xs font-mono text-gray-500 max-w-48 truncate">
                    {receipt.receiptId}
                  </td>
                  <td className="px-4 py-3 text-xs font-mono text-gray-500 max-w-48 truncate">
                    {receipt.receiptInvoiceId}
                  </td>
                  <td className="px-4 py-3 text-xs text-gray-600">
                    {paymentMethodLabels[receipt.receiptPaymentMethod]}
                  </td>
                  <td className="px-4 py-3 text-right text-xs font-mono font-semibold text-emerald-600">
                    {fmt(receipt.receiptAmount)}
                  </td>
                  <td className="px-4 py-3 text-xs text-gray-500">
                    {receipt.receiptPaymentDate}
                  </td>
                  <td className="px-4 py-3 text-xs text-gray-500">
                    {receipt.receiptBankName || "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {invoiceModal === "add" && (
        <Modal
          title="Tạo hóa đơn hàng tháng"
          onClose={() => setInvoiceModal(null)}
          wide
        >
          <div className="grid grid-cols-3 gap-x-4">
            <FormField label="Hợp đồng">
              <select
                className={selectCls}
                value={invoiceForm.contractId}
                onChange={(event) => selectInvoiceContract(event.target.value)}
              >
                {Object.entries(MOCK_CONTRACT_BILLING).map(
                  ([contractId, billing]) => (
                    <option key={contractId} value={contractId}>
                      {contractId} · {billing.premiseIds.length} mặt bằng
                    </option>
                  ),
                )}
              </select>
            </FormField>
            <FormField label="Ngày lập">
              <input
                className={inputCls}
                type="date"
                value={invoiceForm.paymentDate}
                onChange={(event) =>
                  setInvoiceForm({
                    ...invoiceForm,
                    paymentDate: event.target.value,
                  })
                }
              />
            </FormField>
            <FormField label="Ngày đến hạn">
              <input
                className={inputCls}
                type="date"
                value={invoiceForm.dueDate}
                onChange={(event) =>
                  setInvoiceForm({
                    ...invoiceForm,
                    dueDate: event.target.value,
                  })
                }
              />
            </FormField>
          </div>

          <div className="rounded-xl border border-gray-100 overflow-hidden mb-5">
            <div className="grid grid-cols-5 gap-3 bg-gray-50 px-4 py-2.5 text-xs font-semibold text-gray-400">
              <span>Premise ID</span>
              <span className="text-right">Tiền thuê</span>
              <span className="text-right">Tiền điện</span>
              <span className="text-right">Tiền nước</span>
              <span className="text-right">Phí rác</span>
            </div>
            {(MOCK_CONTRACT_BILLING[invoiceForm.contractId]?.premiseIds ?? []).map(
              (premiseId) => {
                const fees = createDetailFees[premiseId] ?? {
                  electricityFee: "",
                  waterFee: "",
                  garbageFee: "",
                }
                return (
                  <div
                    key={premiseId}
                    className="grid grid-cols-5 gap-3 items-center px-4 py-3 border-t border-gray-50"
                  >
                    <span
                      className="text-xs font-mono text-gray-500 truncate"
                      title={premiseId}
                    >
                      {premiseId}
                    </span>
                    <div className="relative">
                      <input
                        className={`${inputCls} pr-12 bg-gray-100 text-gray-500 cursor-not-allowed text-right`}
                        value={
                          MOCK_CONTRACT_BILLING[invoiceForm.contractId]
                            ?.rentalPrice ?? 0
                        }
                        readOnly
                      />
                      <span className="absolute inset-y-0 right-2 flex items-center text-xs text-gray-400 pointer-events-none">
                        VND
                      </span>
                    </div>
                    <input
                      className={`${inputCls} text-right`}
                      type="number"
                      min="0"
                      required
                      value={fees.electricityFee}
                      onChange={(event) =>
                        setCreateDetailFees({
                          ...createDetailFees,
                          [premiseId]: {
                            ...fees,
                            electricityFee: event.target.value,
                          },
                        })
                      }
                      placeholder="Bắt buộc"
                    />
                    <input
                      className={`${inputCls} text-right`}
                      type="number"
                      min="0"
                      required
                      value={fees.waterFee}
                      onChange={(event) =>
                        setCreateDetailFees({
                          ...createDetailFees,
                          [premiseId]: {
                            ...fees,
                            waterFee: event.target.value,
                          },
                        })
                      }
                      placeholder="Bắt buộc"
                    />
                    <input
                      className={`${inputCls} text-right`}
                      type="number"
                      min="0"
                      required
                      value={fees.garbageFee}
                      onChange={(event) =>
                        setCreateDetailFees({
                          ...createDetailFees,
                          [premiseId]: {
                            ...fees,
                            garbageFee: event.target.value,
                          },
                        })
                      }
                      placeholder="Bắt buộc"
                    />
                  </div>
                )
              },
            )}
          </div>

          <div className="flex items-center justify-between rounded-lg border border-blue-100 bg-blue-50 px-4 py-3 mb-4">
            <span className="text-xs text-blue-600">
              Hóa đơn và toàn bộ chi tiết sẽ được tạo cùng lúc.
            </span>
            <span className="text-sm font-mono font-bold text-blue-700">
              Tổng: {fmt(createInvoiceTotal)}
            </span>
          </div>
          <div className="flex justify-end gap-2">
            <button
              onClick={() => setInvoiceModal(null)}
              className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Hủy
            </button>
            <button
              onClick={addInvoice}
              disabled={
                !invoiceForm.contractId ||
                !invoiceForm.paymentDate ||
                !invoiceForm.dueDate ||
                !createInvoiceDetailsValid
              }
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-200 disabled:text-gray-400"
            >
              Tạo hóa đơn và chi tiết
            </button>
          </div>
        </Modal>
      )}

      {invoiceModal === "detail" && selected && (
        <Modal
          title="Chi tiết hóa đơn hàng tháng"
          onClose={() => setInvoiceModal(null)}
          wide
        >
          {isEditing ? (
            <div className="mb-6 border-b border-gray-100 pb-5">
              <div className="grid grid-cols-4 gap-x-4">
                <FormField label="Contract ID">
                  <input
                    className={inputCls}
                    value={invoiceForm.contractId}
                    onChange={(event) =>
                      setInvoiceForm({
                        ...invoiceForm,
                        contractId: event.target.value,
                      })
                    }
                  />
                </FormField>
                <FormField label="Ngày lập">
                  <input
                    className={inputCls}
                    type="date"
                    value={invoiceForm.paymentDate}
                    onChange={(event) =>
                      setInvoiceForm({
                        ...invoiceForm,
                        paymentDate: event.target.value,
                      })
                    }
                  />
                </FormField>
                <FormField label="Ngày đến hạn">
                  <input
                    className={inputCls}
                    type="date"
                    value={invoiceForm.dueDate}
                    onChange={(event) =>
                      setInvoiceForm({
                        ...invoiceForm,
                        dueDate: event.target.value,
                      })
                    }
                  />
                </FormField>
                <FormField label="Trạng thái">
                  <select
                    className={selectCls}
                    value={invoiceForm.status}
                    onChange={(event) =>
                      setInvoiceForm({
                        ...invoiceForm,
                        status: event.target.value as InvoiceStatus,
                      })
                    }
                  >
                    <option value="unpaid">Chưa thanh toán</option>
                    <option value="paid">Đã thanh toán</option>
                    <option value="overdue">Quá hạn</option>
                  </select>
                </FormField>
              </div>
              <div className="flex justify-end gap-2">
                <button
                  onClick={() => setIsEditing(false)}
                  className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
                >
                  Hủy
                </button>
                <button
                  onClick={updateInvoice}
                  className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Lưu thay đổi
                </button>
              </div>
            </div>
          ) : (
            <div className="mb-6 border-b border-gray-100 pb-5">
              <div className="flex items-start justify-between mb-4">
                <div>
                  <p className="text-xs text-gray-400 uppercase tracking-wide mb-1">
                    Invoice ID
                  </p>
                  <p className="font-mono text-sm font-semibold text-gray-800">
                    {selected.invoiceId}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <InvoiceStatusBadge status={selected.invoiceStatus} />
                  <button
                    onClick={startEditing}
                    className="px-3 py-1.5 text-xs bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  >
                    Chỉnh sửa
                  </button>
                </div>
              </div>
              <div className="grid grid-cols-4 gap-3">
                <div className="bg-gray-50 rounded-lg p-3">
                  <p className="text-xs text-gray-400 mb-1">Contract ID</p>
                  <p className="text-xs font-mono truncate">
                    {selected.invoiceContractId}
                  </p>
                </div>
                <div className="bg-gray-50 rounded-lg p-3">
                  <p className="text-xs text-gray-400 mb-1">Ngày lập</p>
                  <p className="text-sm">{selected.invoicePaymentDate}</p>
                </div>
                <div className="bg-gray-50 rounded-lg p-3">
                  <p className="text-xs text-gray-400 mb-1">Ngày đến hạn</p>
                  <p className="text-sm">{selected.invoiceDueDate}</p>
                </div>
                <div className="bg-blue-50 rounded-lg p-3">
                  <p className="text-xs text-blue-400 mb-1">Tổng hóa đơn</p>
                  <p className="text-sm font-mono font-bold text-blue-700">
                    {selected.invoiceTotalAmount === null
                      ? "—"
                      : fmt(selected.invoiceTotalAmount)}
                  </p>
                </div>
                <div className="bg-gray-50 rounded-lg p-3">
                  <p className="text-xs text-gray-400 mb-1">Đã thu</p>
                  <p className="text-sm font-mono font-semibold text-emerald-600">
                    {fmt(receivedFor(selected.invoiceId))}
                  </p>
                </div>
                <div className="bg-gray-50 rounded-lg p-3">
                  <p className="text-xs text-gray-400 mb-1">Ngày tạo</p>
                  <p className="text-xs">
                    {new Date(selected.invoiceCreatedAt).toLocaleString(
                      "vi-VN",
                    )}
                  </p>
                </div>
                <div className="bg-gray-50 rounded-lg p-3">
                  <p className="text-xs text-gray-400 mb-1">Cập nhật</p>
                  <p className="text-xs">
                    {new Date(selected.invoiceUpdatedAt).toLocaleString(
                      "vi-VN",
                    )}
                  </p>
                </div>
              </div>
            </div>
          )}

          <div>
            <div className="flex items-center justify-between mb-3">
              <div>
                <p className="text-sm font-bold text-gray-900">
                  Chi tiết hóa đơn theo mặt bằng
                </p>
                <p className="text-xs text-gray-400 mt-0.5">
                  Một hóa đơn hàng tháng có thể bao gồm nhiều mặt bằng
                </p>
              </div>
              <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2 py-1 rounded">
                {invoiceDetails.length} chi tiết
              </span>
            </div>
            <div className="border border-gray-100 rounded-xl overflow-x-auto">
              <table className="w-full min-w-5xl text-sm">
                <thead>
                  <tr className="bg-gray-50 border-b border-gray-100">
                    <th className="text-left px-3 py-2.5 text-xs font-semibold text-gray-400">
                      ID
                    </th>
                    <th className="text-left px-3 py-2.5 text-xs font-semibold text-gray-400">
                      Premise ID
                    </th>
                    <th className="text-right px-3 py-2.5 text-xs font-semibold text-gray-400">
                      Tiền thuê
                    </th>
                    <th className="text-right px-3 py-2.5 text-xs font-semibold text-gray-400">
                      Tiền điện
                    </th>
                    <th className="text-right px-3 py-2.5 text-xs font-semibold text-gray-400">
                      Tiền nước
                    </th>
                    <th className="text-right px-3 py-2.5 text-xs font-semibold text-gray-400">
                      Phí rác
                    </th>
                    <th className="text-right px-3 py-2.5 text-xs font-semibold text-gray-400">
                      Tổng cộng
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {invoiceDetails.map((detail) => (
                    <tr
                      key={detail.invoiceDetailId}
                      className="hover:bg-blue-50/40 transition-colors"
                    >
                      <td className="px-3 py-3 text-xs font-mono text-gray-500">
                        #{detail.invoiceDetailId}
                      </td>
                      <td
                        className="px-3 py-3 text-xs font-mono text-gray-500 max-w-48 truncate"
                        title={detail.invoiceDetailPremiseId}
                      >
                        {detail.invoiceDetailPremiseId}
                      </td>
                      <td className="px-3 py-3 text-right text-xs font-mono text-gray-600 whitespace-nowrap">
                        {fmt(detail.invoiceDetailRentalPrice)}
                      </td>
                      <td className="px-3 py-3 text-right text-xs font-mono text-gray-600 whitespace-nowrap">
                        {fmt(detail.invoiceDetailElectricityFee)}
                      </td>
                      <td className="px-3 py-3 text-right text-xs font-mono text-gray-600 whitespace-nowrap">
                        {fmt(detail.invoiceDetailWaterFee)}
                      </td>
                      <td className="px-3 py-3 text-right text-xs font-mono text-gray-600 whitespace-nowrap">
                        {fmt(detail.invoiceDetailGarbageFee)}
                      </td>
                      <td className="px-3 py-3 text-right text-xs font-mono font-bold text-blue-700 whitespace-nowrap">
                        {fmt(detail.invoiceDetailTotalAmount)}
                      </td>
                    </tr>
                  ))}
                  {!invoiceDetails.length && (
                    <tr>
                      <td
                        colSpan={7}
                        className="py-10 text-center text-sm text-gray-400"
                      >
                        Hóa đơn chưa có chi tiết.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
            <div className="mt-4 rounded-xl border border-gray-100 bg-gray-50 p-4">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <p className="text-sm font-bold text-gray-900">
                    Thêm chi tiết hóa đơn
                  </p>
                  <p className="text-xs text-gray-400 mt-0.5">
                    Tiền thuê được lấy từ hợp đồng và không thể chỉnh sửa
                  </p>
                </div>
                <span className="text-xs font-mono font-semibold text-blue-700 bg-blue-100 px-2 py-1 rounded">
                  Tổng: {fmt(detailTotal)}
                </span>
              </div>
              {availablePremiseIds.length ? (
                <>
                  <div className="grid grid-cols-5 gap-x-3">
                    <FormField label="Premise ID">
                      <select
                        className={selectCls}
                        value={detailForm.premiseId}
                        onChange={(event) =>
                          setDetailForm({
                            ...detailForm,
                            premiseId: event.target.value,
                          })
                        }
                      >
                        {availablePremiseIds.map((premiseId) => (
                          <option key={premiseId} value={premiseId}>
                            {premiseId}
                          </option>
                        ))}
                      </select>
                    </FormField>
                    <FormField label="Tiền thuê">
                      <div className="relative">
                        <input
                          className={`${inputCls} pr-12 bg-gray-100 text-gray-500 cursor-not-allowed`}
                          value={selectedBilling?.rentalPrice ?? 0}
                          readOnly
                        />
                        <span className="absolute inset-y-0 right-3 flex items-center text-xs font-semibold text-gray-400 pointer-events-none">
                          VND
                        </span>
                      </div>
                    </FormField>
                    <FormField label="Tiền điện">
                      <input
                        className={inputCls}
                        type="number"
                        min="0"
                        required
                        value={detailForm.electricityFee}
                        onChange={(event) =>
                          setDetailForm({
                            ...detailForm,
                            electricityFee: event.target.value,
                          })
                        }
                      />
                    </FormField>
                    <FormField label="Tiền nước">
                      <input
                        className={inputCls}
                        type="number"
                        min="0"
                        required
                        value={detailForm.waterFee}
                        onChange={(event) =>
                          setDetailForm({
                            ...detailForm,
                            waterFee: event.target.value,
                          })
                        }
                      />
                    </FormField>
                    <FormField label="Phí rác">
                      <input
                        className={inputCls}
                        type="number"
                        min="0"
                        required
                        value={detailForm.garbageFee}
                        onChange={(event) =>
                          setDetailForm({
                            ...detailForm,
                            garbageFee: event.target.value,
                          })
                        }
                      />
                    </FormField>
                  </div>
                  <div className="flex justify-end">
                    <button
                      onClick={addInvoiceDetail}
                      disabled={
                        !detailForm.premiseId ||
                        [
                          detailForm.electricityFee,
                          detailForm.waterFee,
                          detailForm.garbageFee,
                        ].some((value) => value === "" || Number(value) < 0)
                      }
                      className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-200 disabled:text-gray-400"
                    >
                      Thêm chi tiết
                    </button>
                  </div>
                </>
              ) : (
                <p className="text-sm text-gray-400">
                  Tất cả mặt bằng của hợp đồng đã có trong hóa đơn này.
                </p>
              )}
            </div>
          </div>

          <div className="flex justify-between mt-6 pt-4 border-t border-gray-100">
            <button
              onClick={() => openAddReceipt(selected)}
              className="px-4 py-2 text-sm text-emerald-700 border border-emerald-200 rounded-lg hover:bg-emerald-50"
            >
              Tạo phiếu thu
            </button>
            <button
              onClick={() => setInvoiceModal(null)}
              className="px-4 py-2 text-sm bg-gray-900 text-white rounded-lg hover:bg-gray-800"
            >
              Đóng
            </button>
          </div>
        </Modal>
      )}

      {receiptModal && (
        <Modal
          title="Tạo phiếu thu"
          onClose={() => setReceiptModal(false)}
          wide
        >
          <div className="grid grid-cols-3 gap-x-4">
            <FormField label="Invoice ID">
              <select
                className={selectCls}
                value={receiptForm.invoiceId}
                onChange={(event) =>
                  setReceiptForm({
                    ...receiptForm,
                    invoiceId: event.target.value,
                  })
                }
              >
                {invoices.map((invoice) => (
                  <option key={invoice.invoiceId} value={invoice.invoiceId}>
                    {invoice.invoiceId}
                  </option>
                ))}
              </select>
            </FormField>
            <FormField label="Account ID người tạo">
              <input
                className={inputCls}
                value={receiptForm.createdByAccountId}
                onChange={(event) =>
                  setReceiptForm({
                    ...receiptForm,
                    createdByAccountId: event.target.value,
                  })
                }
              />
            </FormField>
            <FormField label="Phương thức">
              <select
                className={selectCls}
                value={receiptForm.paymentMethod}
                onChange={(event) =>
                  setReceiptForm({
                    ...receiptForm,
                    paymentMethod: event.target.value as ReceiptPaymentMethod,
                  })
                }
              >
                {Object.entries(paymentMethodLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </FormField>
            <FormField label="Số tiền">
              <input
                className={inputCls}
                type="number"
                min="0"
                value={receiptForm.amount}
                onChange={(event) =>
                  setReceiptForm({ ...receiptForm, amount: event.target.value })
                }
              />
            </FormField>
            <FormField label="Ngày thanh toán">
              <input
                className={inputCls}
                type="date"
                value={receiptForm.paymentDate}
                onChange={(event) =>
                  setReceiptForm({
                    ...receiptForm,
                    paymentDate: event.target.value,
                  })
                }
              />
            </FormField>
            <FormField label="Ngân hàng">
              <input
                className={inputCls}
                maxLength={100}
                value={receiptForm.bankName}
                onChange={(event) =>
                  setReceiptForm({
                    ...receiptForm,
                    bankName: event.target.value,
                  })
                }
              />
            </FormField>
            <FormField label="Mã tham chiếu">
              <input
                className={inputCls}
                maxLength={100}
                value={receiptForm.transactionReference}
                onChange={(event) =>
                  setReceiptForm({
                    ...receiptForm,
                    transactionReference: event.target.value,
                  })
                }
              />
            </FormField>
            <FormField label="Mã giao dịch cổng">
              <input
                className={inputCls}
                maxLength={100}
                value={receiptForm.gatewayTransactionNumber}
                onChange={(event) =>
                  setReceiptForm({
                    ...receiptForm,
                    gatewayTransactionNumber: event.target.value,
                  })
                }
              />
            </FormField>
            <FormField label="Thông tin giao dịch">
              <input
                className={inputCls}
                maxLength={250}
                value={receiptForm.transactionInfo}
                onChange={(event) =>
                  setReceiptForm({
                    ...receiptForm,
                    transactionInfo: event.target.value,
                  })
                }
              />
            </FormField>
          </div>
          <div className="flex justify-end gap-2">
            <button
              onClick={() => setReceiptModal(false)}
              className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Hủy
            </button>
            <button
              onClick={addReceipt}
              disabled={
                !receiptForm.invoiceId ||
                !receiptForm.createdByAccountId ||
                Number(receiptForm.amount) <= 0
              }
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-200 disabled:text-gray-400"
            >
              Tạo phiếu thu
            </button>
          </div>
        </Modal>
      )}

      {selectedReceipt && (
        <Modal
          title="Chi tiết phiếu thu"
          onClose={() => setSelectedReceipt(null)}
          wide
        >
          <div className="grid grid-cols-3 gap-x-4">
            <FormField label="Receipt ID">
              <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`} value={selectedReceipt.receiptId} disabled />
            </FormField>
            <FormField label="Invoice ID">
              <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`} value={selectedReceipt.receiptInvoiceId} disabled />
            </FormField>
            <FormField label="Account ID người tạo">
              <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`} value={selectedReceipt.receiptCreatedByAccountId} disabled />
            </FormField>
            <FormField label="Phương thức">
              <select className={`${selectCls} disabled:bg-gray-50 disabled:text-gray-500`} value={selectedReceipt.receiptPaymentMethod} disabled>
                {Object.entries(paymentMethodLabels).map(([value, label]) => (
                  <option key={value} value={value}>{label}</option>
                ))}
              </select>
            </FormField>
            <FormField label="Số tiền">
              <div className="relative">
                <input
                  className={`${inputCls} pr-14 disabled:bg-gray-50 disabled:text-gray-500`}
                  type="number"
                  min="1"
                  value={receiptEditForm.amount}
                  disabled={!receiptIsEditing}
                  onChange={(event) => setReceiptEditForm({ ...receiptEditForm, amount: event.target.value })}
                />
                <span className="absolute inset-y-0 right-3 flex items-center text-xs font-semibold text-gray-400 pointer-events-none">VND</span>
              </div>
            </FormField>
            <FormField label="Ngày thanh toán">
              <input
                className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                type="date"
                value={receiptEditForm.paymentDate}
                disabled={!receiptIsEditing}
                onChange={(event) => setReceiptEditForm({ ...receiptEditForm, paymentDate: event.target.value })}
              />
            </FormField>
            <FormField label="Mã tham chiếu">
              <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`} value={selectedReceipt.receiptTransactionReference ?? "—"} disabled />
            </FormField>
            <FormField label="Mã giao dịch cổng">
              <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`} value={selectedReceipt.receiptGatewayTransactionNumber ?? "—"} disabled />
            </FormField>
            <FormField label="Ngân hàng">
              <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`} value={selectedReceipt.receiptBankName ?? "—"} disabled />
            </FormField>
            <FormField label="Thông tin giao dịch">
              <input
                className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                maxLength={250}
                value={receiptEditForm.transactionInfo}
                disabled={!receiptIsEditing}
                onChange={(event) => setReceiptEditForm({ ...receiptEditForm, transactionInfo: event.target.value })}
              />
            </FormField>
            <FormField label="Ngày tạo">
              <input className={inputCls} value={new Date(selectedReceipt.receiptCreatedAt).toLocaleString("vi-VN")} disabled />
            </FormField>
            <FormField label="Ngày cập nhật">
              <input className={inputCls} value={new Date(selectedReceipt.receiptUpdatedAt).toLocaleString("vi-VN")} disabled />
            </FormField>
          </div>
          <div className="flex justify-end gap-2 mt-5">
            {receiptIsEditing ? (
              <>
                <button
                  onClick={() => {
                    setReceiptEditForm({
                      paymentDate: selectedReceipt.receiptPaymentDate.slice(0, 10),
                      amount: String(selectedReceipt.receiptAmount),
                      transactionInfo: selectedReceipt.receiptTransactionInfo ?? "",
                    })
                    setReceiptIsEditing(false)
                  }}
                  className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
                >Hủy</button>
                <button
                  onClick={updateReceipt}
                  disabled={!receiptEditForm.paymentDate || Number(receiptEditForm.amount) <= 0}
                  className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-200 disabled:text-gray-400"
                >Lưu thay đổi</button>
              </>
            ) : (
              <>
                <button
                  onClick={() => setSelectedReceipt(null)}
                  className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
                >Đóng</button>
                <button
                  onClick={startEditingReceipt}
                  className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >Chỉnh sửa</button>
              </>
            )}
          </div>
        </Modal>
      )}
    </div>
  )
}
