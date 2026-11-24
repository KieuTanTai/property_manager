namespace Shared.Interfaces
{
    public interface IBaseAssociativeRepository<T, in TTypeId> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<T?> GetByIdsAsync(TTypeId firstForeignId, TTypeId secondForeignId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(TTypeId firstForeignId, TTypeId secondForeignId,
            CancellationToken cancellationToken = default);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(List<T> entities, CancellationToken cancellationToken = default);

    }
}
