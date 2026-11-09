using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Identity.Infrastructure.Credentials.Models;
using Identity.Interfaces.Credentials;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Logging;

namespace Identity.Infrastructure.Credentials
{
    public sealed class JwtAccessTokenValidator(
        IOptions<JwtOptions> options,
        IRsaSigningKeyProvider signingKeyProvider,
        ILogger<JwtAccessTokenValidator> logger,
        ILogPool logPool) : IJwtAccessTokenValidator
    {
        private const string Module = "Identity";
        private const string Layer = "Infrastructure/Credentials";

        private readonly JwtOptions _options = options.Value;
        private readonly IRsaSigningKeyProvider _signingKeyProvider = signingKeyProvider;
        private readonly ILogger<JwtAccessTokenValidator> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        public bool IsValid(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return false;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(accessToken);
                if (string.IsNullOrWhiteSpace(token.Header.Kid))
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer,
                        "Access token validation failed: JWT has no key identifier (kid).");
                    return false;
                }

                var publicKeyPem = _signingKeyProvider.GetPublicKeyPem(token.Header.Kid);
                if (publicKeyPem is null)
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer,
                        "Access token validation failed: no active verification key was found.");
                    return false;
                }

                using var rsa = RSA.Create();
                rsa.ImportFromPem(publicKeyPem);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _options.Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa)
                    {
                        KeyId = token.Header.Kid,
                        // This temporary verification-only RSA is imported from the rotation service's public PEM.
                        // The rotation service's signing RSA remains alive for its configured key lifetime.
                        // Do not cache a signature provider that could retain and reuse this temporary instance.
                        CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
                    },
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                handler.ValidateToken(accessToken, validationParameters, out _);
                return true;
            }
            catch (Exception exception) when (exception is SecurityTokenException or ArgumentException or FormatException or CryptographicException)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    $"Access token validation failed during JWT verification ({exception.GetType().Name}).");
                return false;
            }
        }
    }
}
