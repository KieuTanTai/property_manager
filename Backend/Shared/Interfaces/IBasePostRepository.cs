namespace Shared.Interfaces
{
    public interface IBasePostRepository<in T> where T : class
    {
        // 
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        void UpdateRange(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
    }
}