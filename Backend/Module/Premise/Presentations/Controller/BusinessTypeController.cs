using Microsoft.AspNetCore.Mvc;
using Premise.Interfaces.IApplication;
using Premise.Models.Business;
using Premise.Presentations.Record.Business;

namespace Premise.Presentations.Controller;

[ApiController]
[Route("api/[controller]")]
public sealed class BusinessTypeController(
    IBusinessTypeApplication businessTypeApplication) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecordBusinessTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecordBusinessTypeResponse>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var businessTypes = await businessTypeApplication.GetAllAsync(cancellationToken);
        return Ok(businessTypes.Select(ToResponse).ToList());
    }

    private static RecordBusinessTypeResponse ToResponse(BusinessTypeModel businessType)
    {
        return new RecordBusinessTypeResponse(
            businessType.BusinessTypeId,
            businessType.BusinessTypeName,
            businessType.BusinessTypeDescription,
            businessType.BusinessTypeIsActive,
            businessType.BusinessTypeCreatedAt,
            businessType.BusinessTypeUpdatedAt);
    }
}
