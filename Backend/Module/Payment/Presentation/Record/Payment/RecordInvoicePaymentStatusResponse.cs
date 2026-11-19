using Payment.Utils.Enum;

namespace Payment.Presentation.Record.Payment
{
    /// <summary>
    /// Minimal, stable contract for other services that only need to know whether an invoice
    /// has been paid. <see cref="IsPaid"/> is true as soon as one transaction succeeded.
    /// </summary>
    public sealed record RecordInvoicePaymentStatusResponse(
        Guid InvoiceId,
        bool IsPaid,
        Guid? PaymentTransactionId,
        string? TxnRef,
        EPaymentStatus? Status,
        decimal? Amount,
        DateTime? PayDate);
}
