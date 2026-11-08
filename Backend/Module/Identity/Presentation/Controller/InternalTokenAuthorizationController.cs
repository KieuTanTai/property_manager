using System.Security.Cryptography;
using System.Text;
using Identity.Interfaces.Credentials;
using Identity.Presentation.Record.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Persistence.Record.Auth;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("internal/auth/authorize")]
    public sealed class InternalTokenAuthorizationController(
        IJwtAccessTokenValidator tokenValidator,
        IConfiguration configuration) : ControllerBase
    {
        private const string GatewayKeyHeader = "X-Internal-Gateway-Key";

        [HttpPost]
        public IActionResult AuthorizeToken([FromBody] RecordInternalTokenAuthorizationRequest request)
        {
            if (!IsAuthorizedGateway())
            {
                return Unauthorized();
            }

            return tokenValidator.IsValid(request.AccessToken) ? NoContent() : Unauthorized();
        }

        private bool IsAuthorizedGateway()
        {
            var expectedKey = configuration["InternalGateway:ApiKey"];
            var providedKey = Request.Headers[GatewayKeyHeader].ToString();
            if (string.IsNullOrWhiteSpace(expectedKey) || string.IsNullOrWhiteSpace(providedKey))
            {
                return false;
            }

            var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
            var providedBytes = Encoding.UTF8.GetBytes(providedKey);
            return expectedBytes.Length == providedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }
    }
}
