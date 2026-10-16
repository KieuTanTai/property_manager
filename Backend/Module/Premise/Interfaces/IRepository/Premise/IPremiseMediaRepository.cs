using Premise.Models.Premise;
using Shared.Interfaces;

namespace Premise.Interfaces.IRepository.Premise
{
    public interface IPremiseMediaRepository :
        IBaseReadRepository<PremiseMediaModel, int>,
        IBaseUpsertRepository<PremiseMediaModel>
    {
        Task<IReadOnlyList<PremiseMediaModel>> GetByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseMediaModel>> GetByPremiseIdsAsync(
            IEnumerable<Guid> premiseIds,
            CancellationToken cancellationToken = default);
    }
}
