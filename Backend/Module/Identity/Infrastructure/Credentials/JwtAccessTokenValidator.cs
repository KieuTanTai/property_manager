using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Identity.Infrastructure.Credentials.Models;
using Identity.Interfaces.Credentials;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Credentials
{
    public sealed class JwtAccessTokenValidator(
        IOptions<JwtOptions> options,
        IRsaSigningKeyProvider signingKeyProvider) : IJwtAccessTokenValidator
    {
        private readonly JwtOptions _options = options.Value;
        private readonly IRsaSigningKeyProvider _signingKeyProvider = signingKeyProvider;

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
                    return false;
                }

                var publicKeyPem = _signingKeyProvider.GetPublicKeyPem(token.Header.Kid);
                if (publicKeyPem is null)
                {
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
                    IssuerSigningKey = new RsaSecurityKey(rsa) { KeyId = token.Header.Kid },
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                handler.ValidateToken(accessToken, validationParameters, out _);
                return true;
            }
            catch (Exception exception) when (exception is SecurityTokenException or ArgumentException or FormatException or CryptographicException)
            {
                return false;
            }
        }
    }
}
