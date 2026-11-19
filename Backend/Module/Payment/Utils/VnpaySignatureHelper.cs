using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Payment.Utils
{
    /// <summary>
    /// Re-implementation of the VNPAY checksum verification so that an invalid signature can be
    /// told apart from a valid-but-failed transaction. The library throws the same
    /// <c>VnpayException</c> for both cases, which is not safe to persist.
    /// Algorithm mirrors VNPAY.NET: vnp_* params, ordinal sort, RFC-3986-ish url encoding,
    /// HMAC-SHA512 over UTF-8, uppercase hex, case-insensitive comparison.
    /// </summary>
    public static class VnpaySignatureHelper
    {
        public static bool IsValid(IQueryCollection query, string? hashSecret)
        {
            if (string.IsNullOrEmpty(hashSecret))
            {
                return false;
            }

            var secureHash = query["vnp_SecureHash"].ToString();
            if (string.IsNullOrWhiteSpace(secureHash))
            {
                return false;
            }

            var sorted = new SortedList<string, string>(StringComparer.Ordinal);
            foreach (var parameter in query)
            {
                if (string.IsNullOrEmpty(parameter.Key)
                    || !parameter.Key.StartsWith("vnp_", StringComparison.Ordinal)
                    || string.Equals(parameter.Key, "vnp_SecureHash", StringComparison.Ordinal)
                    || string.Equals(parameter.Key, "vnp_SecureHashType", StringComparison.Ordinal))
                {
                    continue;
                }

                var value = parameter.Value.ToString();
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                sorted[parameter.Key] = value;
            }

            var signData = new StringBuilder();
            foreach (var pair in sorted)
            {
                signData.Append(WebUtility.UrlEncode(pair.Key))
                    .Append('=')
                    .Append(WebUtility.UrlEncode(pair.Value))
                    .Append('&');
            }

            if (signData.Length == 0)
            {
                return false;
            }

            signData.Length--;

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData.ToString()));
            var computed = Convert.ToHexString(computedHash);

            return string.Equals(computed, secureHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
