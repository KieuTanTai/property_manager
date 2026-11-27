import {useEffect, useRef, useState} from "react"
import {BaseTable, FormField, inputCls, InvoiceStatusBadge, Modal, ModulePageHeader, selectCls,} from "./shared"
import type {InvoiceStatus} from "@/types/modules.ts"
import type {InvoiceDetail, InvoiceTab, MonthlyInvoice, Receipt, ReceiptPaymentMethod} from "@/types/invoices"
import {contractApi} from "@/api/contracts"
import {premiseApi} from "@/api/premise"
import type {ContractModel, MonthlyInvoiceModel, ReceiptModel} from "@/types/contracts"
import type {PremiseModel} from "@/components/premises/types"
import {createUuidV7} from "@/scripts/ids.ts"
import {fmt, isIntegerAmountInput, today} from "@/scripts/format.ts"
import {hasAnyPermissionForRole, hasPermission, PERMISSIONS} from "@/utils/authorization"

const paymentMethodLabels: Record<ReceiptPaymentMethod, string> = {
    cash: "Tiền mặt",
    vnpay: "VNPAY",
    bank: "Chuyển khoản",
}

const toUiInvoice = (invoice: MonthlyInvoiceModel): MonthlyInvoice => ({
    ...invoice,
    invoiceStatus: invoice.invoiceStatus.toLowerCase() as InvoiceStatus,
})

const toUiDetail = (detail: MonthlyInvoiceModel["invoiceDetails"][number]): InvoiceDetail => ({...detail})

const toUiReceipt = (receipt: ReceiptModel): Receipt => ({
    ...receipt,
    receiptPaymentMethod: receipt.receiptPaymentMethod.toLowerCase() as ReceiptPaymentMethod,
})

async function loadInvoicesData(includeReceipts: boolean, includePremises: boolean, signal: AbortSignal) {
    const [receipts, contracts, premises] = await Promise.all([
        includeReceipts ? contractApi.receipts(signal) : Promise.resolve([]),
        (async () => {
            const results: ContractModel[] = []
            let cursor: string | null = null
            do {
                const page = await contractApi.contracts(cursor, signal)
                results.push(...page.items)
                cursor = page.isHasMore ? page.nextCursor : null
            } while (cursor)
            return results
        })(),
        includePremises ? premiseApi.getPremise(signal) : Promise.resolve([]),
    ])
    const invoiceModels: MonthlyInvoiceModel[] = []
    let cursor: string | null = null
    do {
        const page = await contractApi.invoices(cursor, signal)
        invoiceModels.push(...page.items)
        cursor = page.isHasMore ? page.nextCursor : null
    } while (cursor)
    return {
        invoices: invoiceModels.map(toUiInvoice),
        details: invoiceModels.flatMap((invoice) => (invoice.invoiceDetails ?? []).map(toUiDetail)),
        receipts: receipts.map(toUiReceipt),
        contracts,
        premises,
    }
}

