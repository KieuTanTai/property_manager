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

            services.AddSharedHybridCache(configuration);

            #endregion

            services.AddSingleton<ILogPool, LogPool>();
            services.AddScoped<ModuleProxyService>();
            services.AddMemoryCache();
            services.AddSingleton<IIdentitySigningKeyResolver, IdentitySigningKeyResolver>();

            return services;
        }
    }
}
