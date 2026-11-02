namespace Shared.ModelHelper
{
    public static class ImageDataUriGuard
    {
        // Base64 expands N bytes to 4 * ceil(N / 3) characters; allow up to 64 chars for the data URI prefix.
        public const int AvatarMaxEncodedCharacters = 4 * ((200 * 1024 + 2) / 3) + 64;
        public const int StandardMaxEncodedCharacters = 4 * ((300 * 1024 + 2) / 3) + 64;

        private static readonly HashSet<string> SupportedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
            { "image/jpeg", "image/png", "image/webp", "image/gif" };

        public static string Required(string? value, int maxDecodedBytes, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Image data is required.", parameterName);
            }

            // Keep existing externally hosted images as HTTPS links; new uploads use data URIs.
            if (Uri.TryCreate(value, UriKind.Absolute, out var imageUri)
                && imageUri.Scheme == Uri.UriSchemeHttps)
            {
                return value;
            }

            var separator = value.IndexOf(',');
            if (separator < 0 || !value[..separator].EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Image must be a Base64 data URI.", parameterName);
            }

            if (value.Length > 4 * ((maxDecodedBytes + 2) / 3) + 64)
            {
                throw new ArgumentException("Image data exceeds the encoded size limit.", parameterName);
            }

            var mediaType = value[5..value.IndexOf(';')];
            if (!SupportedMediaTypes.Contains(mediaType))
            {
                throw new ArgumentException("Image type must be JPEG, PNG, WebP, or GIF.", parameterName);
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(value[(separator + 1)..]);
            }
            catch (FormatException exception)
            {
                throw new ArgumentException("Image data is not valid Base64.", parameterName, exception);
            }

            if (imageBytes.Length == 0 || imageBytes.Length > maxDecodedBytes)
            {
                throw new ArgumentException($"Image must be between 1 and {maxDecodedBytes} bytes after decoding.", parameterName);
            }

            return value;
        }

        public static string? OptionalAvatar(string? value, string parameterName)
        {
            return string.IsNullOrWhiteSpace(value) ? value : Required(value, 200 * 1024, parameterName);
        }
    }
}
