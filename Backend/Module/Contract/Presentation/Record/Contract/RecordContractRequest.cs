using System.ComponentModel.DataAnnotations;
using ContractStatus=Contract.Utils.Enum.EContractStatus;

namespace Contract.Presentation.Record.Contract
{
    public sealed record RecordContractRequest(
        [Required] Guid AccountId,
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Deposit,
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal RentalPrice,
        [Required] DateTime PremiseReturnDate,
        IReadOnlyCollection<Guid>? RegulationIds = null);

    public sealed record RecordContractUpdateRequest(
        [Required] Guid AccountId,
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Deposit,
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal RentalPrice,
        [Required] DateTime PremiseReturnDate,
        ContractStatus Status,
        DateTime? TerminationDate = null,
        IReadOnlyCollection<Guid>? RegulationIds = null);
}