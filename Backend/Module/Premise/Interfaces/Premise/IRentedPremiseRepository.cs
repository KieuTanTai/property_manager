using Premise.Models.Premise;

namespace Premise.Interfaces.Premise
{
    public interface IRentedPremiseRepository
    {
        Task<IReadOnlyList<RentedPremiseModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RentedPremiseModel>> GetByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RentedPremiseModel>> GetByContractIdAsync(
            Guid contractId,
            CancellationToken cancellationToken = default);

        Task<RentedPremiseModel?> GetByIdsAsync(
            Guid contractId,
            Guid premiseId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Guid contractId,
            Guid premiseId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            RentedPremiseModel entity,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<RentedPremiseModel> entities,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid contractId,
            Guid premiseId,
            CancellationToken cancellationToken = default);
    }
}
