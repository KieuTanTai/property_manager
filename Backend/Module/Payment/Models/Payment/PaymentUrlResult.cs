namespace Payment.Models.Payment
{
    /// <summary>
    /// Result of a payment URL creation: the persisted transaction plus the URL the caller
    /// must redirect the payer to.
    /// </summary>
    public sealed record PaymentUrlResult(PaymentTransactionModel Transaction, string PaymentUrl);
}
