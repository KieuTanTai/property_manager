using Shared.Persistence.Record;

namespace Shared.Persistence
{
    public static class SharedGetApplyPagingRepository
    {
        public static void ValidatePageSize(int pageSize,
            Action<ArgumentOutOfRangeException>? onInvalid = null)
        {
            if (pageSize >= 1)
            {
                return;
            }

            var exception = new ArgumentOutOfRangeException(
                nameof(pageSize), pageSize, "Page size must be greater than zero.");
            onInvalid?.Invoke(exception);
            throw exception;
        }

        // generic method get by cursor
        public static async Task<RecordBaseCursorPage<T>> ApplyPaging<T>(IAsyncEnumerable<T> source,
            int pageSize, Func<T, Guid> nextCursorFunc, CancellationToken cancellationToken = default)
            where T : class
        {
            ValidatePageSize(pageSize);
            var items = await source.Take(pageSize + 1).ToListAsync(cancellationToken);
            var isHasMore = items.Count > pageSize;
            if (isHasMore)
            {
                items.RemoveAt(items.Count - 1);
            }

            Guid? nextCursor = isHasMore ? nextCursorFunc(items[^1]) : null;
            return new RecordBaseCursorPage<T>(items, nextCursor, isHasMore);
        }
    }
}
