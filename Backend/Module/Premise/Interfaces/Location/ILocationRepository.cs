using Premise.Models.Premise;
using Shared.Interfaces;

namespace Premise.Interfaces.Location
{
    public interface ILocationRepository : IBaseReadRepository<LocationModel, Guid>, IBasePostRepository<LocationModel>
    {
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
