namespace Shared.Logging;

public interface ILogPool
{
    void Add(
        string module,
        string layer,
        string file,
        string member,
        string level,
        string message,
        Exception? exception = null);

    IReadOnlyList<LogEntry> Fetch();

    Task<int> FlushAsync(
        string module,
        string layer,
        CancellationToken cancellationToken = default);
}
