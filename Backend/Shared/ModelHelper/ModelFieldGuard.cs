namespace Shared.ModelHelper
{
    public static class ModelFieldGuard
    {
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

        public static bool ValidateIds(Guid id, IReadOnlyList<Guid>? ids)
        {
            return id != Guid.Empty && ids is { Count: > 0 } && ids.All(gid => gid != Guid.Empty);
        }
    }
}