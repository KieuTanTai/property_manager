using Contract.Infrastructure.Persistence.DbContext;
using Contract.Interfaces.IRepository;
using Contract.Models.Invoice;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;

namespace Contract.Infrastructure.Repository.InvoiceRepository
{
    public class InvoiceDetailRepository(
        ContractDbContext context,
        ILogger<InvoiceDetailRepository> logger,
        ILogPool logPool) : IInvoiceDetailRepository
    {
        private const string Module = "Contract";

        private const string Layer = "Infrastructure/Repository/MonthlyInvoiceRepository/InvoiceDetailRepository";

        private readonly ContractDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<InvoiceDetailRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<InvoiceDetailModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all invoice details.");
            return await _db.InvoiceDetails.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<InvoiceDetailModel?> GetByIdAsync(int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading invoice detail by id.");
            return await _db.InvoiceDetails.AsNoTracking()
                .FirstOrDefaultAsync(detail => detail.InvoiceDetailId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<InvoiceDetailModel>> GetByIdsAsync(IEnumerable<int> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading invoice details by ids.");
            return await _db.InvoiceDetails.AsNoTracking()
                .Where(detail => ids.Contains(detail.InvoiceDetailId))
                .ToListAsync(cancellationToken);
        }

        public async Task<InvoiceDetailModel?> GetTrackedByIdAsync(int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading tracked invoice detail by id.");
            return await _db.InvoiceDetails.FirstOrDefaultAsync(
                detail => detail.InvoiceDetailId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Checking invoice detail existence.");
            return await _db.InvoiceDetails.AnyAsync(
                detail => detail.InvoiceDetailId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<InvoiceDetailModel>> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading invoice details by invoice id.");
            return await _db.InvoiceDetails.AsNoTracking()
                .Where(detail => detail.InvoiceDetailInvoiceId == invoiceId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<InvoiceDetailModel>> GetByPremiseIdAsync(Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading invoice details by premise id.");
            return await _db.InvoiceDetails.AsNoTracking()
                .Where(detail => detail.InvoiceDetailPremiseId == premiseId)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(InvoiceDetailModel detailModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing invoice detail creation.");
            if (detailModel.InvoiceDetailInvoiceId == Guid.Empty
                || detailModel.InvoiceDetailPremiseId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "MonthlyInvoiceModel and PremiseModel id are required.", nameof(detailModel));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Invoice detail creation rejected because an id is missing.");
                throw exception;
            }

            await _db.InvoiceDetails.AddAsync(detailModel, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Invoice detail staged for creation.");
        }

        public async Task AddRangeAsync(IEnumerable<InvoiceDetailModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing invoice detail range creation.");
            var detailModels = entities.ToList();
            await _db.InvoiceDetails.AddRangeAsync(detailModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Invoice detail range staged for creation.");
        }

        public async Task UpdateAsync(InvoiceDetailModel detailModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing invoice detail update.");
            if (detailModel.InvoiceDetailId <= 0)
            {
                var exception = new ArgumentException("InvoiceDetailModel id is required.",
                    nameof(detailModel.InvoiceDetailId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Invoice detail update rejected because the detail id is missing.");
                throw exception;
            }

            var existedDetail = await _db.InvoiceDetails.AsNoTracking()
                .FirstOrDefaultAsync(
                    detail => detail.InvoiceDetailId == detailModel.InvoiceDetailId,
                    cancellationToken);
            if (existedDetail is null)
            {
                var exception = new InvalidOperationException(
                    $"InvoiceDetailModel not found! \n {detailModel.InvoiceDetailId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Invoice detail update rejected because the detail was not found.");
                throw exception;
            }

            _db.InvoiceDetails.Update(detailModel);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Invoice detail staged for update.");
        }

        public void UpdateRange(IEnumerable<InvoiceDetailModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing invoice detail range update.");
            var detailModels = entities.ToList();
            _db.InvoiceDetails.UpdateRange(detailModels);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Invoice detail range staged for update.");
        }

        #endregion
    }
}
