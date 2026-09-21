using Contract.Utils.Enum;

namespace Contract.Models.Invoice
{
    public class MonthlyInvoiceModel
    {
        public MonthlyInvoiceModel(Guid contractId)
        {
            InvoiceContractId = contractId;
        }

        public MonthlyInvoiceModel(
            Guid invoiceId,
            Guid contractId,
            DateTime paymentDate,
            DateTime dueDate,
            decimal? totalAmount,
            EInvoiceStatus status)
        {
            InvoiceId = invoiceId;
            InvoiceContractId = contractId;
            InvoicePaymentDate = paymentDate;
            InvoiceDueDate = dueDate;
            InvoiceTotalAmount = totalAmount;
            InvoiceStatus = status;
        }

        public MonthlyInvoiceModel() {}

        public Guid InvoiceId { get; init; }

        public Guid InvoiceContractId { get; private set; }

        public DateTime InvoicePaymentDate { get; private set; } = DateTime.Now;

        public DateTime InvoiceDueDate { get; private set; } = DateTime.Now.AddDays(30);

        public decimal? InvoiceTotalAmount { get; private set; }

        public EInvoiceStatus InvoiceStatus { get; private set; } = EInvoiceStatus.Unpaid;

        public DateTime InvoiceCreatedAt { get; init; } = DateTime.Now;

        public DateTime InvoiceUpdatedAt { get; private set; } = DateTime.Now;

        public IReadOnlyList<InvoiceDetailModel> InvoiceDetails { get; private set; } = new List<InvoiceDetailModel>();

        public ReceiptModel? InvoiceReceipt { get; private set; }

        #region Setter

        public void SetPaymentDate(DateTime paymentDate)
        {
            InvoicePaymentDate = paymentDate;
        }

        public void SetDueDate(DateTime dueDate)
        {
            InvoiceDueDate = dueDate;
        }

        public void SetTotalAmount(decimal? totalAmount)
        {
            InvoiceTotalAmount = totalAmount;
        }

        public void SetStatus(EInvoiceStatus status)
        {
            InvoiceStatus = status;
        }

        public void SetContractId(Guid contractId)
        {
            InvoiceContractId = contractId;
        }

        public void SetReceipt(ReceiptModel? receipt)
        {
            InvoiceReceipt = receipt;
        }

        #endregion
    }
}