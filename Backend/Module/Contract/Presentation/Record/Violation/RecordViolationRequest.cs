using System.ComponentModel.DataAnnotations;

namespace Contract.Presentation.Record.Violation
{
    public sealed record RecordViolationRequest(
        [Required] Guid ContractId,
        [Required, StringLength(150, MinimumLength = 1)] string ViolationContent,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? ViolationPenaltyAmount = null,
        DateTime? ViolationDate = null,
        DateTime? ViolationDueDate = null);

    public sealed record RecordViolationUpdateRequest(
        [Required] Guid ContractId,
        [Required, StringLength(150, MinimumLength = 1)] string ViolationContent,
        [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? ViolationPenaltyAmount,
        [Required] DateTime ViolationDate,
        [Required] DateTime ViolationDueDate,
        bool ViolationIsResolved);
}
