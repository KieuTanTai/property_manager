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

        Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize, EInvoiceStatus status, bool isGetDetails = true, bool isGetReceipt = true,
            CancellationToken cancellationToken = default);

        Task<MonthlyInvoiceModel?> GetByIdAsync(Guid invoiceId, bool isGetDetails = true,
            bool isGetReceipt = true, CancellationToken cancellationToken = default);
        
        Task<MonthlyInvoiceModel> AddAsync(MonthlyInvoiceModel invoice,
            IReadOnlyCollection<InvoiceDetailModel> details,
            CancellationToken cancellationToken = default);

        Task<MonthlyInvoiceModel> UpdateAsync(MonthlyInvoiceModel invoice,
            IReadOnlyCollection<InvoiceDetailModel>? details = null,
            CancellationToken cancellationToken = default);
        
        Task<ReceiptModel> AddReceiptAsync(ReceiptModel receipt,
            CancellationToken cancellationToken = default);

        Task<ReceiptModel> UpdateReceiptAsync(ReceiptModel receipt,
            CancellationToken cancellationToken = default);
    }
}
