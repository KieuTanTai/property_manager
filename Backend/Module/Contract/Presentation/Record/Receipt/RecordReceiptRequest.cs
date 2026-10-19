using System.ComponentModel.DataAnnotations;
using Shared.Enum;

namespace Contract.Presentation.Record.Receipt
{
    public sealed record RecordReceiptRequest(
        [Required] Guid InvoiceId,
        [Required] Guid CreatedByAccountId,
        EReceiptPaymentMethod PaymentMethod,
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Amount,
        DateTime PaymentDate,
        [MaxLength(100)] string? TransactionReference = null,
        [MaxLength(100)] string? GatewayTransactionNumber = null,
        [MaxLength(250)] string? TransactionInfo = null,
        [MaxLength(100)] string? BankName = null);

    public sealed record RecordReceiptUpdateRequest(
        [Required] DateTime PaymentDate,
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Amount,
        [MaxLength(250)] string? TransactionInfo = null);
}