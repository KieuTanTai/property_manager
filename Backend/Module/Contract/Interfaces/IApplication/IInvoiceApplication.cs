using Contract.Models.Invoice;
using Contract.Utils.Enum;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IApplication
{
    public interface IInvoiceApplication
    {
        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingAsync(Guid? cursor,
            int pageSize, bool isGetDetails = true, bool isGetReceipt = true,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingByInvoiceStatusAsync(Guid? cursor,
            int pageSize, EInvoiceStatus status, bool isGetDetails = true, bool isGetReceipt = true,
            CancellationToken cancellationToken = default);

        Task<MonthlyInvoiceModel?> GetByInvoiceIdAsync(Guid invoiceId, bool isGetDetails = true,
            bool isGetReceipt = true, CancellationToken cancellationToken = default);
        
        Task<MonthlyInvoiceModel> AddInvoiceAsync(MonthlyInvoiceModel invoice,
            IReadOnlyCollection<InvoiceDetailModel> details,
            CancellationToken cancellationToken = default);

        Task<MonthlyInvoiceModel> UpdateInvoiceAsync(MonthlyInvoiceModel invoice,
            IReadOnlyCollection<InvoiceDetailModel>? details = null,
            CancellationToken cancellationToken = default);
        
    }
}
