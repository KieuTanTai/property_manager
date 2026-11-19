using Payment.Utils.Enum;

namespace Payment.Models.Payment
{
    /// <summary>
    /// The gateway outcome of an IPN, ready to be written in one conditional update.
    /// <see cref="Status"/> is always terminal: in-flight callbacks are not settled.
    /// </summary>
    public sealed record PaymentSettlementModel(
        EPaymentStatus Status,
        string? GatewayTransactionNumber,
        string? BankCode,
        string? BankTranNo,
        string? ResponseCode,
        string? TransactionStatusCode,
        string? CardType,
        string? OrderInfo,
        DateTime? PayDate);
}
