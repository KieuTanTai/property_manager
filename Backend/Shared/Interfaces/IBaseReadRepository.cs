namespace Shared.Interfaces
{
    public interface IBaseReadRepository<T, in TId>
        where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
        
        Task<T?> GetTrackedByIdAsync(TId id, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
    }
}