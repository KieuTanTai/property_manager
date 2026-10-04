using Contract.Models.Invoice;
using Shared.Interfaces;

namespace Contract.Interfaces.IRepository
{
    public interface IInvoiceDetailRepository : IBaseReadRepository<InvoiceDetailModel, int>, IBasePostRepository<InvoiceDetailModel>
    {
        Task<IReadOnlyList<InvoiceDetailModel>> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InvoiceDetailModel>> GetByPremiseIdAsync(Guid premiseId,
            CancellationToken cancellationToken = default);
        
    }
}