export function InvoicesModule({roleCodes = [], permissionCodes = [], accountId, ownOnly = false}: {
    roleCodes?: string[];
    permissionCodes?: string[];
    accountId?: string;
    ownOnly?: boolean
}) {
    const can = (permission: string) => hasPermission(permissionCodes, permission, roleCodes)
    const canAny = (...permissions: string[]) => hasAnyPermissionForRole(permissionCodes, roleCodes, ...permissions)
    const canCreateInvoice = can(PERMISSIONS.invoiceCreate)
    const canUpdateInvoice = canAny(PERMISSIONS.invoiceUpdate, PERMISSIONS.invoiceStatusUpdate)
    const canCreateReceipt = can(PERMISSIONS.receiptCreate)
    const canReadReceipts = canAny("RECEIPT_READ_LIST", "RECEIPT_READ_OWN", PERMISSIONS.receiptCreate)
    const [invoices, setInvoices] = useState<MonthlyInvoice[]>([])
    const [details, setDetails] = useState<InvoiceDetail[]>([])
    const [receipts, setReceipts] = useState<Receipt[]>([])
    const [contracts, setContracts] = useState<ContractModel[]>([])
    const [premises, setPremises] = useState<PremiseModel[]>([])
    const [isLoading, setIsLoading] = useState(true)
    const [isSaving, setIsSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)
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
        createdByAccountId: accountId ?? "",
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
    const [createPremiseIds, setCreatePremiseIds] = useState<string[]>([])
    const [receiptEditForm, setReceiptEditForm] = useState({
        paymentDate: "",
        amount: "",
        transactionInfo: "",
    })
    const loadControllerRef = useRef<AbortController | null>(null)

    const ownContractIds = new Set(contracts.filter((contract) => contract.contractAccountId === accountId).map((contract) => contract.contractId))
    const visibleInvoices = ownOnly ? invoices.filter((invoice) => ownContractIds.has(invoice.invoiceContractId)) : invoices
    const visibleInvoiceIds = new Set(visibleInvoices.map((invoice) => invoice.invoiceId))
    const visibleReceipts = ownOnly ? receipts.filter((receipt) => visibleInvoiceIds.has(receipt.receiptInvoiceId)) : receipts
    const filtered =
        filter === "all"
            ? visibleInvoices
            : visibleInvoices.filter((invoice) => invoice.invoiceStatus === filter)
    const totalUnpaid = visibleInvoices
        .filter((invoice) => invoice.invoiceStatus !== "paid")
        .reduce((sum, invoice) => sum + (invoice.invoiceTotalAmount ?? 0), 0)

    const refreshData = async () => {
        loadControllerRef.current?.abort()
        const controller = new AbortController()
        loadControllerRef.current = controller
        const data = await loadInvoicesData(canReadReceipts, canCreateInvoice, controller.signal)
        setInvoices(data.invoices)
        setDetails(data.details)
        setReceipts(data.receipts)
        setContracts(data.contracts)
        setPremises(data.premises)
        return data
    }

    useEffect(() => {
        let isActive = true
        loadControllerRef.current?.abort()
        const controller = new AbortController()
        loadControllerRef.current = controller
        void loadInvoicesData(canReadReceipts, canCreateInvoice, controller.signal)
            .then((data) => {
                if (!isActive) return
                setInvoices(data.invoices)
                setDetails(data.details)
                setReceipts(data.receipts)
                setContracts(data.contracts)
                setPremises(data.premises)
            })
            .catch((loadError: unknown) => {
                if (isActive && !controller.signal.aborted) {
                    setError(loadError instanceof Error ? loadError.message : "Không tải được dữ liệu hóa đơn.")
                }
            })
            .finally(() => {
                if (isActive && !controller.signal.aborted) setIsLoading(false)
            })
        return () => {
            isActive = false
            controller.abort()
            if (loadControllerRef.current === controller) loadControllerRef.current = null
        }
    }, [canCreateInvoice, canReadReceipts])

    const runMutation = async (operation: () => Promise<void>) => {
        setError(null)
        setIsSaving(true)
        try {
            await operation()
        } catch (mutationError) {
            setError(mutationError instanceof Error ? mutationError.message : "Không thể lưu thay đổi.")
        } finally {
            setIsSaving(false)
        }
    }

    const openAddInvoice = () => {
        const dueDate = new Date()
        dueDate.setDate(dueDate.getDate() + 30)
        const availableContracts = contracts.filter((contract) => !ownOnly || contract.contractAccountId === accountId)
        const contractId = availableContracts[0]?.contractId ?? ""
        setInvoiceForm({
            contractId,
            paymentDate: today(),
            dueDate: dueDate.toISOString().slice(0, 10),
            totalAmount: "",
            status: "unpaid",
        })
        setCreatePremiseIds([])
        setCreateDetailFees({})
        setInvoiceModal("add")
    }

    const selectInvoiceContract = (contractId: string) => {
        setInvoiceForm({...invoiceForm, contractId})
    }

    const openInvoice = (invoice: MonthlyInvoice) => {
        setSelected(invoice)
        setIsEditing(false)
        const availablePremise = premises.find((premise) => premise.premiseStatus === "Rented" &&
            !details.some((detail) => detail.invoiceDetailInvoiceId === invoice.invoiceId && detail.invoiceDetailPremiseId === premise.premiseId),
        )?.premiseId ?? ""
        setDetailForm({
            premiseId: availablePremise,
            electricityFee: "",
            waterFee: "",
            garbageFee: "",
        })
        setInvoiceModal("detail")
    }

    const addInvoice = async () => {
        const contract = contracts.find((item) => item.contractId === invoiceForm.contractId)
        if (!contract) return
        const invoiceDetails = createPremiseIds.map((premiseId) => {
            const fees = createDetailFees[premiseId]
            return {
                premiseId,
                rentalPrice: contract.contractRentalPrice,
                electricityFee: Number(fees?.electricityFee),
                waterFee: Number(fees?.waterFee),
                garbageFee: Number(fees?.garbageFee),
            }
        })
        const totalAmount = invoiceDetails.reduce(
            (sum, detail) => sum + detail.rentalPrice + detail.electricityFee + detail.waterFee + detail.garbageFee,
            0,
        )
        await runMutation(async () => {
            const created = await contractApi.createInvoice({
                invoiceId: createUuidV7(),
                contractId: invoiceForm.contractId,
                paymentDate: invoiceForm.paymentDate,
                dueDate: invoiceForm.dueDate,
                totalAmount,
                status: "Unpaid",
                details: invoiceDetails,
            })
            const data = await refreshData()
            const invoice = data.invoices.find((item) => item.invoiceId === created.invoiceId)
            if (invoice) {
                setSelected(invoice)
                setInvoiceModal("detail")
            } else {
                setInvoiceModal(null)
            }
        })
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

    const updateInvoice = async () => {
        if (!selected) return
        await runMutation(async () => {
            await contractApi.updateInvoice(selected.invoiceId, {
                contractId: invoiceForm.contractId,
                paymentDate: invoiceForm.paymentDate,
                dueDate: invoiceForm.dueDate,
                totalAmount: selected.invoiceTotalAmount ?? 0,
                status: invoiceForm.status.replace(/^\w/, (letter) => letter.toUpperCase()) as MonthlyInvoiceModel["invoiceStatus"],
                details: details.filter((detail) => detail.invoiceDetailInvoiceId === selected.invoiceId).map((detail) => ({
                    detailId: detail.invoiceDetailId,
                    premiseId: detail.invoiceDetailPremiseId,
                    rentalPrice: detail.invoiceDetailRentalPrice,
                    electricityFee: detail.invoiceDetailElectricityFee,
                    waterFee: detail.invoiceDetailWaterFee,
                    garbageFee: detail.invoiceDetailGarbageFee,
                })),
            })
            await refreshData()
            setSelected({
                ...selected,
                invoiceContractId: invoiceForm.contractId,
                invoicePaymentDate: invoiceForm.paymentDate,
                invoiceDueDate: invoiceForm.dueDate,
                invoiceStatus: invoiceForm.status
            })
            setIsEditing(false)
        })
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

    const syncInvoicePaymentStatus = async (invoice: MonthlyInvoice, amountReceived: number) => {
        if (invoice.invoiceTotalAmount === null) return
        const nextStatus: InvoiceStatus = amountReceived >= invoice.invoiceTotalAmount
            ? "paid"
            : invoice.invoiceDueDate.slice(0, 10) < today() ? "overdue" : "unpaid"
        if (nextStatus === invoice.invoiceStatus) return
        await contractApi.updateInvoice(invoice.invoiceId, {
            contractId: invoice.invoiceContractId,
            paymentDate: invoice.invoicePaymentDate,
            dueDate: invoice.invoiceDueDate,
            totalAmount: invoice.invoiceTotalAmount,
            status: nextStatus.replace(/^\w/, (letter) => letter.toUpperCase()) as MonthlyInvoiceModel["invoiceStatus"],
        })
    }

    const addReceipt = async () => {
        await runMutation(async () => {
            const created = await contractApi.createReceipt({
                invoiceId: receiptForm.invoiceId,
                createdByAccountId: receiptForm.createdByAccountId,
                paymentMethod: receiptForm.paymentMethod === "vnpay" ? "VnPay" : receiptForm.paymentMethod === "bank" ? "Bank" : "Cash",
                amount: Number(receiptForm.amount),
                paymentDate: receiptForm.paymentDate,
                transactionReference: receiptForm.transactionReference.trim() || null,
                gatewayTransactionNumber: receiptForm.gatewayTransactionNumber.trim() || null,
                transactionInfo: receiptForm.transactionInfo.trim() || null,
                bankName: receiptForm.bankName.trim() || null,
            })
            const relatedInvoice = invoices.find((invoice) => invoice.invoiceId === created.receiptInvoiceId)
            try {
                if (relatedInvoice) {
                    const amountReceived = receipts
                        .filter((receipt) => receipt.receiptInvoiceId === created.receiptInvoiceId)
                        .reduce((sum, receipt) => sum + receipt.receiptAmount, 0) + created.receiptAmount
                    await syncInvoicePaymentStatus(relatedInvoice, amountReceived)
                }
            } finally {
                await refreshData()
            }
            setReceiptModal(false)
        })
    }

    const addInvoiceDetail = async () => {
        if (!selected) return
        const contract = contracts.find((item) => item.contractId === selected.invoiceContractId)
        const electricityFee = Number(detailForm.electricityFee)
        const waterFee = Number(detailForm.waterFee)
        const garbageFee = Number(detailForm.garbageFee)
        if (
            !contract ||
            !detailForm.premiseId ||
            [
                detailForm.electricityFee,
                detailForm.waterFee,
                detailForm.garbageFee,
            ].some((value) => value === "") ||
            [electricityFee, waterFee, garbageFee].some((value) => value < 0)
        )
            return

        const nextDetails = [
            ...details.filter((item) => item.invoiceDetailInvoiceId === selected.invoiceId),
            {
                invoiceDetailId: 0,
                invoiceDetailInvoiceId: selected.invoiceId,
                invoiceDetailPremiseId: detailForm.premiseId,
                invoiceDetailRentalPrice: contract.contractRentalPrice,
                invoiceDetailElectricityFee: electricityFee,
                invoiceDetailWaterFee: waterFee,
                invoiceDetailGarbageFee: garbageFee,
                invoiceDetailTotalAmount: contract.contractRentalPrice + electricityFee + waterFee + garbageFee,
            },
        ]
        const total = nextDetails.reduce((sum, detail) => sum + detail.invoiceDetailTotalAmount, 0)
        await runMutation(async () => {
            await contractApi.updateInvoice(selected.invoiceId, {
                contractId: selected.invoiceContractId,
                paymentDate: selected.invoicePaymentDate,
                dueDate: selected.invoiceDueDate,
                totalAmount: total,
                status: selected.invoiceStatus.replace(/^\w/, (letter) => letter.toUpperCase()) as MonthlyInvoiceModel["invoiceStatus"],
                details: nextDetails.map((detail) => ({
                    detailId: detail.invoiceDetailId,
                    premiseId: detail.invoiceDetailPremiseId,
                    rentalPrice: detail.invoiceDetailRentalPrice,
                    electricityFee: detail.invoiceDetailElectricityFee,
                    waterFee: detail.invoiceDetailWaterFee,
                    garbageFee: detail.invoiceDetailGarbageFee,
                })),
            })
            await refreshData()
            setDetailForm({premiseId: "", electricityFee: "", waterFee: "", garbageFee: ""})
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

    const updateReceipt = async () => {
        if (!selectedReceipt || Number(receiptEditForm.amount) <= 0) return
        await runMutation(async () => {
            const updated = await contractApi.updateReceipt(
                selectedReceipt.receiptId,
                selectedReceipt.receiptInvoiceId,
                {
                    paymentDate: receiptEditForm.paymentDate,
                    amount: Number(receiptEditForm.amount),
                    transactionInfo: receiptEditForm.transactionInfo.trim() || null,
                },
            )
            const relatedInvoice = invoices.find((invoice) => invoice.invoiceId === updated.receiptInvoiceId)
            try {
                if (relatedInvoice) {
                    const amountReceived = receipts
                        .filter((receipt) => receipt.receiptInvoiceId === updated.receiptInvoiceId && receipt.receiptId !== updated.receiptId)
                        .reduce((sum, receipt) => sum + receipt.receiptAmount, 0) + updated.receiptAmount
                    await syncInvoicePaymentStatus(relatedInvoice, amountReceived)
                }
            } finally {
                await refreshData()
            }
            setSelectedReceipt(toUiReceipt(updated))
            setReceiptIsEditing(false)
        })
    }

    const invoiceDetails = selected
        ? details.filter(
            (detail) => detail.invoiceDetailInvoiceId === selected.invoiceId,
        )
        : []
    const availablePremiseIds =
        premises.filter((premise) => premise.premiseStatus === "Rented").map((premise) => premise.premiseId).filter(
            (premiseId) =>
                !invoiceDetails.some(
                    (detail) => detail.invoiceDetailPremiseId === premiseId,
                ),
        ) ?? []
    const selectedContract = selected ? contracts.find((contract) => contract.contractId === selected.invoiceContractId) : undefined
    const detailTotal =
        (selectedContract?.contractRentalPrice ?? 0) +
        (Number(detailForm.electricityFee) || 0) +
        (Number(detailForm.waterFee) || 0) +
        (Number(detailForm.garbageFee) || 0)
    const createInvoiceDetailsValid =
        createPremiseIds.length > 0 &&
        createPremiseIds.every((premiseId) => {
            const fees = createDetailFees[premiseId]
            return fees && [fees.electricityFee, fees.waterFee, fees.garbageFee].every(
                (value) => value !== "" && Number(value) >= 0,
            )
        })
    const createInvoiceTotal = createPremiseIds.reduce((sum, premiseId) => {
        const fees = createDetailFees[premiseId]
        return (
            sum +
            (contracts.find((contract) => contract.contractId === invoiceForm.contractId)?.contractRentalPrice ?? 0) +
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
            <ModulePageHeader title="Hóa đơn hàng tháng" description={<>Tổng chưa thu: <span
                className="font-mono text-red-500">{fmt(totalUnpaid)}</span></>}>
                {(activeTab === "invoices" ? canCreateInvoice : canCreateReceipt) && <button
                    onClick={
                        activeTab === "invoices"
                            ? openAddInvoice
                            : () => openAddReceipt(null)
                    }
                    className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors flex items-center gap-2"
                    disabled={isLoading || isSaving}
                >
                    <span>+</span>{" "}
                    {activeTab === "invoices" ? "Tạo hóa đơn" : "Tạo phiếu thu"}
                </button>}
            </ModulePageHeader>

            {error && (
                <div role="alert"
                     className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    {error}
                </div>
            )}

            <div className="border-b border-gray-200 mb-5">
                <div className="flex gap-6">
                    {([
                        {id: "invoices", label: ownOnly ? "Hóa đơn của tôi" : "Hóa đơn", count: visibleInvoices.length},
                        ...(canReadReceipts ? [{
                            id: "receipts" as const,
                            label: "Phiếu thu",
                            count: visibleReceipts.length
                        }] : []),
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

            {isLoading ? (
                <p className="py-12 text-center text-sm text-gray-500">Đang tải dữ liệu từ database...</p>
            ) : activeTab === "invoices" ? (
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
                                    ? visibleInvoices.length
                                    : visibleInvoices.filter(
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
                    <BaseTable
                        rows={filtered}
                        getRowKey={(invoice) => invoice.invoiceId}
                        tableClassName="w-full min-w-5xl text-sm"
                        paginationResetKey={filter}
                        rowClassName={() => "hover:bg-blue-50/50 focus:bg-blue-50/50 focus:outline-none transition-colors"}
                        rowTitle="Bấm để xem các chi tiết hóa đơn"
                        onRowClick={openInvoice}
                        columns={[
                            {
                                key: "invoiceId",
                                header: "Invoice ID",
                                cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-500",
                                render: (invoice) => invoice.invoiceId
                            },
                            {
                                key: "contractId",
                                header: "Contract ID",
                                cellClassName: "max-w-44 truncate px-4 py-3 font-mono text-xs text-gray-500",
                                render: (invoice) => invoice.invoiceContractId
                            },
                            {
                                key: "paymentDate",
                                header: "Ngày lập",
                                cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500",
                                render: (invoice) => invoice.invoicePaymentDate
                            },
                            {
                                key: "dueDate",
                                header: "Ngày đến hạn",
                                cellClassName: "whitespace-nowrap px-4 py-3 text-xs text-gray-500",
                                render: (invoice) => invoice.invoiceDueDate
                            },
                            {
                                key: "amount",
                                header: "Tổng tiền",
                                headerClassName: "px-4 py-3 text-right font-medium",
                                cellClassName: "whitespace-nowrap px-4 py-3 text-right font-mono text-xs font-semibold text-gray-800",
                                render: (invoice) => invoice.invoiceTotalAmount === null ? "—" : fmt(invoice.invoiceTotalAmount)
                            },
                            {
                                key: "details",
                                header: "Chi tiết",
                                headerClassName: "px-4 py-3 text-center font-medium",
                                cellClassName: "px-4 py-3 text-center",
                                render: (invoice) => <span
                                    className="rounded bg-blue-50 px-2 py-1 text-xs font-semibold text-blue-600">{details.filter((detail) => detail.invoiceDetailInvoiceId === invoice.invoiceId).length}</span>
                            },
                            {
                                key: "status",
                                header: "Trạng thái",
                                render: (invoice) => <InvoiceStatusBadge status={invoice.invoiceStatus}/>
                            },
                        ]}
                    />
                </>
            ) : (
                <BaseTable
                    rows={visibleReceipts}
                    getRowKey={(receipt) => receipt.receiptId}
                    tableClassName="w-full min-w-5xl text-sm"
                    rowClassName={() => "hover:bg-blue-50/50 transition-colors"}
                    onRowClick={openReceipt}
                    columns={[
                        {
                            key: "receiptId",
                            header: "Receipt ID",
                            cellClassName: "max-w-48 truncate px-4 py-3 font-mono text-xs text-gray-500",
                            render: (receipt) => receipt.receiptId
                        },
                        {
                            key: "invoiceId",
                            header: "Invoice ID",
                            cellClassName: "max-w-48 truncate px-4 py-3 font-mono text-xs text-gray-500",
                            render: (receipt) => receipt.receiptInvoiceId
                        },
                        {
                            key: "method",
                            header: "Phương thức",
                            cellClassName: "px-4 py-3 text-xs text-gray-600",
                            render: (receipt) => paymentMethodLabels[receipt.receiptPaymentMethod]
                        },
                        {
                            key: "amount",
                            header: "Số tiền",
                            headerClassName: "px-4 py-3 text-right font-medium",
                            cellClassName: "px-4 py-3 text-right font-mono text-xs font-semibold text-emerald-600",
                            render: (receipt) => fmt(receipt.receiptAmount)
                        },
                        {
                            key: "paymentDate",
                            header: "Ngày thanh toán",
                            cellClassName: "px-4 py-3 text-xs text-gray-500",
                            render: (receipt) => receipt.receiptPaymentDate
                        },
                        {
                            key: "bank",
                            header: "Ngân hàng",
                            cellClassName: "px-4 py-3 text-xs text-gray-500",
                            render: (receipt) => receipt.receiptBankName || "—"
                        },
                    ]}
                />
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
                                {contracts
                                    .filter((contract) => !ownOnly || contract.contractAccountId === accountId)
                                    .map((contract) => (
                                        <option key={contract.contractId} value={contract.contractId}>
                                            {contract.contractId} · {fmt(contract.contractRentalPrice)}/tháng
                                        </option>
                                    ))}
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

                    <div className="mb-5 overflow-hidden rounded-xl border border-gray-100">
                        <div className="max-h-72 overflow-auto">
                            <div className="min-w-225">
                        <div className="sticky top-0 z-10 grid grid-cols-5 gap-3 bg-gray-50 px-4 py-2.5 text-xs font-semibold text-gray-400">
                            <span>Mặt bằng</span>
                            <span className="text-right">Tiền thuê</span>
                            <span className="text-right">Tiền điện</span>
                            <span className="text-right">Tiền nước</span>
                            <span className="text-right">Phí rác</span>
                        </div>
                        {premises.filter((premise) => premise.premiseStatus === "Rented").map((premise) => {
                            const premiseId = premise.premiseId
                            const isSelected = createPremiseIds.includes(premiseId)
                            const fees = createDetailFees[premiseId] ?? {
                                electricityFee: "",
                                waterFee: "",
                                garbageFee: "",
                            }
                            return (
                                <div
                                    key={premiseId}
                                    className="grid grid-cols-5 items-center gap-3 border-t border-gray-50 px-4 py-3"
                                >
                                    <label className="flex min-w-0 items-center gap-2 text-xs text-gray-700">
                                        <input
                                            type="checkbox"
                                            checked={isSelected}
                                            onChange={(event) => {
                                                setCreatePremiseIds(event.target.checked
                                                    ? [...createPremiseIds, premiseId]
                                                    : createPremiseIds.filter((id) => id !== premiseId))
                                                if (event.target.checked) {
                                                    setCreateDetailFees({
                                                        ...createDetailFees,
                                                        [premiseId]: {electricityFee: "", waterFee: "", garbageFee: ""},
                                                    })
                                                }
                                            }}
                                        />
                                        <span className="truncate" title={premise.premiseName}>{premise.premiseName}</span>
                                    </label>
                                    <div className="relative">
                                        <input
                                            className={`${inputCls} pr-12 bg-gray-100 text-gray-500 cursor-not-allowed text-right disabled:opacity-50`}
                                            value={
                                                contracts.find((contract) => contract.contractId === invoiceForm.contractId)?.contractRentalPrice ?? 0
                                            }
                                            disabled={!isSelected}
                                            readOnly
                                        />
                                        <span
                                            className="absolute inset-y-0 right-2 flex items-center text-xs text-gray-400 pointer-events-none">
                        VND
                      </span>
                                    </div>
                                    <input
                                        className={`${inputCls} text-right`}
                                        type="number"
                                        min="0"
                                        step="1"
                                        required
                                        value={fees.electricityFee}
                                        disabled={!isSelected}
                                        onChange={(event) => {
                                            if (!isIntegerAmountInput(event.target.value)) return
                                            setCreateDetailFees({
                                                ...createDetailFees,
                                                [premiseId]: {
                                                    ...fees,
                                                    electricityFee: event.target.value,
                                                },
                                            })
                                        }}
                                        placeholder="Bắt buộc"
                                    />
                                    <input
                                        className={`${inputCls} text-right`}
                                        type="number"
                                        min="0"
                                        step="1"
                                        required
                                        value={fees.waterFee}
                                        disabled={!isSelected}
                                        onChange={(event) => {
                                            if (!isIntegerAmountInput(event.target.value)) return
                                            setCreateDetailFees({
                                                ...createDetailFees,
                                                [premiseId]: {
                                                    ...fees,
                                                    waterFee: event.target.value,
                                                },
                                            })
                                        }}
                                        placeholder="Bắt buộc"
                                    />
                                    <input
                                        className={`${inputCls} text-right`}
                                        type="number"
                                        min="0"
                                        step="1"
                                        required
                                        value={fees.garbageFee}
                                        disabled={!isSelected}
                                        onChange={(event) => {
                                            if (!isIntegerAmountInput(event.target.value)) return
                                            setCreateDetailFees({
                                                ...createDetailFees,
                                                [premiseId]: {
                                                    ...fees,
                                                    garbageFee: event.target.value,
                                                },
                                            })
                                        }}
                                        placeholder="Bắt buộc"
                                    />
                                </div>
                            )
                        })}
                            </div>
                        </div>
                    </div>

                    <div
                        className="flex items-center justify-between rounded-lg border border-blue-100 bg-blue-50 px-4 py-3 mb-4">
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
                                isSaving ||
                                !invoiceForm.contractId ||
                                !invoiceForm.paymentDate ||
                                !invoiceForm.dueDate ||
                                !createInvoiceDetailsValid
                            }
                            className="px-4 py-2 text-sm text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-200 disabled:text-gray-400"
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
                                    disabled={isSaving}
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
                                    <InvoiceStatusBadge status={selected.invoiceStatus}/>
                                    {canUpdateInvoice && <button
                                        onClick={startEditing}
                                        className="px-3 py-1.5 text-xs bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                                    >
                                        Chỉnh sửa
                                    </button>}
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
                                    Chi tiết hóa đơn
                                </p>
                                <p className="text-xs text-gray-400 mt-0.5">
                                    Một hóa đơn hàng tháng có thể bao gồm nhiều mặt bằng
                                </p>
                            </div>
                            <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2 py-1 rounded">
                {invoiceDetails.length} chi tiết
              </span>
                        </div>
                        <div className="max-h-50 overflow-auto rounded-xl [&_thead]:sticky [&_thead]:top-0 [&_thead]:z-10">
                        <BaseTable
                            rows={invoiceDetails}
                            getRowKey={(detail) => String(detail.invoiceDetailId)}
                            emptyMessage="Hóa đơn chưa có chi tiết."
                            tableClassName="w-full min-w-5xl text-sm"
                            rowClassName={() => "hover:bg-blue-50/40 transition-colors"}
                            columns={[
                                {
                                    key: "id",
                                    header: "ID",
                                    cellClassName: "px-3 py-3 font-mono text-xs text-gray-500",
                                    render: (detail) => `#${detail.invoiceDetailId}`
                                },
                                {
                                    key: "premise",
                                    header: "Mặt bằng",
                                    cellClassName: "max-w-48 truncate px-3 py-3 text-xs text-gray-700",
                                    render: (detail) => {
                                        const premiseName = premises.find(
                                            (premise) => premise.premiseId === detail.invoiceDetailPremiseId,
                                        )?.premiseName ?? "Không rõ mặt bằng"
                                        return <span title={premiseName}>{premiseName}</span>
                                    }
                                },
                                {
                                    key: "rental",
                                    header: "Tiền thuê",
                                    headerClassName: "px-3 py-2.5 text-right text-xs font-semibold text-gray-400",
                                    cellClassName: "whitespace-nowrap px-3 py-3 text-right font-mono text-xs text-gray-600",
                                    render: (detail) => fmt(detail.invoiceDetailRentalPrice)
                                },
                                {
                                    key: "electricity",
                                    header: "Tiền điện",
                                    headerClassName: "px-3 py-2.5 text-right text-xs font-semibold text-gray-400",
                                    cellClassName: "whitespace-nowrap px-3 py-3 text-right font-mono text-xs text-gray-600",
                                    render: (detail) => fmt(detail.invoiceDetailElectricityFee)
                                },
                                {
                                    key: "water",
                                    header: "Tiền nước",
                                    headerClassName: "px-3 py-2.5 text-right text-xs font-semibold text-gray-400",
                                    cellClassName: "whitespace-nowrap px-3 py-3 text-right font-mono text-xs text-gray-600",
                                    render: (detail) => fmt(detail.invoiceDetailWaterFee)
                                },
                                {
                                    key: "garbage",
                                    header: "Phí rác",
                                    headerClassName: "px-3 py-2.5 text-right text-xs font-semibold text-gray-400",
                                    cellClassName: "whitespace-nowrap px-3 py-3 text-right font-mono text-xs text-gray-600",
                                    render: (detail) => fmt(detail.invoiceDetailGarbageFee)
                                },
                                {
                                    key: "total",
                                    header: "Tổng cộng",
                                    headerClassName: "px-3 py-2.5 text-right text-xs font-semibold text-gray-400",
                                    cellClassName: "whitespace-nowrap px-3 py-3 text-right font-mono text-xs font-bold text-blue-700",
                                    render: (detail) => fmt(detail.invoiceDetailTotalAmount)
                                },
                            ]}
                        />
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
                                <span
                                    className="text-xs font-mono font-semibold text-blue-700 bg-blue-100 px-2 py-1 rounded">
                  Tổng: {fmt(detailTotal)}
                </span>
                            </div>
                            {availablePremiseIds.length ? (
                                <>
                                    <div className="grid grid-cols-5 gap-x-3">
                                        <FormField label="Mặt bằng">
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
                                                        {premises.find((premise) => premise.premiseId === premiseId)?.premiseName ?? "Không rõ mặt bằng"}
                                                    </option>
                                                ))}
                                            </select>
                                        </FormField>
                                        <FormField label="Tiền thuê">
                                            <div className="relative">
                                                <input
                                                    className={`${inputCls} pr-12 bg-gray-100 text-gray-500 cursor-not-allowed`}
                                                    value={selectedContract?.contractRentalPrice ?? 0}
                                                    readOnly
                                                />
                                                <span
                                                    className="absolute inset-y-0 right-3 flex items-center text-xs font-semibold text-gray-400 pointer-events-none">
                          VND
                        </span>
                                            </div>
                                        </FormField>
                                        <FormField label="Tiền điện">
                                            <input
                                                className={inputCls}
                                                type="number"
                                                min="0"
                                                step="1"
                                                required
                                                value={detailForm.electricityFee}
                                                onChange={(event) => {
                                                    if (!isIntegerAmountInput(event.target.value)) return
                                                    setDetailForm({
                                                        ...detailForm,
                                                        electricityFee: event.target.value,
                                                    })
                                                }}
                                            />
                                        </FormField>
                                        <FormField label="Tiền nước">
                                            <input
                                                className={inputCls}
                                                type="number"
                                                min="0"
                                                step="1"
                                                required
                                                value={detailForm.waterFee}
                                                onChange={(event) => {
                                                    if (!isIntegerAmountInput(event.target.value)) return
                                                    setDetailForm({
                                                        ...detailForm,
                                                        waterFee: event.target.value,
                                                    })
                                                }}
                                            />
                                        </FormField>
                                        <FormField label="Phí rác">
                                            <input
                                                className={inputCls}
                                                type="number"
                                                min="0"
                                                step="1"
                                                required
                                                value={detailForm.garbageFee}
                                                onChange={(event) => {
                                                    if (!isIntegerAmountInput(event.target.value)) return
                                                    setDetailForm({
                                                        ...detailForm,
                                                        garbageFee: event.target.value,
                                                    })
                                                }}
                                            />
                                        </FormField>
                                    </div>
                                    <div className="flex justify-end">
                                        <button
                                            onClick={addInvoiceDetail}
                                            disabled={
                                                isSaving ||
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
                                    Không còn mặt bằng đang thuê khả dụng cho chi tiết hóa đơn.
                                </p>
                            )}
                        </div>
                    </div>

                    <div className="flex justify-between mt-6 pt-4 border-t border-gray-100">
                        {canCreateReceipt && <button
                            onClick={() => openAddReceipt(selected)}
                            className="px-4 py-2 text-sm text-emerald-700 border border-emerald-200 rounded-lg hover:bg-emerald-50"
                        >
                            Tạo phiếu thu
                        </button>}
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
                                step="1"
                                value={receiptForm.amount}
                                onChange={(event) => {
                                    if (isIntegerAmountInput(event.target.value))
                                        setReceiptForm({...receiptForm, amount: event.target.value})
                                }}
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
                                isSaving ||
                                !receiptForm.createdByAccountId ||
                                !receiptForm.invoiceId ||
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
                            <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                   value={selectedReceipt.receiptId} disabled/>
                        </FormField>
                        <FormField label="Invoice ID">
                            <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                   value={selectedReceipt.receiptInvoiceId} disabled/>
                        </FormField>
                        <FormField label="Account ID người tạo">
                            <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                   value={selectedReceipt.receiptCreatedByAccountId} disabled/>
                        </FormField>
                        <FormField label="Phương thức">
                            <select className={`${selectCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                    value={selectedReceipt.receiptPaymentMethod} disabled>
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
                                    step="1"
                                    value={receiptEditForm.amount}
                                    disabled={!receiptIsEditing}
                                    onChange={(event) => {
                                        if (isIntegerAmountInput(event.target.value))
                                            setReceiptEditForm({...receiptEditForm, amount: event.target.value})
                                    }}
                                />
                                <span
                                    className="absolute inset-y-0 right-3 flex items-center text-xs font-semibold text-gray-400 pointer-events-none">VND</span>
                            </div>
                        </FormField>
                        <FormField label="Ngày thanh toán">
                            <input
                                className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                type="date"
                                value={receiptEditForm.paymentDate}
                                disabled={!receiptIsEditing}
                                onChange={(event) => setReceiptEditForm({
                                    ...receiptEditForm,
                                    paymentDate: event.target.value
                                })}
                            />
                        </FormField>
                        <FormField label="Mã tham chiếu">
                            <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                   value={selectedReceipt.receiptTransactionReference ?? "—"} disabled/>
                        </FormField>
                        <FormField label="Mã giao dịch cổng">
                            <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                   value={selectedReceipt.receiptGatewayTransactionNumber ?? "—"} disabled/>
                        </FormField>
                        <FormField label="Ngân hàng">
                            <input className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                   value={selectedReceipt.receiptBankName ?? "—"} disabled/>
                        </FormField>
                        <FormField label="Thông tin giao dịch">
                            <input
                                className={`${inputCls} disabled:bg-gray-50 disabled:text-gray-500`}
                                maxLength={250}
                                value={receiptEditForm.transactionInfo}
                                disabled={!receiptIsEditing}
                                onChange={(event) => setReceiptEditForm({
                                    ...receiptEditForm,
                                    transactionInfo: event.target.value
                                })}
                            />
                        </FormField>
                        <FormField label="Ngày tạo">
                            <input className={inputCls}
                                   value={new Date(selectedReceipt.receiptCreatedAt).toLocaleString("vi-VN")} disabled/>
                        </FormField>
                        <FormField label="Ngày cập nhật">
                            <input className={inputCls}
                                   value={new Date(selectedReceipt.receiptUpdatedAt).toLocaleString("vi-VN")} disabled/>
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
                                >Hủy
                                </button>
                                <button
                                    onClick={updateReceipt}
                                    disabled={isSaving || !receiptEditForm.paymentDate || Number(receiptEditForm.amount) <= 0}
                                    className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-200 disabled:text-gray-400"
                                >Lưu thay đổi
                                </button>
                            </>
                        ) : (
                            <>
                                <button
                                    onClick={() => setSelectedReceipt(null)}
                                    className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50"
                                >Đóng
                                </button>
                                {canCreateReceipt && <button
                                    onClick={startEditingReceipt}
                                    className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                                >Chỉnh sửa
                                </button>}
                            </>
                        )}
                    </div>
                </Modal>
            )}
        </div>
    )
}
