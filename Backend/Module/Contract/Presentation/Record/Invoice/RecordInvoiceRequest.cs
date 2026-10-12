using System.ComponentModel.DataAnnotations;
using Contract.Utils.Enum;

namespace Contract.Presentation.Record.Invoice
{
    public sealed record RecordInvoiceDetailRequest(
        [Required] Guid PremiseId,
        [Range(0, int.MaxValue)] int? DetailId = null,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal RentalPrice = 0,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal ElectricityFee = 0,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal WaterFee = 0,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal GarbageFee = 0);

    public sealed record RecordInvoiceRequest(
        [Required] Guid InvoiceId,
        [Required] Guid ContractId,
        [Required] DateTime PaymentDate,
        [Required] DateTime DueDate,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? TotalAmount,
        EInvoiceStatus Status,
        [Required, MinLength(1)] IReadOnlyCollection<RecordInvoiceDetailRequest> Details);

    public sealed record RecordInvoiceUpdateRequest(
        [Required] Guid ContractId,
        [Required] DateTime PaymentDate,
        [Required] DateTime DueDate,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? TotalAmount,
        EInvoiceStatus Status,
        IReadOnlyCollection<RecordInvoiceDetailRequest>? Details = null);
}
