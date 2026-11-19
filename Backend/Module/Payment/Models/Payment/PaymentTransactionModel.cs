using System.ComponentModel.DataAnnotations;
using Payment.Utils.Enum;

namespace Payment.Models.Payment
{
    public class PaymentTransactionModel
    {
        public PaymentTransactionModel(Guid invoiceId, Guid accountId, decimal amount,
            string? orderInfo, DateTime expiresAt)
        {
            PaymentInvoiceId = invoiceId;
            PaymentAccountId = accountId;
            PaymentAmount = amount;
            PaymentOrderInfo = orderInfo;
            PaymentExpiresAt = expiresAt;
        }

        public PaymentTransactionModel(
            Guid paymentTransactionId,
            Guid invoiceId,
            Guid accountId,
            decimal amount,
            EPaymentStatus status,
            string currency,
            string? txnRef,
            string? gatewayTransactionNumber,
            string? bankCode,
            string? bankTranNo,
            string? responseCode,
            string? transactionStatusCode,
            string? cardType,
            string? orderInfo,
            DateTime? payDate,
            DateTime expiresAt,
            DateTime createdAt,
            DateTime updatedAt)
        {
            PaymentTransactionId = paymentTransactionId;
            PaymentInvoiceId = invoiceId;
            PaymentAccountId = accountId;
            PaymentAmount = amount;
            PaymentStatus = status;
            PaymentCurrency = currency;
            PaymentTxnRef = txnRef;
            PaymentGatewayTransactionNumber = gatewayTransactionNumber;
            PaymentBankCode = bankCode;
            PaymentBankTranNo = bankTranNo;
            PaymentResponseCode = responseCode;
            PaymentTransactionStatusCode = transactionStatusCode;
            PaymentCardType = cardType;
            PaymentOrderInfo = orderInfo;
            PaymentPayDate = payDate;
            PaymentExpiresAt = expiresAt;
            PaymentCreatedAt = createdAt;
            PaymentUpdatedAt = updatedAt;
        }

        public PaymentTransactionModel() {}

        public Guid PaymentTransactionId { get; init; }

        public Guid PaymentInvoiceId { get; private set; }

        public Guid PaymentAccountId { get; private set; }

        public decimal PaymentAmount { get; private set; }

        public EPaymentStatus PaymentStatus { get; private set; } = EPaymentStatus.Pending;

        [MaxLength(3)] public string PaymentCurrency { get; private set; } = "VND";

        [MaxLength(50)] public string? PaymentTxnRef { get; private set; }

        [MaxLength(100)] public string? PaymentGatewayTransactionNumber { get; private set; }

        [MaxLength(20)] public string? PaymentBankCode { get; private set; }

        [MaxLength(100)] public string? PaymentBankTranNo { get; private set; }

        [MaxLength(10)] public string? PaymentResponseCode { get; private set; }

        [MaxLength(10)] public string? PaymentTransactionStatusCode { get; private set; }

        [MaxLength(50)] public string? PaymentCardType { get; private set; }

        [MaxLength(255)] public string? PaymentOrderInfo { get; private set; }

        public DateTime? PaymentPayDate { get; private set; }

        public DateTime PaymentExpiresAt { get; private set; } = DateTime.UtcNow.AddMinutes(15);

        public DateTime PaymentCreatedAt { get; init; } = DateTime.UtcNow;

        public DateTime PaymentUpdatedAt { get; private set; } = DateTime.UtcNow;

        #region Setter

        public void SetInvoiceId(Guid invoiceId)
        {
            PaymentInvoiceId = invoiceId;
        }

        public void SetAccountId(Guid accountId)
        {
            PaymentAccountId = accountId;
        }

        public void SetAmount(decimal amount)
        {
            PaymentAmount = amount;
        }

        public void SetStatus(EPaymentStatus status)
        {
            PaymentStatus = status;
        }

        public void SetTxnRef(string? txnRef)
        {
            PaymentTxnRef = txnRef;
        }

        public void SetOrderInfo(string? orderInfo)
        {
            PaymentOrderInfo = orderInfo;
        }

        public void SetExpiresAt(DateTime expiresAt)
        {
            PaymentExpiresAt = expiresAt;
        }

        public void SetGatewayResult(
            string? gatewayTransactionNumber,
            string? bankCode,
            string? bankTranNo,
            string? responseCode,
            string? transactionStatusCode,
            string? cardType,
            string? orderInfo,
            DateTime? payDate)
        {
            PaymentGatewayTransactionNumber = gatewayTransactionNumber;
            PaymentBankCode = bankCode;
            PaymentBankTranNo = bankTranNo;
            PaymentResponseCode = responseCode;
            PaymentTransactionStatusCode = transactionStatusCode;
            PaymentCardType = cardType;
            PaymentOrderInfo = orderInfo ?? PaymentOrderInfo;
            PaymentPayDate = payDate;
        }

        #endregion

        #region Domain helper

        public bool IsPending => PaymentStatus == EPaymentStatus.Pending;

        public bool IsSucceeded => PaymentStatus == EPaymentStatus.Succeeded;

        public bool IsNeedsReview => PaymentStatus == EPaymentStatus.NeedsReview;

        public bool IsFinalized => PaymentStatus is EPaymentStatus.Succeeded
            or EPaymentStatus.Failed or EPaymentStatus.Expired or EPaymentStatus.NeedsReview;

        #endregion
    }
}
