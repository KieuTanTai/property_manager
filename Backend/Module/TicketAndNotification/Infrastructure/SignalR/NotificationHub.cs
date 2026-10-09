using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TicketAndNotification.Infrastructure.SignalR
{
    [Authorize]
    public sealed class NotificationHub(ILogger<NotificationHub> logger) : Hub
    {
        public const string Route = "/hubs/notifications";

        public static string GetAccountGroupName(Guid accountId) =>
            $"account:{accountId:D}";

        public override async Task OnConnectedAsync()
        {
            var accountIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(accountIdValue, out var accountId)
                || accountId == Guid.Empty)
            {
                logger.LogWarning(
                    "SignalR notification connection rejected because the authenticated user has no valid account id claim.");
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId, GetAccountGroupName(accountId));
            await base.OnConnectedAsync();
        }
    }
}
