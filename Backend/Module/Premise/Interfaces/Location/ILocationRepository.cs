using Premise.Models.Premise;
using Shared.Interfaces;

namespace Premise.Interfaces.Location
{
    public interface ILocationRepository : IBaseReadRepository<LocationModel, Guid>, IBasePostRepository<LocationModel> {}
}