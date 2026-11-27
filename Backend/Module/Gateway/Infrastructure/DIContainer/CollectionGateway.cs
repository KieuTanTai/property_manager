using Gateway.Interfaces.Authorization;
using Gateway.Services;
using Shared.DIContainer;
using Shared.Logging;
using System.Threading.RateLimiting;

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

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    // Read-only API calls are intentionally not rate limited here. They are
                    // canceled by the client when a page reloads or unmounts. Limit writes
                    // and credential endpoints together by module API group.
                    if (HttpMethods.IsGet(context.Request.Method))
                    {
                        return RateLimitPartition.GetNoLimiter("read-only");
                    }

                    // Use the TCP peer address; forwarded and fingerprint headers are
                    // client-controlled unless a trusted proxy integration is configured.
                    var clientKey = context.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";
                    var apiGroup = GetApiGroup(context.Request.Path);
                    var partitionKey = $"{clientKey}:{apiGroup}";
                    var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 10);
                    var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = Math.Max(1, permitLimit),
                            Window = TimeSpan.FromSeconds(Math.Max(1, windowSeconds)),
                            QueueLimit = 0,
                            AutoReplenishment = true,
                        });
                });
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/problem+json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        type = "about:blank",
                        title = "Too many requests",
                        status = StatusCodes.Status429TooManyRequests,
                        detail = "Rate limit exceeded. Try again after the configured window resets.",
                    }, cancellationToken);
                };
            });

            #endregion

            #region PROXY

            services.AddScoped<ModuleProxyService>();

            #endregion

            #region AUTHORIZATION

            services.AddSingleton<IIdentityTokenAuthorizationService, IdentityTokenAuthorizationService>();

            #endregion

            return services;
        }

        private static string GetApiGroup(PathString path)
        {
            var firstSegment = path.Value?
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .SkipWhile(segment => !segment.Equals("api", StringComparison.OrdinalIgnoreCase))
                .Skip(1)
                .FirstOrDefault();

            return firstSegment?.ToLowerInvariant() switch
            {
                "account" or "authorization" or "profile" => "identity",
                "contract" or "regulation" or "invoice" or "receipt" or "contact" or "violation" => "contract",
                "premise" or "location" or "product" or "businesstype" or "business-type" => "premise",
                "ticket" or "ticketmedia" or "ticket-medias" or "notifications" or "notification-recipients" => "ticket-notification",
                _ => "other-api",
            };
        }
    }
}
