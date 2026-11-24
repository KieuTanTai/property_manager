using Premise.Models.Premise;
using Shared.Interfaces;

namespace Premise.Interfaces.IRepository.Premise
{
    public interface IRentedPremiseRepository : IBaseAssociativeRepository<RentedPremiseModel, Guid>
    {
        Task<IReadOnlyList<RentedPremiseModel>> GetByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RentedPremiseModel>> GetByContractIdAsync(
            Guid contractId,
            CancellationToken cancellationToken = default);
    }
}
