using Microsoft.AspNetCore.Mvc;
using Shared.Logging;
using Shared.Services;

namespace Premise.Presentation.Controllers
{
    [ApiController]
    [Route("api/premise-medias")]
    public sealed class PremiseMediaController(
        GoogleDriveImageStorage imageStorage,
        ILogger<PremiseMediaController> logger,
        ILogPool logPool
    ) : ControllerBase
    {
        private const long MaxImageSize = 10 * 1024 * 1024;

        private const string Module = "Premise";

        private const string Layer = "Presentation/Controller";

        private static readonly HashSet<string> SupportedImageTypes =
            ["image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"];

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<PremiseMediaController> _logger = logger;

        [HttpPost("upload")]
        [RequestSizeLimit(MaxImageSize)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file.Length is 0 or > MaxImageSize)
            {
                return BadRequest("Ảnh phải có dung lượng từ 1 byte đến 10 MB.");
            }

            if (!SupportedImageTypes.Contains(file.ContentType))
            {
                _logger.LogLayerInformation(_logPool, Module, Layer, "unsupported image type.");
                await _logPool.FlushAsync(Module, Layer, cancellationToken);
                return BadRequest("Chỉ hỗ trợ ảnh JPEG, PNG, JPG, WebP hoặc GIF.");
            }

            try
            {
                var imageUrl = await imageStorage.UploadAsync(file, cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer, $"successfully uploaded image to Google Drive: {imageUrl}");
                return Ok(new { imageUrl });
            }
            catch (Exception exception)
            {
                _logger.LogLayerError(_logPool, Module, Layer, exception, "failed to upload image to Google Drive.");
                return BadRequest(exception.Message);
            }
            finally
            {
                await _logPool.FlushAsync(Module, Layer, cancellationToken);
            }
        }
    }
}