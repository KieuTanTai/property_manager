using Microsoft.AspNetCore.SignalR;
using Shared.Logging;

namespace TicketAndNotification.Infrastructure.SignalR
{
    public sealed class NotificationHub(
        ILogger<NotificationHub> logger,
        ILogPool logPool,
        IConfiguration configuration) : Hub
    {
        private const string Module = "TicketAndNotification";

        private const string Layer = "Infrastructure/SignalR";

        public const string Route = "/hubs/notifications";

        public static string GetAccountGroupName(Guid accountId)
        {
            return $"account:{accountId:D}";
        }

        public override async Task OnConnectedAsync()
        {
            var configuredKey = configuration["INTERNAL_GATEWAY_API_KEY"];
            var providedKey = Context.GetHttpContext()?.Request.Headers["X-Internal-Gateway-Key"].ToString();
            if (string.IsNullOrWhiteSpace(configuredKey)
                || string.IsNullOrWhiteSpace(providedKey)
                || !FixedTimeEquals(configuredKey, providedKey))
            {
                logger.LogLayerWarning(logPool, Module, Layer,
                    "SignalR notification connection rejected because Gateway authentication headers are missing or invalid.");
                await logPool.FlushAsync(Module, Layer, CancellationToken.None);
                Context.Abort();
                return;
            }

            var accountIdValue = Context.GetHttpContext()?.Request.Headers["X-Authenticated-Account-Id"].ToString();
            if (!Guid.TryParse(accountIdValue, out var accountId)
                || accountId == Guid.Empty)
            {
                logger.LogLayerWarning(logPool, Module, Layer,
                    "SignalR notification connection rejected because the authenticated user has no valid account id claim.");
                await logPool.FlushAsync(Module, Layer, CancellationToken.None);
                Context.Abort();
                return;
            }

            logger.LogLayerInformation(logPool, Module, Layer,
                $"SignalR notification connection joined account group for account {accountId}.");

            await Groups.AddToGroupAsync(
                Context.ConnectionId, GetAccountGroupName(accountId));
            await logPool.FlushAsync(Module, Layer, CancellationToken.None);
            await base.OnConnectedAsync();
        }

        private static bool FixedTimeEquals(string expected, string provided)
        {
            var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expected);
            var providedBytes = System.Text.Encoding.UTF8.GetBytes(provided);
            return expectedBytes.Length == providedBytes.Length
                   && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }
    }
}