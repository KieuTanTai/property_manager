using Gateway.Services;
using Shared.Caching;
using Shared.Logging;

namespace Gateway.Infrastructure.DIContainer
{
    public static class CollectionGateway
    {
        public static IServiceCollection AddGatewayCollection(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            #region CACHE

            services.AddHybridCache();
            var redisConnection = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
            }
            services.AddScoped(typeof(IBaseCacheService<>), typeof(BaseCacheService<>));

            #endregion

            services.AddSingleton<ILogPool, LogPool>();
            services.AddScoped<ModuleProxyService>();

            return services;
        }
    }
}
