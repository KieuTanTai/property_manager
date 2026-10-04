using Contract.Models.Invoice;
using Shared.Enum;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IRepository
{
    public interface IReceiptRepository : IBaseReadRepository<ReceiptModel, Guid>, IBasePostRepository<ReceiptModel>
    {
        Task<ReceiptModel?> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReceiptModel>> GetByAccountIdAsync(Guid accountId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReceiptModel>> GetByPaymentMethodAsync(EReceiptPaymentMethod paymentMethod,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ReceiptModel>> GetApplyPagingByPaymentDateAsync(Guid? cursor,
            int pageSize, DateTime paymentDate, CancellationToken cancellationToken = default);
    }
}
