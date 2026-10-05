using Contract.Infrastructure.Persistence.DbContext;
using Contract.Interfaces.IRepository;
using Contract.Models.Contract;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Contract.Infrastructure.Repository.ContractRepository
{
    public class ContractViolationRepository(
        ContractDbContext context,
        ILogger<ContractViolationRepository> logger,
        ILogPool logPool) : IContractViolationRepository
    {
        private const string Module = "Contract";
        private const string Layer = "Infrastructure/Repository/ContractRepository";

        private readonly ContractDbContext _db = context;
        private readonly ILogPool _logPool = logPool;
        private readonly ILogger<ContractViolationRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<ContractViolationModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all contract violations.");
            return await _db.ContractViolations.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<ContractViolationModel?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading contract violation by id.");
            return await _db.ContractViolations.AsNoTracking()
                .FirstOrDefaultAsync(violation => violation.ViolationId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ContractViolationModel>> GetByIdsAsync(IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract violations by ids.");
            return await _db.ContractViolations.AsNoTracking()
                .Where(violation => ids.Contains(violation.ViolationId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ContractViolationModel>> GetByContractIdAsync(Guid contractId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract violations by contract id.");
            return await _db.ContractViolations.AsNoTracking()
                .Where(violation => violation.ContractId == contractId)
                .ToListAsync(cancellationToken);
        }

        public async Task<ContractViolationModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading tracked contract violation by id.");
            return await _db.ContractViolations.FirstOrDefaultAsync(
                violation => violation.ViolationId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Checking contract violation existence.");
            return await _db.ContractViolations.AnyAsync(
                violation => violation.ViolationId == id, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingAsync(
            Guid? cursor, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = CreatePagingQuery(pageSize, "Loading contract violation page.");
            if (cursor.HasValue)
            {
                query = query.Where(violation => violation.ViolationId < cursor.Value);
            }

            return await ApplyPagingAsync(query, pageSize, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<ContractViolationModel>>
            GetApplyPagingByViolationContentAsync(
            Guid? cursor, int pageSize, string violationContent,
            CancellationToken cancellationToken = default)
        {
            var query = CreatePagingQuery(pageSize,
                "Loading contract violation page by content.")
                .Where(violation => violation.ViolationContent.Contains(violationContent));

            if (cursor.HasValue)
            {
                query = query.Where(violation => violation.ViolationId < cursor.Value);
            }

            return await ApplyPagingAsync(query, pageSize, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<ContractViolationModel>>
            GetApplyPagingByRangeViolationDateAsync(
                Guid? cursor, int pageSize, DateTime minViolationDate, DateTime maxViolationDate,
                CancellationToken cancellationToken = default)
        {
            var query = CreatePagingQuery(pageSize,
                "Loading contract violation page by violation date range.")
                .Where(violation => violation.ViolationDate >= minViolationDate
                                    && violation.ViolationDate <= maxViolationDate);

            if (cursor.HasValue)
            {
                query = query.Where(violation => violation.ViolationId < cursor.Value);
            }

            return await ApplyPagingAsync(query, pageSize, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByStatusAsync(
            Guid? cursor, int pageSize, bool isResolved,
            CancellationToken cancellationToken = default)
        {
            var query = CreatePagingQuery(pageSize,
                "Loading contract violation page by status.")
                .Where(violation => violation.ViolationIsResolved == isResolved);

            if (cursor.HasValue)
            {
                query = query.Where(violation => violation.ViolationId < cursor.Value);
            }

            return await ApplyPagingAsync(query, pageSize, cancellationToken);
        }

        private IQueryable<ContractViolationModel> CreatePagingQuery(int pageSize,
            string message)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, message);
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract violation paging rejected because the page size is invalid."));
            return _db.ContractViolations.AsNoTracking()
                .OrderByDescending(violation => violation.ViolationId);
        }

        private static async Task<RecordBaseCursorPage<ContractViolationModel>> ApplyPagingAsync(
            IQueryable<ContractViolationModel> query, int pageSize,
            CancellationToken cancellationToken)
        {
            return await SharedGetApplyPagingRepository.ApplyPaging(
                query.AsAsyncEnumerable(), pageSize, violation => violation.ViolationId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(ContractViolationModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing contract violation creation.");
            if (entity.ContractId == Guid.Empty)
            {
                throw new ArgumentException("ContractViolationModel contract id is required.",
                    nameof(entity.ContractId));
            }

            if (string.IsNullOrWhiteSpace(entity.ViolationContent))
            {
                throw new ArgumentException("ContractViolationModel content is required.",
                    nameof(entity.ViolationContent));
            }

            await _db.ContractViolations.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract violation staged for creation.");
        }

        public async Task AddRangeAsync(IEnumerable<ContractViolationModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing contract violation range creation.");
            var violations = entities.ToList();
            await _db.ContractViolations.AddRangeAsync(violations, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract violation range staged for creation.");
        }

        public async Task UpdateAsync(ContractViolationModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing contract violation update.");
            if (entity.ViolationId == Guid.Empty)
            {
                throw new ArgumentException("ContractViolationModel id is required.",
                    nameof(entity.ViolationId));
            }

            var existedViolation = await _db.ContractViolations.AsNoTracking()
                .FirstOrDefaultAsync(violation => violation.ViolationId == entity.ViolationId,
                    cancellationToken);
            if (existedViolation is null)
            {
                throw new InvalidOperationException(
                    $"ContractViolationModel not found! \n {entity.ViolationId}");
            }

            _db.ContractViolations.Update(entity);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract violation staged for update.");
        }

        public void UpdateRange(IEnumerable<ContractViolationModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing contract violation range update.");
            var violations = entities.ToList();
            _db.ContractViolations.UpdateRange(violations);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract violation range staged for update.");
        }

        #endregion
    }
}
