using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace Shared.Logging
{
    public sealed class LogPool(IHostEnvironment environment) : ILogPool
    {
        private readonly ConcurrentQueue<LogEntry> _entries = new();

        public void Add(
            string module,
            string layer,
            string file,
            string member,
            string level,
            string message,
            Exception? exception = null)
        {
            _entries.Enqueue(new LogEntry(
                DateTimeOffset.UtcNow,
                module,
                Path.GetFileName(file),
                layer,
                member,
                level,
                message,
                exception?.GetType().Name));
        }

        public IReadOnlyList<LogEntry> Fetch()
        {
            return [.. _entries];
        }

        public async Task<int> FlushAsync(
            string module,
            string layer,
            CancellationToken cancellationToken = default)
        {
            var entries = _entries
                .Where(entry => string.Equals(entry.Module, module, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (entries.Count == 0)
            {
                return 0;
            }

            var entrySet = entries.ToHashSet();
            var remaining = new ConcurrentQueue<LogEntry>(
                _entries.Where(entry => !entrySet.Contains(entry)));
            while (_entries.TryDequeue(out _))
            {
            }
            foreach (var entry in remaining)
            {
                _entries.Enqueue(entry);
            }

            var logDirectory = Path.Combine(
                environment.ContentRootPath,
                "..", "..", "..", "docs", "backend", "modules", module);
            Directory.CreateDirectory(logDirectory);
            var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
            var logFile = Path.Combine(logDirectory, $"{module.ToLowerInvariant()}_{date}.jsonl");

            await using var stream = new FileStream(
                logFile, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true);
            await using var writer = new StreamWriter(stream);
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = JsonSerializer.Serialize(entry);
                await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
            }

            return entries.Count;
        }
    }
}
