using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Presentation.Controller
{
    [ApiController]
    [Route("api/ticket")]
    [Route("api/ticketmedia")]
    [Route("api/ticket-medias")]
    [Route("api/notifications")]
    [Route("api/notification-recipients")]
    public sealed class TicketAndNotificationProxyController(ModuleProxyService proxyService) : ModuleProxyControllerBase(proxyService)
    {
        protected override string ModuleUrlKey
        {
            get => "ModuleUrls:TicketAndNotification";
        }
    }
}