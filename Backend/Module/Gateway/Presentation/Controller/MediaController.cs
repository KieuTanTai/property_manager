using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Shared.Logging;
using Shared.Services;

namespace Gateway.Presentation.Controller
{
    [ApiController]
    [Route("api/media")]
    public sealed class MediaController(
        GoogleDriveImageStorage imageStorage,
        IConfiguration configuration,
        ILogger<MediaController> logger,
        ILogPool logPool
    ) : ControllerBase
    {
        private const long MaxImageSize = 10 * 1024 * 1024;

        private const string Module = "Media";

        private const string Layer = "Presentation/Controller";

        private static readonly HashSet<string> SupportedImageTypes =
            ["image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"];

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<MediaController> _logger = logger;

        [HttpGet("{fileId}/content")]
        public async Task<IActionResult> GetContent([FromRoute] string fileId, CancellationToken cancellationToken)
        {
            try
            {
                var (content, contentType) = await imageStorage.DownloadAsync(fileId, cancellationToken);
                return File(content, contentType);
            }
            catch (Exception exception)
            {
                _logger.LogLayerError(_logPool, Module, Layer, exception, "failed to download image from Google Drive.");
                await _logPool.FlushAsync(Module, Layer, cancellationToken);
                return StatusCode(StatusCodes.Status502BadGateway, "Không tải được ảnh từ Google Drive.");
            }
        }

        [HttpPost("upload/{purpose}")]
        [RequestSizeLimit(MaxImageSize)]
        public async Task<IActionResult> Upload(
            [FromRoute] string purpose,
            [FromForm] IFormFile file,
            CancellationToken cancellationToken)
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
                var folderSetting = purpose.ToLowerInvariant() switch
                {
                    "avatar" => "AvatarFolderId",
                    "premise" => "PremiseFolderId",
                    "ticket-and-notification" => "TicketAndNotificationFolderId",
                    _ => null,
                };
                if (folderSetting is null)
                {
                    return BadRequest("Loại ảnh tải lên không được hỗ trợ.");
                }

                var folderId = configuration[$"GoogleDrive:{folderSetting}"];
                if (string.IsNullOrWhiteSpace(folderId))
                {
                    _logger.LogLayerError(_logPool, Module, Layer,
                        new InvalidOperationException($"GoogleDrive:{folderSetting} is not configured."),
                        "Image upload folder is not configured.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Chưa cấu hình thư mục lưu ảnh trên Gateway.");
                }

                var imageUrl = await imageStorage.UploadAsync(file, folderId, cancellationToken);
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
