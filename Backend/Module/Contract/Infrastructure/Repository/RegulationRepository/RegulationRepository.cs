using Contract.Infrastructure.Persistence.DbContext;
using Contract.Interfaces.IRepository;
using Contract.Models.Contract;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Contract.Infrastructure.Repository.RegulationRepository
{
    public class RegulationRepository(
        ContractDbContext context,
        ILogger<RegulationRepository> logger,
        ILogPool logPool) : IRegulationRepository
    {
        private const string Module = "Contract";

        private const string Layer = "Infrastructure/Repository/RegulationRepository";

        private readonly ContractDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<RegulationRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<RegulationModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all regulations.");
            return await _db.Regulations.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<RegulationModel?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation by id.");
            return await _db.Regulations.AsNoTracking()
                .FirstOrDefaultAsync(regulation => regulation.RegulationId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<RegulationModel>> GetByIdsAsync(IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulations by ids.");
            return await _db.Regulations.AsNoTracking()
                .Where(regulation => ids.Contains(regulation.RegulationId))
                .ToListAsync(cancellationToken);
        }

        public async Task<RegulationModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked regulation by id.");
            return await _db.Regulations.FirstOrDefaultAsync(
                regulation => regulation.RegulationId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking regulation existence.");
            return await _db.Regulations.AnyAsync(
                regulation => regulation.RegulationId == id, cancellationToken);
        }

        public async Task<RegulationModel> GetRegulationByNameAsync(string regulationName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation by name.");
            var regulation = await _db.Regulations.AsNoTracking()
                .FirstOrDefaultAsync(regulation => regulation.RegulationName == regulationName,
                    cancellationToken);
            if (regulation is null)
            {
                var exception = new InvalidOperationException("RegulationModel not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation lookup rejected because the regulation was not found by name.");
                throw exception;
            }

            return regulation;
        }

        public async Task<RegulationModel> GetTrackedRegulationByNameAsync(string regulationName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked regulation by name.");
            var regulation = await _db.Regulations.FirstOrDefaultAsync(
                regulation => regulation.RegulationName == regulationName, cancellationToken);
            if (regulation is null)
            {
                var exception = new InvalidOperationException("RegulationModel not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Tracked regulation lookup rejected because the regulation was not found by name.");
                throw exception;
            }

            return regulation;
        }

        public async Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation page.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation paging rejected because the page size is invalid."));
            var query = _db.Regulations.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(regulation => regulation.RegulationId < cursor.Value);
            }

            query = query.OrderByDescending(regulation => regulation.RegulationId).Take(pageSize + 1);
            var regulations = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                regulations, pageSize, regulation => regulation.RegulationId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByNameAsync(Guid? cursor,
            int pageSize, string regulationName, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation page by name.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation paging rejected because the page size is invalid."));
            var query = _db.Regulations.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(regulation => regulation.RegulationId < cursor.Value);
            }

            query = query.Where(regulation => regulation.RegulationName.Contains(regulationName));
            query = query.OrderByDescending(regulation => regulation.RegulationId).Take(pageSize + 1);
            var regulations = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                regulations, pageSize, regulation => regulation.RegulationId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize, bool isActive, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation page by status.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation paging rejected because the page size is invalid."));
            var query = _db.Regulations.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(regulation => regulation.RegulationId < cursor.Value);
            }

            query = query.Where(regulation => regulation.RegulationIsActive == isActive);
            query = query.OrderByDescending(regulation => regulation.RegulationId).Take(pageSize + 1);
            var regulations = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                regulations, pageSize, regulation => regulation.RegulationId, cancellationToken);
        }
        
        public async Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByRangeFineAmountAsync(Guid? cursor, int pageSize, decimal minFineAmount, decimal maxFineAmount,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading regulation page by range fine amount.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation paging rejected because the page size is invalid."));
            var query = _db.Regulations.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(regulation => regulation.RegulationId < cursor.Value);
            }

            query = query.Where(regulation => regulation.RegulationFineAmount >= minFineAmount && regulation.RegulationFineAmount <= maxFineAmount);
            query = query.OrderByDescending(regulation => regulation.RegulationId).Take(pageSize + 1);
            var regulations = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                regulations, pageSize, regulation => regulation.RegulationId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByRangeCreatedAtAsync(
            Guid? cursor, int pageSize, DateTime minCreatedAt, DateTime maxCreatedAt,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading regulation page by created at range.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation paging rejected because the page size is invalid."));
            var query = _db.Regulations.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(regulation => regulation.RegulationId < cursor.Value);
            }

            query = query.Where(regulation => regulation.RegulationCreatedAt >= minCreatedAt
                                              && regulation.RegulationCreatedAt <= maxCreatedAt);
            query = query.OrderByDescending(regulation => regulation.RegulationId).Take(pageSize + 1);
            var regulations = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                regulations, pageSize, regulation => regulation.RegulationId, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(RegulationModel regulationModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing regulation creation.");
            if (string.IsNullOrWhiteSpace(regulationModel.RegulationName))
            {
                var exception = new ArgumentException("RegulationModel name is required.",
                    nameof(regulationModel.RegulationName));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation creation rejected because the regulation name is missing.");
                throw exception;
            }

            var isExisted = await _db.Regulations.AnyAsync(
                existedRegulation => existedRegulation.RegulationName == regulationModel.RegulationName,
                cancellationToken);

            if (isExisted)
            {
                var exception = new InvalidOperationException($"Already existed! \n {regulationModel.RegulationName}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation creation rejected because the regulation already exists.");
                throw exception;
            }

            await _db.Regulations.AddAsync(regulationModel, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Regulation staged for creation.");
        }

        public async Task UpdateAsync(RegulationModel regulationModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing regulation update.");
            if (regulationModel.RegulationId == Guid.Empty)
            {
                var exception = new ArgumentException("RegulationModel id is required.",
                    nameof(regulationModel.RegulationId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation update rejected because the regulation identifier is missing.");
                throw exception;
            }

            var existedRegulation = await _db.Regulations.AsNoTracking()
                .FirstOrDefaultAsync(
                    regulation => regulation.RegulationId == regulationModel.RegulationId,
                    cancellationToken);

            if (existedRegulation is null)
            {
                var exception = new InvalidOperationException(
                    $"RegulationModel not found! \n {regulationModel.RegulationId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Regulation update rejected because the regulation was not found.");
                throw exception;
            }

            if (existedRegulation.RegulationName != regulationModel.RegulationName)
            {
                var isExisted = await _db.Regulations.AnyAsync(
                    regulation => regulation.RegulationName == regulationModel.RegulationName
                                  && regulation.RegulationId != regulationModel.RegulationId,
                    cancellationToken);

                if (isExisted)
                {
                    var exception = new InvalidOperationException(
                        $"Already existed! \n {regulationModel.RegulationName}");
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Regulation update rejected because the regulation name already exists.");
                    throw exception;
                }
            }

            _db.Regulations.Update(regulationModel);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Regulation staged for update.");
        }

        #endregion

    }
}
