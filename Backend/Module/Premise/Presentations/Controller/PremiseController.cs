using Microsoft.AspNetCore.Mvc;
using Premise.Application;
using Premise.Models.Business;
using Premise.Models.Premise;
using Premise.Presentations.Record.Location;
using Premise.Presentations.Record.Premise;
using Shared.Enum;
using Shared.Persistence.Record;

namespace Premise.Presentations.Controller
{
    [ApiController]
    [Route("api/premises")]
    public sealed class PremiseController(
        PremiseApplication premiseApplication) : ControllerBase
    {
        private readonly PremiseApplication _premiseApplication = premiseApplication;

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RecordPremiseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<RecordPremiseResponse>>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var premises = await _premiseApplication.GetAllAsync(cancellationToken);
                return Ok(premises.Select(ToResponse).ToList());
            }
            catch (InvalidOperationException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RecordPremiseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordPremiseResponse>> GetByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var premise = await _premiseApplication.GetByIdAsync(id, cancellationToken);
                return Ok(ToResponse(premise));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("by-name")]
        [ProducesResponseType(typeof(RecordPremiseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordPremiseResponse>> GetByNameAsync(
            [FromQuery] string name,
            CancellationToken cancellationToken)
        {
            try
            {
                var premise = await _premiseApplication.GetByNameAsync(name, cancellationToken);
                return Ok(ToResponse(premise));
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
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("by-floor/{floor:int}")]
        [ProducesResponseType(typeof(IReadOnlyList<RecordPremiseResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<RecordPremiseResponse>>> GetByFloorAsync(
            [FromRoute] int floor,
            CancellationToken cancellationToken)
        {
            try
            {
                var premises = await _premiseApplication.GetByFloorAsync(floor, cancellationToken);
                return Ok(premises.Select(ToResponse).ToList());
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("by-location/{locationId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<RecordPremiseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<RecordPremiseResponse>>> GetByLocationAsync(
            [FromRoute] Guid locationId,
            CancellationToken cancellationToken)
        {
            try
            {
                var premises = await _premiseApplication.GetByLocationIdAsync(locationId, cancellationToken);
                return Ok(premises.Select(ToResponse).ToList());
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("page")]
        [ProducesResponseType(typeof(RecordBaseCursorPage<RecordPremiseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecordBaseCursorPage<RecordPremiseResponse>>> GetPageAsync(
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var page = await _premiseApplication.GetApplyPagingAsync(cursor, pageSize, cancellationToken);
                return Ok(ToResponsePage(page));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("page/by-status/{status}")]
        [ProducesResponseType(typeof(RecordBaseCursorPage<RecordPremiseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecordBaseCursorPage<RecordPremiseResponse>>> GetPageByStatusAsync(
            [FromRoute] EPremiseStatus status,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(status))
            {
                return BadRequest(new ProblemDetails { Title = "Premise status is invalid." });
            }

            try
            {
                var page = await _premiseApplication.GetApplyPagingByStatusAsync(
                    cursor, pageSize, status, cancellationToken);
                return Ok(ToResponsePage(page));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("page/by-name")]
        [ProducesResponseType(typeof(RecordBaseCursorPage<RecordPremiseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecordBaseCursorPage<RecordPremiseResponse>>> GetPageByNameAsync(
            [FromQuery] string name,
            [FromQuery] Guid? cursor,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var page = await _premiseApplication.GetApplyPagingByNameAsync(
                    cursor, pageSize, name, cancellationToken);
                return Ok(ToResponsePage(page));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("{id:guid}/details")]
        [ProducesResponseType(typeof(RecordPremiseDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordPremiseDetailsResponse>> GetDetailsAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var premise = await _premiseApplication.GetDetailsByIdAsync(
                    id, cancellationToken: cancellationToken);
                return Ok(new RecordPremiseDetailsResponse(
                    ToResponse(premise),
                    premise.PremiseLocation is null ? null : ToLocationResponse(premise.PremiseLocation),
                    premise.PremiseMedia.Select(ToImageResponse).ToList(),
                    premise.PremiseBusinessTypes.Select(ToBusinessTypeResponse).ToList()));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(RecordPremiseResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordPremiseResponse>> CreateAsync(
            [FromBody] RecordAddPremiseRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var premise = new PremiseModel(
                    request.PremiseName,
                    request.PremiseLocationId,
                    request.PremiseStatus,
                    request.PremisePosition,
                    request.PremiseFloor,
                    request.PremiseArea,
                    request.PremiseDescription);

                var created = await _premiseApplication.AddAsync(premise, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = created.PremiseId }, ToResponse(created));
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
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(RecordPremiseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordPremiseResponse>> UpdateAsync(
            [FromRoute] Guid id,
            [FromBody] RecordUpdatePremiseRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updated = new PremiseModel(
                    id,
                    request.PremiseName,
                    request.PremiseLocationId,
                    request.PremiseStatus,
                    request.PremisePosition,
                    request.PremiseFloor,
                    request.PremiseArea,
                    request.PremiseDescription);

                var result = await _premiseApplication.UpdateAsync(updated, cancellationToken);
                return Ok(ToResponse(result));
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
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpPost("{id:guid}/images")]
        [ProducesResponseType(typeof(RecordPremiseImageResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordPremiseImageResponse>> AddImageAsync(
            [FromRoute] Guid id,
            [FromBody] RecordPremiseImageRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var image = await _premiseApplication.AddImageAsync(id, request.ImageUrl, cancellationToken);
                var response = ToImageResponse(image);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpPut("{id:guid}/images/{imageId:int}")]
        [ProducesResponseType(typeof(RecordPremiseImageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordPremiseImageResponse>> UpdateImageAsync(
            [FromRoute] Guid id,
            [FromRoute] int imageId,
            [FromBody] RecordPremiseImageRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var image = await _premiseApplication.UpdateImageAsync(
                    id,
                    imageId,
                    request.ImageUrl,
                    cancellationToken);
                return Ok(ToImageResponse(image));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("{id:guid}/images")]
        [ProducesResponseType(typeof(IReadOnlyList<RecordPremiseImageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<RecordPremiseImageResponse>>> GetImagesAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var images = await _premiseApplication.GetImagesAsync(id, cancellationToken);
                return Ok(images.Select(ToImageResponse).ToList());
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpGet("{id:guid}/business-types")]
        [ProducesResponseType(typeof(IReadOnlyList<RecordPremiseBusinessTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<RecordPremiseBusinessTypeResponse>>> GetBusinessTypesAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var businessTypes = await _premiseApplication.GetBusinessTypesAsync(id, cancellationToken);
                return Ok(businessTypes.Select(ToBusinessTypeResponse).ToList());
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = exception.Message });
            }
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpPost("{id:guid}/business-types")]
        [ProducesResponseType(typeof(RecordPremiseBusinessTypeAssignmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordPremiseBusinessTypeAssignmentResponse>> AddBusinessTypeAsync(
            [FromRoute] Guid id,
            [FromBody] RecordPremiseBusinessTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var association = await _premiseApplication.AddBusinessTypeAsync(
                    id, request.BusinessTypeId, cancellationToken);
                return StatusCode(
                    StatusCodes.Status201Created,
                    new RecordPremiseBusinessTypeAssignmentResponse(
                        association.PremiseId, association.BusinessTypeId, association.AssignedAt));
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
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpDelete("{id:guid}/business-types/{businessTypeId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RemoveBusinessTypeAsync(
            [FromRoute] Guid id,
            [FromRoute] Guid businessTypeId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _premiseApplication.RemoveBusinessTypeAsync(id, businessTypeId, cancellationToken);
                return NoContent();
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
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        private static RecordPremiseResponse ToResponse(PremiseModel premise)
        {
            return new RecordPremiseResponse(
                premise.PremiseId,
                premise.PremiseName,
                premise.PremiseLocationId,
                premise.PremiseStatus,
                premise.PremisePosition,
                premise.PremiseFloor,
                premise.PremiseArea,
                premise.PremiseDescription,
                premise.PremiseCreatedAt,
                premise.PremiseUpdatedAt);
        }

        private static RecordBaseCursorPage<RecordPremiseResponse> ToResponsePage(
            RecordBaseCursorPage<PremiseModel> page)
        {
            return new RecordBaseCursorPage<RecordPremiseResponse>(
                page.Items.Select(ToResponse).ToList(),
                page.NextCursor,
                page.IsHasMore);
        }

        private static RecordPremiseImageResponse ToImageResponse(PremiseMediaModel image)
        {
            return new RecordPremiseImageResponse(
                image.PremiseMediaId,
                image.PremiseMediaPremiseId,
                image.PremiseMediaImageUrl,
                image.PremiseMediaCreatedAt,
                image.PremiseMediaUpdatedAt);
        }

        private static RecordLocationResponse ToLocationResponse(LocationModel location)
        {
            return new RecordLocationResponse(
                location.LocationId,
                location.LocationAddress,
                location.LocationIsActive,
                location.LocationCreatedAt,
                location.LocationUpdatedAt);
        }

        private static RecordPremiseBusinessTypeResponse ToBusinessTypeResponse(BusinessTypeModel businessType)
        {
            return new RecordPremiseBusinessTypeResponse(
                businessType.BusinessTypeId,
                businessType.BusinessTypeName,
                businessType.BusinessTypeDescription,
                businessType.BusinessTypeIsActive);
        }
    }
}
