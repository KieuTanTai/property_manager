using Microsoft.AspNetCore.Mvc;
using Premise.Application;
using Premise.Interfaces.IApplication;
using Premise.Models.Premise;
using Premise.Presentations.Record.Location;

namespace Premise.Presentations.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class LocationController(
        ILocationApplication locationApplication) : ControllerBase
    {
        private readonly ILocationApplication _locationApplication = locationApplication;

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RecordLocationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<RecordLocationResponse>>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var locations = await _locationApplication.GetAllAsync(cancellationToken);
                return Ok(locations.Select(ToResponse).ToList());
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
        [ProducesResponseType(typeof(RecordLocationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordLocationResponse>> GetByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var location = await _locationApplication.GetByIdAsync(id, cancellationToken);
                return Ok(ToResponse(location));
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
        [ProducesResponseType(typeof(RecordLocationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordLocationResponse>> CreateAsync(
            [FromBody] RecordAddLocationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var created = await _locationApplication.AddAsync(request.LocationAddress, cancellationToken);
                var response = ToResponse(created);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = created.LocationId }, response);
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
        [ProducesResponseType(typeof(RecordLocationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordLocationResponse>> UpdateAsync(
            [FromRoute] Guid id,
            [FromBody] RecordUpdateLocationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var updated = await _locationApplication.UpdateAsync(id, request.LocationAddress, cancellationToken);
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
            catch (OperationCanceledException exception)
            {
                return BadRequest(new ProblemDetails { Title = $"Request canceled. {exception.Message}" });
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                await _locationApplication.DeleteAsync(id, cancellationToken);
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

        private static RecordLocationResponse ToResponse(LocationModel location)
        {
            return new RecordLocationResponse(
                location.LocationId,
                location.LocationAddress,
                location.LocationCreatedAt,
                location.LocationUpdatedAt);
        }
    }
}
