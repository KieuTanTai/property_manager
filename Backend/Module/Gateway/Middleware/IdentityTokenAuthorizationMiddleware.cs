using System.Net;
using System.Text.Json;
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

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<IdentityTokenAuthorizationMiddleware> _logger = logger;

        private readonly RequestDelegate _next = next;

        private readonly IIdentityTokenAuthorizationService _tokenAuthorizationService = tokenAuthorizationService;

        public async Task InvokeAsync(HttpContext context)
        {
            await ProcessRequestAsync(context);
        }

        private async Task ProcessRequestAsync(HttpContext context)
        {
            if (IsPublicEndpoint(context.Request.Method, context.Request.Path))
            {
                await _next(context);
                return;
            }

            var authorizationHeader = context.Request.Headers.Authorization.ToString();
            string accessToken;
            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                accessToken = authorizationHeader["Bearer ".Length..].Trim();
            }
            else
            {
                accessToken = context.Request.Query["access_token"].ToString().Trim();
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Request rejected because a bearer access token is missing.");
                await RejectAsync(context, StatusCodes.Status401Unauthorized, "Bearer access token is required.");
                return;
            }
            var statusCode = await _tokenAuthorizationService.AuthorizeAsync(accessToken, context.RequestAborted);
            if (statusCode is null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Request rejected because the identity module could not be reached.");
                await RejectAsync(context, StatusCodes.Status503ServiceUnavailable, "Identity module is unavailable.");
                return;
            }

            if (statusCode != HttpStatusCode.NoContent)
            {
                if (statusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogLayerError(_logPool, Module, Layer,
                        new InvalidOperationException("Identity rejected the Gateway API key."),
                        "Request rejected because Gateway and Identity internal API keys do not match.");
                    await RejectAsync(context, StatusCodes.Status503ServiceUnavailable,
                        "Gateway and Identity internal API keys do not match.");
                    return;
                }

                _logger.LogLayerWarning(_logPool, Module, Layer, $"Request rejected because the access token is invalid. Identity module returned {statusCode}.");
                await RejectAsync(context, StatusCodes.Status401Unauthorized, "Access token is invalid.");
                return;
            }

            var accountId = TryGetAccountId(accessToken);
            var gatewayApiKey = context.RequestServices
                .GetRequiredService<IConfiguration>()["INTERNAL_GATEWAY_API_KEY"];
            if (!Guid.TryParse(accountId, out _) || string.IsNullOrWhiteSpace(gatewayApiKey))
            {
                await RejectAsync(context, StatusCodes.Status401Unauthorized,
                    "Authenticated account identity could not be established.");
                return;
            }

            context.Request.Headers["X-Authenticated-Account-Id"] = accountId;
            context.Request.Headers["X-Internal-Gateway-Key"] = gatewayApiKey;
            await _next(context);
        }

        private static string? TryGetAccountId(string accessToken)
        {
            var parts = accessToken.Split('.');
            if (parts.Length != 3)
            {
                return null;
            }

            var payload = Convert.FromBase64String(PadBase64Url(parts[1]));
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            if (root.TryGetProperty("sub", out var sub))
            {
                return sub.GetString();
            }
            if (root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                    out var nameIdentifier))
            {
                return nameIdentifier.GetString();
            }
            return null;
        }

        private static string PadBase64Url(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            return (padded.Length % 4) switch
            {
                2 => padded + "==",
                3 => padded + "=",
                _ => padded
            };
        }

        private static bool IsPublicEndpoint(string method, PathString path)
        {
            if (path.Equals("/api/account/login", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/api/account/register", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/api/account/session-instance", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!HttpMethods.IsGet(method))
            {
                return false;
            }

            return path.Equals("/api/contact", StringComparison.OrdinalIgnoreCase)
                   || path.StartsWithSegments("/api/premise", StringComparison.OrdinalIgnoreCase)
                   || path.StartsWithSegments("/api/location", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task RejectAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new { error = message }, context.RequestAborted);
        }
    }
}