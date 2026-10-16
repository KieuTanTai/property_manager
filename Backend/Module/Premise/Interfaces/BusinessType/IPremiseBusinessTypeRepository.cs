using Premise.Models.Business;

namespace Premise.Interfaces.BusinessType
{
    public interface IPremiseBusinessTypeRepository
    {
        Task<IReadOnlyList<PremiseBusinessTypeModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseBusinessTypeModel>> GetByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseBusinessTypeModel>> GetByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseBusinessTypeModel>> GetByPremiseIdsAsync(
            IEnumerable<Guid> premiseIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseBusinessTypeModel>> GetByBusinessTypeIdsAsync(
            IEnumerable<Guid> businessTypeIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseBusinessTypeModel>> GetTrackedByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseBusinessTypeModel>> GetTrackedByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default);

        Task<PremiseBusinessTypeModel?> GetByIdsAsync(
            Guid premiseId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Guid premiseId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            PremiseBusinessTypeModel entity,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<PremiseBusinessTypeModel> entities,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid premiseId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default);
    }
}
