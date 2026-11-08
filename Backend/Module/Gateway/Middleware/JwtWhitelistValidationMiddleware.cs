using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Gateway.Services;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Middleware
{
    public sealed class JwtWhitelistValidationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IIdentitySigningKeyResolver signingKeyResolver)
    {
        private readonly RequestDelegate _next = next;
        private readonly IConfiguration _configuration = configuration;
        private readonly IIdentitySigningKeyResolver _signingKeyResolver = signingKeyResolver;

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsPublicEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var authorization = context.Request.Headers.Authorization.ToString();
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await RejectAsync(context, "Bearer access token is required.");
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            var rawToken = authorization["Bearer ".Length..].Trim();
            try
            {
                var jwt = handler.ReadJwtToken(rawToken);
                if (string.IsNullOrWhiteSpace(jwt.Header.Kid))
                {
                    await RejectAsync(context, "Token does not identify its signing key.");
                    return;
                }

                var publicKeyPem = await _signingKeyResolver.GetPublicKeyPemAsync(jwt.Header.Kid, context.RequestAborted);
                if (publicKeyPem is null)
                {
                    await RejectAsync(context, "Token signing key is not trusted.");
                    return;
                }

                using var rsa = RSA.Create();
                rsa.ImportFromPem(publicKeyPem);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa) { KeyId = jwt.Header.Kid },
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                handler.ValidateToken(rawToken, validationParameters, out _);
                await _next(context);
            }
            catch (Exception exception) when (exception is SecurityTokenException or ArgumentException or FormatException or CryptographicException)
            {
                await RejectAsync(context, "Access token is invalid or expired.");
            }
        }

        private static bool IsPublicEndpoint(PathString path) =>
            path.Equals("/api/account/login", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/account/register", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/account/session-instance", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWithSegments("/internal/auth/keys", StringComparison.OrdinalIgnoreCase);

        private static async Task RejectAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = message }, context.RequestAborted);
        }
    }
}
