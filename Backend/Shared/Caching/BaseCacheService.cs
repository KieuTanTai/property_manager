using Microsoft.Extensions.Caching.Hybrid;

namespace Shared.Caching
{
    public sealed class BaseCacheService<TModel>(HybridCache cache) : IBaseCacheService<TModel>
    {
        public ValueTask<TModel> GetOrCreateAsync(
            string key,
            Func<CancellationToken, Task<TModel>> loader,
            HybridCacheEntryOptions? options = null,
            CancellationToken cancellationToken = default) =>
            cache.GetOrCreateAsync(
                key,
                async token => await loader(token),
                options,
                cancellationToken: cancellationToken);

        public ValueTask<TModel[]> GetOrCreateManyAsync(
            string key,
            Func<CancellationToken, Task<IReadOnlyList<TModel>>> loader,
            HybridCacheEntryOptions? options = null,
            CancellationToken cancellationToken = default) =>
            cache.GetOrCreateAsync(
                key,
                async token => (await loader(token)).ToArray(),
                options,
                cancellationToken: cancellationToken);

        public ValueTask InvalidateAsync(string key, CancellationToken cancellationToken = default) =>
            cache.RemoveAsync(key, cancellationToken);
    }
}
