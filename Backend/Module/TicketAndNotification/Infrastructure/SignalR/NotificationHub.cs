using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Logging;

namespace TicketAndNotification.Infrastructure.SignalR
{
    [Authorize]
    public sealed class NotificationHub(
        ILogger<NotificationHub> logger,
        ILogPool logPool) : Hub
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
            var accountIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
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
    }
}