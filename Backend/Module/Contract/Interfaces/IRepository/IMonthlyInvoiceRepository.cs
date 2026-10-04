using Contract.Models.Invoice;
using Contract.Utils.Enum;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IRepository
{
    public interface IMonthlyInvoiceRepository : IBaseReadRepository<MonthlyInvoiceModel, Guid>, IBasePostRepository<MonthlyInvoiceModel>
    {
        Task<IReadOnlyList<MonthlyInvoiceModel>> GetByContractIdAsync(Guid contractId,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize,
            EInvoiceStatus status, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingWithNavigationAsync(Guid? cursor,
            int pageSize, CancellationToken cancellationToken = default);
    }
}
