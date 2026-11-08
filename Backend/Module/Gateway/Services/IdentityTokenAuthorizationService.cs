using System.Net;
using Gateway.Interfaces.Authorization;
using Shared.Persistence.Record.Auth;
using Shared.Logging;

namespace Gateway.Services
{
    public sealed class IdentityTokenAuthorizationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<IdentityTokenAuthorizationService> logger,
        ILogPool logPool) : IIdentityTokenAuthorizationService
    {
        private const string Module = "Gateway";
        private const string Layer = "Services";

        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogPool _logPool = logPool;
        private readonly ILogger<IdentityTokenAuthorizationService> _logger = logger;

        public async Task<HttpStatusCode?> AuthorizeAsync(string accessToken, CancellationToken cancellationToken)
        {
            var identityUrl = _configuration["ModuleUrls:Identity"];
            var gatewayApiKey = _configuration["INTERNAL_GATEWAY_API_KEY"];
            if (!Uri.TryCreate(identityUrl, UriKind.Absolute, out var baseUri) ||
                !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"Identity token authorization requires a valid HTTPS ModuleUrls:Identity value. Configured value: '{identityUrl ?? "<missing>"}'.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(gatewayApiKey))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Identity token authorization requires INTERNAL_GATEWAY_API_KEY to be configured.");
                return null;
            }

            var endpoint = new Uri(baseUri, "internal/auth/authorize");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = JsonContent.Create(new RecordInternalTokenAuthorizationRequest(accessToken));
            request.Headers.Add("X-Internal-Gateway-Key", gatewayApiKey);

            try
            {
                using var response = await _httpClientFactory.CreateClient("ModuleProxy")
                    .SendAsync(request, cancellationToken);
                return response.StatusCode;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Identity token authorization endpoint could not be reached.");
                return null;
            }
        }
    }
}
