using Microsoft.AspNetCore.Mvc;
using Premise.Interfaces.IApplication;
using Premise.Models.Business;
using Premise.Presentations.Record.Business;

namespace Premise.Presentations.Controller
{
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

        [HttpPost]
        [ProducesResponseType(typeof(RecordBusinessTypeResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordBusinessTypeResponse>> CreateAsync(
            [FromBody] RecordBusinessTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var created = await businessTypeApplication.AddAsync(
                    request.BusinessTypeName,
                    request.BusinessTypeDescription,
                    request.BusinessTypeIsActive,
                    cancellationToken);
                return StatusCode(StatusCodes.Status201Created, ToResponse(created));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new ProblemDetails { Title = exception.Message });
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(RecordBusinessTypeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordBusinessTypeResponse>> UpdateAsync(
            [FromRoute] Guid id,
            [FromBody] RecordBusinessTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updated = await businessTypeApplication.UpdateAsync(
                    id,
                    request.BusinessTypeName,
                    request.BusinessTypeDescription,
                    request.BusinessTypeIsActive,
                    cancellationToken);
                return Ok(ToResponse(updated));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new ProblemDetails { Title = exception.Message });
            }
        }

        [HttpPatch("{id:guid}/active")]
        [ProducesResponseType(typeof(RecordBusinessTypeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordBusinessTypeResponse>> SetActiveAsync(
            [FromRoute] Guid id,
            [FromBody] bool isActive,
            CancellationToken cancellationToken)
        {
            try
            {
                var updated = await businessTypeApplication.SetActiveAsync(
                    id, isActive, cancellationToken);
                return Ok(ToResponse(updated));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new ProblemDetails { Title = exception.Message });
            }
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
}