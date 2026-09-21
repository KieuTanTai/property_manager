using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace Contract.Models.Invoice
{
    public class ReceiptModel
    {
        public ReceiptModel(Guid invoiceId, Guid createdByAccountId, decimal amount)
        {
            ReceiptInvoiceId = invoiceId;
            ReceiptCreatedByAccountId = createdByAccountId;
            ReceiptAmount = amount;
        }

        public ReceiptModel(
            Guid receiptId,
            Guid invoiceId,
            Guid createdByAccountId,
            EReceiptPaymentMethod paymentMethod,
            decimal amount,
            DateTime paymentDate,
            string? transactionReference,
            string? gatewayTransactionNumber,
            string? transactionInfo,
            string? bankName)
        {
            ReceiptId = receiptId;
            ReceiptInvoiceId = invoiceId;
            ReceiptCreatedByAccountId = createdByAccountId;
            ReceiptPaymentMethod = paymentMethod;
            ReceiptAmount = amount;
            ReceiptPaymentDate = paymentDate;
            ReceiptTransactionReference = transactionReference;
            ReceiptGatewayTransactionNumber = gatewayTransactionNumber;
            ReceiptTransactionInfo = transactionInfo;
            ReceiptBankName = bankName;
        }

        public ReceiptModel() {}

        public Guid ReceiptId { get; init; }

        public Guid ReceiptInvoiceId { get; private set; }

        public Guid ReceiptCreatedByAccountId { get; private set; }

        public EReceiptPaymentMethod ReceiptPaymentMethod { get; private set; } = EReceiptPaymentMethod.Cash;

        public decimal ReceiptAmount { get; private set; }

        public DateTime ReceiptPaymentDate { get; private set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ReceiptTransactionReference { get; private set; }

        [MaxLength(100)]
        public string? ReceiptGatewayTransactionNumber { get; private set; }

        [MaxLength(250)]
        public string? ReceiptTransactionInfo { get; private set; }

        [MaxLength(100)]
        public string? ReceiptBankName { get; private set; }

        public DateTime ReceiptCreatedAt { get; init; } = DateTime.Now;

        public DateTime ReceiptUpdatedAt { get; private set; } = DateTime.Now;

        #region Setter

        public void SetInvoiceId(Guid invoiceId)
        {
            ReceiptInvoiceId = invoiceId;
        }

        public void SetCreatedByAccountId(Guid createdByAccountId)
        {
            ReceiptCreatedByAccountId = createdByAccountId;
        }

        public void SetPaymentMethod(EReceiptPaymentMethod paymentMethod)
        {
            ReceiptPaymentMethod = paymentMethod;
        }

        public void SetAmount(decimal amount)
        {
            ReceiptAmount = amount;
        }

        public void SetPaymentDate(DateTime paymentDate)
        {
            ReceiptPaymentDate = paymentDate;
        }

        public void SetTransactionReference(string? transactionReference)
        {
            ReceiptTransactionReference = transactionReference;
        }

        public void SetGatewayTransactionNumber(string? gatewayTransactionNumber)
        {
            ReceiptGatewayTransactionNumber = gatewayTransactionNumber;
        }

        public void SetTransactionInfo(string? transactionInfo)
        {
            ReceiptTransactionInfo = transactionInfo;
        }

        public void SetBankName(string? bankName)
        {
            ReceiptBankName = bankName;
        }

        #endregion
    }
}
