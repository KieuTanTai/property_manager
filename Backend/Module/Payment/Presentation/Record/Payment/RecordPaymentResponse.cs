using Payment.Utils.Enum;

namespace Payment.Presentation.Record.Payment
{
    public sealed record RecordPaymentResponse(
        Guid PaymentTransactionId,
        Guid InvoiceId,
        Guid AccountId,
        decimal Amount,
        string Currency,
        EPaymentStatus Status,
        string? TxnRef,
        string? PaymentUrl,
        string? GatewayTransactionNumber,
        string? BankCode,
        string? BankTranNo,
        string? ResponseCode,
        string? TransactionStatusCode,
        string? CardType,
        string? OrderInfo,
        DateTime? PayDate,
        DateTime ExpiresAt,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
