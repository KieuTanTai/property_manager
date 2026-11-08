using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Gateway.Presentation.Record.Jwt;
using Microsoft.Extensions.Caching.Memory;

namespace Gateway.Services
{
    public sealed class IdentitySigningKeyResolver(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        ILogger<IdentitySigningKeyResolver> logger) : IIdentitySigningKeyResolver
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly ILogger<IdentitySigningKeyResolver> _logger = logger;

        public async Task<string?> GetPublicKeyPemAsync(string keyId, CancellationToken cancellationToken)
        {
            var cacheKey = $"identity-jwt-public-key:{keyId}";
            if (_memoryCache.TryGetValue(cacheKey, out string? cachedKey))
            {
                return cachedKey;
            }

            var identityUrl = _configuration["ModuleUrls:Identity"];
            var gatewayApiKey = _configuration["InternalGateway:ApiKey"];
            if (!Uri.TryCreate(identityUrl, UriKind.Absolute, out var baseUri) ||
                !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(gatewayApiKey))
            {
                _logger.LogError("Identity signing-key lookup is not configured with an HTTPS URL and gateway API key.");
                return null;
            }

            var escapedKeyId = Uri.EscapeDataString(keyId);
            var keyUri = new Uri(baseUri, $"internal/auth/keys/{escapedKeyId}");
            using var request = new HttpRequestMessage(HttpMethod.Get, keyUri);
            request.Headers.Add("X-Internal-Gateway-Key", gatewayApiKey);

            try
            {
                using var response = await _httpClientFactory.CreateClient("ModuleProxy")
                    .SendAsync(request, cancellationToken);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
                var keyResponse = await response.Content.ReadFromJsonAsync<RecordPublicSigningKeyResponse>(
                    cancellationToken: cancellationToken);
                if (keyResponse is null || !string.Equals(keyResponse.Kid, keyId, StringComparison.Ordinal))
                {
                    return null;
                }

                _memoryCache.Set(cacheKey, keyResponse.PublicKeyPem, TimeSpan.FromMinutes(10));
                return keyResponse.PublicKeyPem;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "Could not resolve JWT signing key {KeyId} from Identity.", keyId);
                return null;
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Identity returned an invalid JWT public-key response for {KeyId}.", keyId);
                return null;
            }
        }
    }
}
