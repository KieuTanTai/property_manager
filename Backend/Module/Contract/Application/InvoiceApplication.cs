using Contract.Interfaces.IApplication;
using Contract.Interfaces.IRepository;
using Shared.Interfaces;
using Shared.Logging;

namespace Contract.Application
{
    public class InvoiceApplication(
        IUnitOfWork unitOfWork,
        IMonthlyInvoiceRepository invoiceRepository,
        IInvoiceDetailRepository invoiceDetailRepository,
        IReceiptRepository receiptRepository,
        ILogger<InvoiceApplication> logger,
        ILogPool logPool)
        : IInvoiceApplication
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMonthlyInvoiceRepository _invoiceRepository = invoiceRepository;
        private readonly IInvoiceDetailRepository _invoiceDetailRepository = invoiceDetailRepository;
        private readonly IReceiptRepository _receiptRepository = receiptRepository;
        private readonly ILogger<InvoiceApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        
        private const string Module = "Contract";
        private const string Layer = "Application";
    }
}