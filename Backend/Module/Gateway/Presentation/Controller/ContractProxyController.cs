using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Presentation.Controller
{
    [ApiController]
    [Route("api/contract")]
    [Route("api/regulation")]
    [Route("api/invoice")]
    [Route("api/receipt")]
    [Route("api/contact")]
    [Route("api/violation")]
    public sealed class ContractProxyController(ModuleProxyService proxyService) : ModuleProxyControllerBase(proxyService)
    {
        protected override string ModuleUrlKey
        {
            get => "ModuleUrls:Contract";
        }
    }
}
