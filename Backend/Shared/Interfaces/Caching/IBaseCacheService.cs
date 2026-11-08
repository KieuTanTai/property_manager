using Microsoft.Extensions.Caching.Hybrid;

namespace Shared.Interfaces.Caching
{
    public interface IBaseCacheService<TModel>
    {
        ValueTask<TModel> GetOrCreateAsync(
            string key,
            Func<CancellationToken, Task<TModel>> loader,
            HybridCacheEntryOptions? options = null,
            CancellationToken cancellationToken = default);

        ValueTask<TModel[]> GetOrCreateManyAsync(
            string key,
            Func<CancellationToken, Task<IReadOnlyList<TModel>>> loader,
            HybridCacheEntryOptions? options = null,
            CancellationToken cancellationToken = default);

        ValueTask InvalidateAsync(string key, CancellationToken cancellationToken = default);
    }
}
