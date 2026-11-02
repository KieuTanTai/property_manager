using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Presentation.Controller
{
    [ApiController]
    [Route("api/premise")]
    [Route("api/businesstype")]
    [Route("api/business-type")]
    [Route("api/location")]
    [Route("api/product")]
    public sealed class PremiseProxyController(ModuleProxyService proxyService) : ModuleProxyControllerBase(proxyService)
    {
        protected override string ModuleUrlKey
        {
            get => "ModuleUrls:Premise";
        }
    }
}