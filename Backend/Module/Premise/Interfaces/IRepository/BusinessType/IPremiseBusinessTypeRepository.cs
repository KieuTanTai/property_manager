using Premise.Models.Business;
using Shared.Interfaces;

namespace Premise.Interfaces.IRepository.BusinessType
{
    public interface IPremiseBusinessTypeRepository : IBaseAssociativeRepository<PremiseBusinessTypeModel, Guid>
    {
        Task DeleteAsync(Guid premiseId, Guid businessTypeId, CancellationToken cancellationToken = default);

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
    }
}