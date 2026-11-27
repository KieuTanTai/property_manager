using System.Text.RegularExpressions;

namespace Shared.ModelHelper
{
    public static class ModelFieldGuard
    {
        public const string PremiseAreaPattern = @"^[0-9]+(?:\.[0-9]+)?$";

        public static string Required(
            string? value,
            int maxLength,
            string parameterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                value,
                parameterName);

            value = value.Trim();

            if (value.Length > maxLength)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"Value cannot exceed {maxLength} characters.");
            }

            return value;
        }

        public static string NumericArea(string? value, string parameterName)
        {
            value = Required(value, 10, parameterName);

            if (!Regex.IsMatch(value, PremiseAreaPattern, RegexOptions.CultureInvariant))
            {
                throw new ArgumentException(
                    "Premise area must contain only digits and an optional decimal point.",
                    parameterName);
            }

            return value;
        }

        public static bool ValidateIds(Guid id, IReadOnlyList<Guid>? ids)
        {
            return id != Guid.Empty && ids is { Count: > 0 } && ids.All(gid => gid != Guid.Empty);
        }
    }
}