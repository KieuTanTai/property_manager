using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Presentation.Controller
{
    [ApiController]
    [Route("api/account")]
    [Route("api/authorization")]
    [Route("api/profile")]
    public sealed class IdentityProxyController(ModuleProxyService proxyService) : ModuleProxyControllerBase(proxyService)
    {
        protected override string ModuleUrlKey
        {
            get => "ModuleUrls:Identity";
        }
    }
}
