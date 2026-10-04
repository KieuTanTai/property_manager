namespace Shared.Interfaces
{
    public interface IBaseAssociativeRepository<T, in TTypeId> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetByFirstForeignIdAsync(TTypeId firstForeignId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetBySecondForeignIdAsync(TTypeId secondForeignId,
            CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(TTypeId firstForeignId, TTypeId secondForeignId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(TTypeId firstForeignId, TTypeId secondForeignId,
            CancellationToken cancellationToken = default);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(List<T> entities, CancellationToken cancellationToken = default);

        Task DeleteByFirstForeignIdAsync(TTypeId firstForeignId,
            CancellationToken cancellationToken = default);

        Task DeleteBySecondForeignIdAsync(TTypeId secondForeignId,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(TTypeId firstForeignId, TTypeId secondForeignId,
            CancellationToken cancellationToken = default);
    }
}
