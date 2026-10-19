using static System.Text.RegularExpressions.Regex;

namespace Shared.Enum
{
    public static class SnakeCaseEnum
    {
        public static string ToSnakeCase<TEnum>(TEnum value) where TEnum : struct, System.Enum =>
            Replace(value.ToString(), "(?<=[a-z0-9])(?=[A-Z])", "_").ToLowerInvariant();

        public static TEnum FromSnakeCase<TEnum>(string value) where TEnum : struct, System.Enum
        {
            var normalizedValue = Normalize(value);
            var names = System.Enum.GetNames<TEnum>();
            var name = names.FirstOrDefault(candidate =>
                string.Equals(Normalize(candidate), normalizedValue, StringComparison.OrdinalIgnoreCase));

            // Accept short database enum labels such as "cancel", "sign", and "expire".
            if (name is not null)
            {
                return name is not null
                    ? System.Enum.Parse<TEnum>(name)
                    : throw new ArgumentOutOfRangeException(nameof(value), value,
                        $"'{value}' does not match a {typeof(TEnum).Name} value. Accepted values are enum names, snake_case names, or an unambiguous enum-name prefix.");
            }
            {
                var matches = names.Where(candidate =>
                    Normalize(candidate).StartsWith(normalizedValue, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (matches.Length == 1) name = matches[0];
            }

            return name is not null
                ? System.Enum.Parse<TEnum>(name)
                : throw new ArgumentOutOfRangeException(nameof(value), value,
                    $"'{value}' does not match a {typeof(TEnum).Name} value. Accepted values are enum names, snake_case names, or an unambiguous enum-name prefix.");
        }

        private static string Normalize(string value) =>
            string.Concat(value.Where(char.IsLetterOrDigit));
    }
}
