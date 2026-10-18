using System.Globalization;
using System.Text;

namespace Payment.Utils
{
    /// <summary>
    /// VNPAY requires <c>vnp_OrderInfo</c> to be non-accented and free of special characters.
    /// </summary>
    public static class PaymentDescriptionHelper
    {
        private const int DefaultMaxLength = 255;

        public static string Sanitize(string? input, int maxLength = DefaultMaxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var normalized = input.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                switch (character)
                {
                    case 'đ':
                        builder.Append('d');
                        break;
                    case 'Đ':
                        builder.Append('D');
                        break;
                    default:
                        if (char.IsLetterOrDigit(character) || character == ' ')
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            var result = builder.ToString().Normalize(NormalizationForm.FormC).Trim();
            while (result.Contains("  ", StringComparison.Ordinal))
            {
                result = result.Replace("  ", " ", StringComparison.Ordinal);
            }

            return result.Length <= maxLength ? result : result[..maxLength];
        }
    }
}
