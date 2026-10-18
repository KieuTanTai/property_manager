using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Logging;

namespace Shared.Services
{
    public sealed class GoogleDriveImageStorage(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleDriveImageStorage> logger,
        ILogPool logPool,
        IHostEnvironment environment
        )
    {
        private const string LogLayer = "DriveUploader";
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<GoogleDriveImageStorage> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        private readonly string _module = environment.ApplicationName;

        public async Task<string> UploadAsync(
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, _module, LogLayer,
                $"Google Drive upload started. FileName={Path.GetFileName(file.FileName)}, ContentType={file.ContentType}, Size={file.Length} bytes.");
            try
            {
                var folderId = _configuration["GoogleDrive:FolderId"];
                var clientId = _configuration["GoogleDrive:OAuth:ClientId"];
                var clientSecret = _configuration["GoogleDrive:OAuth:ClientSecret"];
                var refreshToken = _configuration["GoogleDrive:OAuth:RefreshToken"];
                if (string.IsNullOrWhiteSpace(folderId) ||
                    string.IsNullOrWhiteSpace(clientId) ||
                    string.IsNullOrWhiteSpace(clientSecret) ||
                    string.IsNullOrWhiteSpace(refreshToken))
                {
                    throw new InvalidOperationException(
                        "Google Drive OAuth is not configured. Set GoogleDrive__FolderId, GoogleDrive__OAuth__ClientId, GoogleDrive__OAuth__ClientSecret, and GoogleDrive__OAuth__RefreshToken for the API using this service.");
                }

                var accessToken = await GetAccessTokenAsync(clientId, clientSecret, refreshToken, cancellationToken);
                _logger.LogLayerInformation(_logPool, _module, LogLayer, "Google OAuth access token acquired.");
                var fileId = await UploadFileAsync(file, folderId, accessToken, cancellationToken);
                _logger.LogLayerInformation(_logPool, _module, LogLayer, $"Image uploaded to Google Drive. FileId={fileId}.");
                await MakeFileReadableAsync(fileId, accessToken, cancellationToken);
                _logger.LogLayerInformation(_logPool, _module, LogLayer, $"Google Drive image permission created. FileId={fileId}.");
                return $"https://drive.google.com/uc?export=view&id={Uri.EscapeDataString(fileId)}";
            }
            catch (Exception exception)
            {
                _logger.LogLayerError(_logPool, _module, LogLayer, exception,
                    $"Google Drive upload failed. FileName={Path.GetFileName(file.FileName)}, ContentType={file.ContentType}, Size={file.Length} bytes. Error={exception}");
                throw;
            }
            finally
            {
                try
                {
                    await _logPool.FlushAsync(_module, LogLayer, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Could not flush Google Drive upload logs for module {Module}", _module);
                }
            }
        }

        private async Task<string> GetAccessTokenAsync(
            string clientId,
            string clientSecret,
            string refreshToken,
            CancellationToken cancellationToken)
        {
            using var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["refresh_token"] = refreshToken,
                }),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Google OAuth did not grant Drive access (HTTP {(int)response.StatusCode}): {responseBody}");
            }

            using var tokenResponse = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return tokenResponse.RootElement.GetProperty("access_token").GetString()
                   ?? throw new InvalidOperationException("Google OAuth returned an empty access token.");
        }

        private async Task<string> UploadFileAsync(
            IFormFile file,
            string folderId,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var extension = file.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpeg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => throw new InvalidOperationException("The image format is not supported."),
            };
            var fileName = $"image-{Guid.NewGuid():N}{extension}";
            var metadata = new Dictionary<string, object>
            {
                ["name"] = fileName,
                ["mimeType"] = file.ContentType,
                ["parents"] = new[] { folderId },
            };
            using var content = new MultipartContent("related");
            var metadataContent = new StringContent(JsonSerializer.Serialize(metadata), Encoding.UTF8);
            metadataContent.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "UTF-8" };
            content.Add(metadataContent);
            var imageContent = new StreamContent(file.OpenReadStream());
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(imageContent);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart&supportsAllDrives=true&fields=id");
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Google Drive could not save the image (HTTP {(int)response.StatusCode}): {responseBody}");
            }

            using var uploadResponse = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return uploadResponse.RootElement.GetProperty("id").GetString()
                   ?? throw new InvalidOperationException("Google Drive did not return an image file ID.");
        }

        private async Task MakeFileReadableAsync(
            string fileId,
            string accessToken,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://www.googleapis.com/drive/v3/files/{Uri.EscapeDataString(fileId)}/permissions?supportsAllDrives=true");
            request.Content = new StringContent("{\"type\":\"anyone\",\"role\":\"reader\",\"allowFileDiscovery\":false}", Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Could not create a public Google Drive image link (HTTP {(int)response.StatusCode}): {responseBody}");
            }
        }

    }
}
