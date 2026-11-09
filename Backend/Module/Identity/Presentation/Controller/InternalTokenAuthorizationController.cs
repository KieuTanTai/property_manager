using System.Security.Cryptography;
using System.Text;
using Identity.Interfaces.Credentials;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging;
using Shared.Persistence.Record.Auth;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("internal/auth/authorize")]
    public sealed class InternalTokenAuthorizationController(
        IJwtAccessTokenValidator tokenValidator,
        IConfiguration configuration,
        ILogger<InternalTokenAuthorizationController> logger,
        ILogPool logPool) : ControllerBase
    {
        private const string Module = "Identity";
        private const string Layer = "Presentation/Controller";
        private const string GatewayKeyHeader = "X-Internal-Gateway-Key";

        private readonly ILogger<InternalTokenAuthorizationController> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        [HttpPost]
        public IActionResult AuthorizeToken([FromBody] RecordInternalTokenAuthorizationRequest request)
        {
            if (!IsAuthorizedGateway())
            {
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Internal token authorization rejected because the Gateway API key did not match.");
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (tokenValidator.IsValid(request.AccessToken))
            {
                return NoContent();
            }
            _logger.LogLayerWarning(_logPool, Module, Layer,
                "Internal token authorization rejected an invalid or expired access token.");
            return Unauthorized();

        }

        private bool IsAuthorizedGateway()
        {
            var expectedKey = configuration["INTERNAL_GATEWAY_API_KEY"];
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
