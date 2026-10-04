namespace Shared.Logging
{
    public sealed record LogEntry(
        DateTimeOffset Timestamp,
        string Module,
        string File,
        string Layer,
        string Member,
        string Level,
        string Message,
        string? Exception);
}