using Gateway.Interfaces.Authorization;
using Gateway.Services;
using Shared.Caching;
using Shared.DIContainer;
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

            #region LOGGING

            services.AddSingleton<ILogPool, LogPool>();

            #endregion

            #region PROXY

            services.AddScoped<ModuleProxyService>();

            #endregion

            #region AUTHORIZATION
            services.AddSingleton<IIdentityTokenAuthorizationService, IdentityTokenAuthorizationService>();

            #endregion

            return services;
        }
    }
}
