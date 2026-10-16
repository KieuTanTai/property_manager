using Premise.Models.Premise;

namespace Premise.Interfaces.IApplication;

public interface ILocationApplication
{
    Task<IReadOnlyList<LocationModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<LocationModel> GetByIdAsync(Guid locationId, CancellationToken cancellationToken = default);

    Task<LocationModel> AddAsync(string locationAddress, CancellationToken cancellationToken = default);

    Task<LocationModel> UpdateAsync(
        Guid locationId,
        string locationAddress,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid locationId, CancellationToken cancellationToken = default);
}
