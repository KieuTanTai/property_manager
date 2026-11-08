using System.Security.Cryptography;
using System.Text;
using Identity.Interfaces.Credentials;
using Identity.Presentation.Record.Account;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("internal/auth/keys")]
    public sealed class InternalSigningKeyController(
        IRsaSigningKeyProvider signingKeyProvider,
        IConfiguration configuration) : ControllerBase
    {
        private const string GatewayKeyHeader = "X-Internal-Gateway-Key";

        [HttpGet("{kid}")]
        public IActionResult GetPublicKey(string kid)
        {
            if (!IsAuthorizedGateway())
            {
                return Unauthorized();
            }

            var publicKeyPem = signingKeyProvider.GetPublicKeyPem(kid);
            return publicKeyPem is null
                ? NotFound()
                : Ok(new RecordPublicSigningKeyResponse(kid, publicKeyPem));
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
