using Contract.Models.Invoice;
using Shared.Enum;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IApplication
{
    public interface IReceiptApplication
    {
        Task<IReadOnlyList<ReceiptModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<ReceiptModel?> GetByIdAsync(Guid receiptId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReceiptModel>> GetByIdsAsync(IEnumerable<Guid> receiptIds,
            CancellationToken cancellationToken = default);

        Task<ReceiptModel?> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReceiptModel>> GetByAccountIdAsync(Guid accountId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReceiptModel>> GetByPaymentMethodAsync(
            EReceiptPaymentMethod paymentMethod,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ReceiptModel>> GetApplyPagingByPaymentDateAsync(
            Guid? cursor, int pageSize, DateTime paymentDate,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Guid receiptId,
            CancellationToken cancellationToken = default);

        Task<ReceiptModel> AddAsync(ReceiptModel receipt,
            CancellationToken cancellationToken = default);

        Task<ReceiptModel> UpdateAsync(Guid receiptId, Guid invoiceId, DateTime paymentDate, decimal amount, string? transactionInfo,
            CancellationToken cancellationToken = default);
    }
}
