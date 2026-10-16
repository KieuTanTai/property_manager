using Contract.Models.Invoice;
using Shared.Enum;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IApplication
{
    public interface IReceiptApplication
    {
        Task<IReadOnlyList<ReceiptModel>> GetAllReceiptsAsync(
            CancellationToken cancellationToken = default);

        Task<ReceiptModel?> GetByReceiptIdAsync(Guid receiptId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReceiptModel>> GetReceiptsByIdsAsync(IEnumerable<Guid> receiptIds,
            CancellationToken cancellationToken = default);

        Task<ReceiptModel?> GetReceiptByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReceiptModel>> GetReceiptsByAccountIdAsync(Guid accountId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReceiptModel>> GetReceiptsByPaymentMethodAsync(
            EReceiptPaymentMethod paymentMethod,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ReceiptModel>> GetApplyPagingByReceiptPaymentDateAsync(
            Guid? cursor, int pageSize, DateTime paymentDate,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Guid receiptId,
            CancellationToken cancellationToken = default);

        Task<ReceiptModel> AddReceiptAsync(ReceiptModel receipt,
            CancellationToken cancellationToken = default);

        Task<ReceiptModel> UpdateReceiptAsync(Guid receiptId, Guid invoiceId, DateTime paymentDate, decimal amount, string? transactionInfo,
            CancellationToken cancellationToken = default);
    }
}
