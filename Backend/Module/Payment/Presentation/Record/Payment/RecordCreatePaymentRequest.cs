using System.ComponentModel.DataAnnotations;

namespace Payment.Presentation.Record.Payment
{
    public sealed record RecordCreatePaymentRequest(
        Guid InvoiceId,
        Guid AccountId,
        [Range(typeof(decimal), "5000", "1000000000")]
        decimal Amount,
        [StringLength(255)] string? Description = null,
        string? BankCode = null,
        string? Language = null);
}
