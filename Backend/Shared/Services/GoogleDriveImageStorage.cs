using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Shared.Services
{
    public sealed class GoogleDriveImageStorage(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        private const string DriveScope = "https://www.googleapis.com/auth/drive.file";
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string> UploadAsync(
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            var serviceAccountJson = _configuration["GoogleDrive:ServiceAccountJson"];
            var folderId = _configuration["GoogleDrive:FolderId"];
            if (string.IsNullOrWhiteSpace(serviceAccountJson) || string.IsNullOrWhiteSpace(folderId))
            {
                throw new InvalidOperationException(
                    "Google Drive chưa được cấu hình. Hãy đặt GoogleDrive__ServiceAccountJson và GoogleDrive__FolderId cho API đang sử dụng dịch vụ.");
            }

            using var serviceAccount = JsonDocument.Parse(serviceAccountJson);
            var root = serviceAccount.RootElement;
            var email = root.GetProperty("client_email").GetString();
            var privateKey = root.GetProperty("private_key").GetString();
            var tokenUri = root.TryGetProperty("token_uri", out var tokenUriElement)
                ? tokenUriElement.GetString()!
                : "https://oauth2.googleapis.com/token";
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(privateKey))
            {
                throw new InvalidOperationException("Google Drive service account JSON không hợp lệ.");
            }

            var accessToken = await GetAccessTokenAsync(email, privateKey, tokenUri, cancellationToken);
            var fileId = await UploadFileAsync(file, folderId, accessToken, cancellationToken);
            await MakeFileReadableAsync(fileId, accessToken, cancellationToken);
            return $"https://drive.google.com/uc?export=view&id={Uri.EscapeDataString(fileId)}";
        }

        private async Task<string> GetAccessTokenAsync(
            string email,
            string privateKey,
            string tokenUri,
            CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", typ = "JWT" }));
            var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
            {
                iss = email,
                scope = DriveScope,
                aud = tokenUri,
                iat = now,
                exp = now + 3600,
            }));
            var unsignedToken = $"{header}.{payload}";

            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);
            var signature = rsa.SignData(Encoding.ASCII.GetBytes(unsignedToken), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var assertion = $"{unsignedToken}.{Base64UrlEncode(signature)}";

            using var response = await _httpClient.PostAsync(
                tokenUri,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    ["assertion"] = assertion,
                }),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Google OAuth không cấp được quyền truy cập Drive.");
            }

            using var tokenResponse = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return tokenResponse.RootElement.GetProperty("access_token").GetString()
                   ?? throw new InvalidOperationException("Google OAuth trả về access token rỗng.");
        }

        private async Task<string> UploadFileAsync(
            IFormFile file,
            string folderId,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var extension = file.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => throw new InvalidOperationException("Định dạng ảnh không được hỗ trợ."),
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
                throw new InvalidOperationException($"Google Drive không lưu được ảnh (HTTP {(int)response.StatusCode}).");
            }

            using var uploadResponse = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return uploadResponse.RootElement.GetProperty("id").GetString()
                   ?? throw new InvalidOperationException("Google Drive không trả về mã tệp ảnh.");
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
                throw new InvalidOperationException($"Không thể tạo link xem ảnh Drive (HTTP {(int)response.StatusCode}).");
            }
        }

        private static string Base64UrlEncode(byte[] value) =>
            Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
