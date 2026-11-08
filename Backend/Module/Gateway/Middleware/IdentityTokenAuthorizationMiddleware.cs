using System.Net;
using Gateway.Interfaces.Authorization;
using Shared.Logging;

namespace Gateway.Middleware
{
    public sealed class IdentityTokenAuthorizationMiddleware(
        RequestDelegate next,
        IIdentityTokenAuthorizationService tokenAuthorizationService,
        ILogger<IdentityTokenAuthorizationMiddleware> logger,
        ILogPool logPool)
    {
        private const string Module = "Gateway";
        private const string Layer = "Middleware";

        private readonly RequestDelegate _next = next;
        private readonly IIdentityTokenAuthorizationService _tokenAuthorizationService = tokenAuthorizationService;
        private readonly ILogger<IdentityTokenAuthorizationMiddleware> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        public async Task InvokeAsync(HttpContext context)
        {
            await ProcessRequestAsync(context);
        }

        private async Task ProcessRequestAsync(HttpContext context)
        {
            if (IsPublicEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var authorizationHeader = context.Request.Headers.Authorization.ToString();
            if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Request rejected because a bearer access token is missing.");
                await RejectAsync(context, StatusCodes.Status401Unauthorized, "Bearer access token is required.");
                return;
            }

            var accessToken = authorizationHeader["Bearer ".Length..].Trim();
            var statusCode = await _tokenAuthorizationService.AuthorizeAsync(accessToken, context.RequestAborted);
            if (statusCode is null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Request rejected because the identity module could not be reached.");
                await RejectAsync(context, StatusCodes.Status503ServiceUnavailable, "Identity module is unavailable.");
                return;
            }

            if (statusCode != HttpStatusCode.NoContent)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, $"Request rejected because the access token is invalid. Identity module returned {statusCode}.");
                await RejectAsync(context, StatusCodes.Status401Unauthorized, "Access token is invalid.");
                return;
            }

            await _next(context);
        }

        private static bool IsPublicEndpoint(PathString path) =>
            path.Equals("/api/account/login", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/account/register", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/account/session-instance", StringComparison.OrdinalIgnoreCase);

        private static async Task RejectAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new { error = message }, context.RequestAborted);
        }
    }
}
