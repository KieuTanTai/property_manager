using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace Shared.Logging
{
    public sealed class LogPool(IHostEnvironment environment) : ILogPool
    {
        private readonly object _entriesLock = new();

        private readonly SemaphoreSlim _flushLock = new(1, 1);

        private Queue<LogEntry> _entries = new();

        public void Add(
            string module,
            string layer,
            string file,
            string member,
            string level,
            string message,
            Exception? exception = null)
        {
            var entry = new LogEntry(
                DateTimeOffset.UtcNow,
                module,
                Path.GetFileName(file),
                layer,
                member,
                level,
                message,
                exception?.GetType().Name);
            lock (_entriesLock)
            {
                _entries.Enqueue(entry);
            }
        }

        public IReadOnlyList<LogEntry> Fetch()
        {
            lock (_entriesLock)
            {
                return [.. _entries];
            }
        }

        public async Task<int> FlushAsync(
            string module,
            string layer,
            CancellationToken cancellationToken = default)
        {
            lock (_entriesLock)
            {
                if (!_entries.Any(entry => string.Equals(
                        entry.Module, module, StringComparison.OrdinalIgnoreCase)))
                {
                    return 0;
                }
            }

            var logDirectory = Path.Combine(
                environment.ContentRootPath,
                "..", "..", "..", "docs", "backend", "modules", module);
            Directory.CreateDirectory(logDirectory);
            var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
            var logFile = Path.Combine(logDirectory, $"{module.ToLowerInvariant()}_{date}.jsonl");

            await _flushLock.WaitAsync(cancellationToken);
            try
            {
                await using var stream = await OpenLogFileForAppendAsync(logFile, cancellationToken);
                LogEntry[] entries;
                lock (_entriesLock)
                {
                    entries = _entries
                        .Where(entry => string.Equals(entry.Module, module, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                }

                if (entries.Length == 0)
                {
                    return 0;
                }

                var originalLength = stream.Length;
                try
                {
                    stream.Position = originalLength;
                    await using (var writer = new StreamWriter(stream, leaveOpen: true))
                    {
                        foreach (var entry in entries)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await writer.WriteLineAsync(JsonSerializer.Serialize(entry).AsMemory(), cancellationToken);
                        }
                        await writer.FlushAsync(cancellationToken);
                    }
                }
                catch
                {
                    stream.SetLength(originalLength);
                    throw;
                }

                var entrySet = entries.ToHashSet(ReferenceEqualityComparer.Instance);
                lock (_entriesLock)
                {
                    _entries = new Queue<LogEntry>(_entries.Where(entry => !entrySet.Contains(entry)));
                }

                return entries.Length;
            }
            finally
            {
                _flushLock.Release();
            }
        }

        private static async Task<FileStream> OpenLogFileForAppendAsync(
            string logFile,
            CancellationToken cancellationToken)
        {
            const int maxAttempts = 30;
            for (var attempt = 0;; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return new FileStream(
                        logFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, true);
                }
                catch (IOException exception) when (IsFileLock(exception) && attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(Math.Min(50 + attempt * 25, 500)), cancellationToken);
                }
            }
        }

        private static bool IsFileLock(IOException exception)
        {
            var errorCode = exception.HResult & 0xFFFF;
            return errorCode is 11 or 32 or 33 or 35;
        }
    }
}