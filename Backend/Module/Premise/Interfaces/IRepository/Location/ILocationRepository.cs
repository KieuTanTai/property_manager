using Premise.Models.Premise;
using Shared.Interfaces;

namespace Premise.Interfaces.IRepository.Location
{
    public interface ILocationRepository : IBaseReadRepository<LocationModel, Guid>, IBaseUpsertRepository<LocationModel>
    {
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task<long> CountTotalAsync(CancellationToken cancellationToken = default);
    }
}
