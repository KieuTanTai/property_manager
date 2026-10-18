using Contract.Models.Invoice;
using Contract.Utils.Enum;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IRepository
{
    public interface IMonthlyInvoiceRepository : IBaseReadRepository<MonthlyInvoiceModel, Guid>, IBaseUpsertRepository<MonthlyInvoiceModel>
    {
        Task<IReadOnlyList<MonthlyInvoiceModel>> GetAllWithNavigationAsync(bool isIncludedDetails = true,
            bool isIncludedReceipt = true, CancellationToken cancellationToken = default);

        Task<MonthlyInvoiceModel?> GetByIdWithNavigationAsync(Guid invoiceId, bool isIncludedDetails = true,
            bool isIncludedReceipt = true,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MonthlyInvoiceModel>> GetByContractIdAsync(Guid contractId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MonthlyInvoiceModel>> GetByContractIdWithNavigationAsync(Guid contractId,
            bool isIncludedDetails = true, bool isIncludedReceipt = true,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            bool isIncludedDetails = true, bool isIncludedReceipt = true,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize,
            EInvoiceStatus status, bool isIncludedDetails = true, bool isIncludedReceipt = true,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingWithNavigationAsync(Guid? cursor,
            int pageSize, CancellationToken cancellationToken = default);
    }
}