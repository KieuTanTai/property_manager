using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Caching;
using Shared.Interfaces.Caching;

namespace Shared.DIContainer
{
    public static class HybridCacheCollection
    {
        public static IServiceCollection AddSharedHybridCache(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHybridCache();

            var redisConnection = configuration["REDIS_CACHE"];
            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                throw new InvalidOperationException("Environment variable 'REDIS_CACHE' was not found.");
            }

            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);

            services.TryAddSingleton(typeof(IBaseCacheService<>), typeof(BaseCacheService<>));
            return services;
        }
    }
}