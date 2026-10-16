using Shared.Interfaces;

namespace Identity.Interfaces.IRepository
{
    public interface IBaseAuthorizationRepository<T, in TEnumType, in TId> : IBaseReadRepository<T, TId>, IBaseUpsertRepository<T>
        where T : class
        where TEnumType : Enum
    {
        Task<T?> GetByCodeAsync(TEnumType code, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetByActiveStatus(bool isActive, CancellationToken cancellationToken = default);
    }
}