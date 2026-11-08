using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.Interfaces.Credentials;
using Microsoft.Extensions.Options;
using Shared.Persistence.Record.Auth;

namespace Identity.Infrastructure.Credentials
{
    public sealed class JwtTokenService(
        IOptions<JwtOptions> options,
        IRsaSigningKeyProvider signingKeyProvider) : IJwtTokenService
    {
        private readonly JwtOptions _options = options.Value;
        private readonly IRsaSigningKeyProvider _signingKeyProvider = signingKeyProvider;

        public string GenerateAccessToken(RecordAuthResponse account)
        {
            var now = DateTime.UtcNow;
            var key = _signingKeyProvider.GetCurrentSigningKey();
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, account.AccountId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
                new("version", _options.Version.ToString(System.Globalization.CultureInfo.InvariantCulture), ClaimValueTypes.Integer32)
            };
            claims.AddRange(account.RoleCodes.Select(role => new Claim("roles", role)));
            claims.AddRange(account.PermissionCodes.Select(permission => new Claim("permissions", permission)));

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddHours(_options.ExpirationHours),
                signingCredentials: key.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
