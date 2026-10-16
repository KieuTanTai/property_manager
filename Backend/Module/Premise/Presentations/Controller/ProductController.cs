using Microsoft.AspNetCore.Mvc;
using Premise.Application;
using Premise.Interfaces.IApplication;
using Premise.Models.Business;
using Premise.Models.Product;
using Premise.Presentations.Record.Product;

namespace Premise.Presentations.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ProductController(
        IProductApplication productApplication) : ControllerBase
    {
        private readonly IProductApplication _productApplication = productApplication;

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RecordProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<RecordProductResponse>>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var products = await _productApplication.GetAllAsync(cancellationToken);
                return Ok(products.Select(ToResponse).ToList());
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
        [ProducesResponseType(typeof(RecordProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordProductResponse>> GetByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productApplication.GetByIdAsync(id, cancellationToken);
                return Ok(ToResponse(product));
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
        [ProducesResponseType(typeof(RecordProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordProductResponse>> CreateAsync(
            [FromBody] RecordAddProductRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productApplication.AddAsync(
                    request.ProductName,
                    request.ProductDescription,
                    cancellationToken);

                return CreatedAtAction(
                    nameof(GetByIdAsync),
                    new { id = product.WhitelistProductId },
                    ToResponse(product));
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
        [ProducesResponseType(typeof(RecordProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordProductResponse>> UpdateAsync(
            [FromRoute] Guid id,
            [FromBody] RecordUpdateProductRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productApplication.UpdateAsync(
                    id,
                    request.ProductName,
                    request.ProductDescription,
                    cancellationToken);

                return Ok(ToResponse(product));
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

        [HttpGet("by-business-type/{businessTypeId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<RecordProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<RecordProductResponse>>> GetByBusinessTypeAsync(
            [FromRoute] Guid businessTypeId,
            CancellationToken cancellationToken)
        {
            try
            {
                var products = await _productApplication.GetByBusinessTypeIdAsync(
                    businessTypeId, cancellationToken);
                return Ok(products.Select(ToResponse).ToList());
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
        [ProducesResponseType(typeof(IReadOnlyList<RecordProductBusinessTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<RecordProductBusinessTypeResponse>>> GetBusinessTypesAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var businessTypes = await _productApplication.GetBusinessTypesAsync(id, cancellationToken);
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
        [ProducesResponseType(
            typeof(RecordProductBusinessTypeAssignmentResponse),
            StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordProductBusinessTypeAssignmentResponse>> AddBusinessTypeAsync(
            [FromRoute] Guid id,
            [FromBody] RecordProductBusinessTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var association = await _productApplication.AddBusinessTypeAsync(
                    id, request.BusinessTypeId, cancellationToken);
                return StatusCode(
                    StatusCodes.Status201Created,
                    new RecordProductBusinessTypeAssignmentResponse(
                        association.ProductId,
                        association.BusinessTypeId,
                        association.AssignedAt));
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
                await _productApplication.RemoveBusinessTypeAsync(id, businessTypeId, cancellationToken);
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

        private static RecordProductResponse ToResponse(WhitelistProductModel product)
        {
            return new RecordProductResponse(
                product.WhitelistProductId,
                product.WhitelistProductName,
                product.WhitelistProductDescription,
                product.WhitelistProductCreatedAt,
                product.WhitelistProductUpdatedAt);
        }

        private static RecordProductBusinessTypeResponse ToBusinessTypeResponse(
            BusinessTypeModel businessType)
        {
            return new RecordProductBusinessTypeResponse(
                businessType.BusinessTypeId,
                businessType.BusinessTypeName,
                businessType.BusinessTypeDescription,
                businessType.BusinessTypeIsActive);
        }
    }
}
