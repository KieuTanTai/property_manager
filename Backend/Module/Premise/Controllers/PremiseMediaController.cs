using Microsoft.AspNetCore.Mvc;
using Shared.Services;

namespace Premise.Controllers;

[ApiController]
[Route("api/premise-medias")]
public sealed class PremiseMediaController(GoogleDriveImageStorage imageStorage) : ControllerBase
{
    private const long MaxImageSize = 10 * 1024 * 1024;
    private static readonly HashSet<string> SupportedImageTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];

    [HttpPost("upload")]
    [RequestSizeLimit(MaxImageSize)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0 || file.Length > MaxImageSize)
            return BadRequest("Ảnh phải có dung lượng từ 1 byte đến 10 MB.");

        if (!SupportedImageTypes.Contains(file.ContentType))
            return BadRequest("Chỉ hỗ trợ ảnh JPEG, PNG, WebP hoặc GIF.");

        try
        {
            var imageUrl = await imageStorage.UploadAsync(file, cancellationToken);
            return Ok(new { imageUrl });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
