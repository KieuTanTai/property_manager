using Premise.Models.Premise;
using Shared.Interfaces;

namespace Premise.Interfaces.Premise
{
    public interface IPremiseMediaRepository :
        IBaseReadRepository<PremiseMediaModel, int>,
        IBasePostRepository<PremiseMediaModel>
    {
        Task<IReadOnlyList<PremiseMediaModel>> GetByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseMediaModel>> GetByPremiseIdsAsync(
            IEnumerable<Guid> premiseIds,
            CancellationToken cancellationToken = default);
    }
}
