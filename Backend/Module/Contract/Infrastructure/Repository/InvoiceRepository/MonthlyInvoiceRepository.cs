using Contract.Infrastructure.Persistence.DbContext;
using Contract.Interfaces.IRepository;
using Contract.Models.Invoice;
using Contract.Utils.Enum;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Contract.Infrastructure.Repository.InvoiceRepository
{
    public class MonthlyInvoiceRepository(
        ContractDbContext context,
        ILogger<MonthlyInvoiceRepository> logger,
        ILogPool logPool) : IMonthlyInvoiceRepository
    {
        private const string Module = "Contract";
        
        private const string Layer = "Infrastructure/Repository/InvoiceRepository";
        
        private readonly ContractDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<MonthlyInvoiceRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<MonthlyInvoiceModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all monthly invoices.");
            return await _db.MonthlyInvoices.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<MonthlyInvoiceModel?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading monthly invoice by id.");
            return await _db.MonthlyInvoices.AsNoTracking()
                .FirstOrDefaultAsync(invoice => invoice.InvoiceId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<MonthlyInvoiceModel>> GetByIdsAsync(IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading monthly invoices by ids.");
            return await _db.MonthlyInvoices.AsNoTracking()
                .Where(invoice => ids.Contains(invoice.InvoiceId))
                .ToListAsync(cancellationToken);
        }

        public async Task<MonthlyInvoiceModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading tracked monthly invoice by id.");
            return await _db.MonthlyInvoices.FirstOrDefaultAsync(
                invoice => invoice.InvoiceId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Checking monthly invoice existence.");
            return await _db.MonthlyInvoices.AnyAsync(
                invoice => invoice.InvoiceId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<MonthlyInvoiceModel>> GetByContractIdAsync(Guid contractId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading monthly invoices by contract id.");
            return await _db.MonthlyInvoices.AsNoTracking()
                .Where(invoice => invoice.InvoiceContractId == contractId)
                .ToListAsync(cancellationToken);
        }

        public async Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingAsync(Guid? cursor,
            int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading monthly invoice page.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Monthly invoice paging rejected because the page size is invalid."));
            var query = _db.MonthlyInvoices.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(invoice => invoice.InvoiceId < cursor.Value);
            }

            query = query.OrderByDescending(invoice => invoice.InvoiceId).Take(pageSize + 1);
            var invoices = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                invoices, pageSize, invoice => invoice.InvoiceId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingByStatusAsync(
            Guid? cursor, int pageSize, EInvoiceStatus status,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading monthly invoice page by status.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Monthly invoice paging rejected because the page size is invalid."));
            var query = _db.MonthlyInvoices.AsNoTracking()
                .Where(invoice => invoice.InvoiceStatus == status);

            if (cursor.HasValue)
            {
                query = query.Where(invoice => invoice.InvoiceId < cursor.Value);
            }

            query = query.OrderByDescending(invoice => invoice.InvoiceId).Take(pageSize + 1);
            var invoices = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                invoices, pageSize, invoice => invoice.InvoiceId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<MonthlyInvoiceModel>> GetApplyPagingWithNavigationAsync(
            Guid? cursor, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading monthly invoice page with navigation properties.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Monthly invoice paging rejected because the page size is invalid."));
            var query = _db.MonthlyInvoices.AsNoTracking()
                .AsSplitQuery()
                .AsQueryable();
            query = query
                .Include(invoice => invoice.InvoiceDetails)
                .Include(invoice => invoice.InvoiceReceipt);

            if (cursor.HasValue)
            {
                query = query.Where(invoice => invoice.InvoiceId < cursor.Value);
            }

            query = query.OrderByDescending(invoice => invoice.InvoiceId).Take(pageSize + 1);
            var invoices = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                invoices, pageSize, invoice => invoice.InvoiceId, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(MonthlyInvoiceModel invoiceModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing monthly invoice creation.");
            if (invoiceModel.InvoiceContractId == Guid.Empty)
            {
                var exception = new ArgumentException("MonthlyInvoiceModel contract id is required.",
                    nameof(invoiceModel.InvoiceContractId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Monthly invoice creation rejected because the contract id is missing.");
                throw exception;
            }

            await _db.MonthlyInvoices.AddAsync(invoiceModel, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Monthly invoice staged for creation.");
        }

        public async Task AddRangeAsync(IEnumerable<MonthlyInvoiceModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing monthly invoice range creation.");
            var invoiceModels = entities.ToList();
            await _db.MonthlyInvoices.AddRangeAsync(invoiceModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Monthly invoice range staged for creation.");
        }

        public async Task UpdateAsync(MonthlyInvoiceModel invoiceModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing monthly invoice update.");
            if (invoiceModel.InvoiceId == Guid.Empty)
            {
                var exception = new ArgumentException("MonthlyInvoiceModel id is required.",
                    nameof(invoiceModel.InvoiceId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Monthly invoice update rejected because the invoice id is missing.");
                throw exception;
            }

            var existedInvoice = await _db.MonthlyInvoices.AsNoTracking()
                .FirstOrDefaultAsync(invoice => invoice.InvoiceId == invoiceModel.InvoiceId,
                    cancellationToken);
            if (existedInvoice is null)
            {
                var exception = new InvalidOperationException(
                    $"MonthlyInvoiceModel not found! \n {invoiceModel.InvoiceId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Monthly invoice update rejected because the invoice was not found.");
                throw exception;
            }

            _db.MonthlyInvoices.Update(invoiceModel);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Monthly invoice staged for update.");
        }

        public void UpdateRange(IEnumerable<MonthlyInvoiceModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing monthly invoice range update.");
            var invoiceModels = entities.ToList();
            _db.MonthlyInvoices.UpdateRange(invoiceModels);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Monthly invoice range staged for update.");
        }

        #endregion
    }
}
