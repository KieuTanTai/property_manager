namespace Gateway.Middleware;

public static class ProxyMappingMiddleware
{
    public static IServiceCollection AddProxyMapping(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddHttpClient("ModuleProxy", client => client.Timeout = Timeout.InfiniteTimeSpan)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = environment.IsDevelopment()
                    ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    : null,
            });

        return services;
    }
}
