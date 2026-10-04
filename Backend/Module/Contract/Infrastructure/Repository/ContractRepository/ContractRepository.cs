using Contract.Infrastructure.Persistence.DbContext;
using Contract.Interfaces.IRepository;
using Contract.Models.Contract;
using Contract.Utils.Enum;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Contract.Infrastructure.Repository.ContractRepository
{
    public class ContractRepository(
        ContractDbContext context,
        ILogger<ContractRepository> logger,
        ILogPool logPool) : IContractRepository
    {
        private const string Module = "Contract";

        private const string Layer = "Infrastructure/Repository/ContractRepository";

        private readonly ContractDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ContractRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<ContractModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all contracts.");
            return await _db.Contracts.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<ContractModel?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading contract by id.");
            return await _db.Contracts.AsNoTracking()
                .FirstOrDefaultAsync(contract => contract.ContractId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ContractModel>> GetByIdsAsync(IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading contracts by ids.");
            return await _db.Contracts.AsNoTracking()
                .Where(contract => ids.Contains(contract.ContractId))
                .ToListAsync(cancellationToken);
        }

        public async Task<ContractModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked contract by id.");
            return await _db.Contracts.FirstOrDefaultAsync(
                contract => contract.ContractId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking contract existence.");
            return await _db.Contracts.AnyAsync(
                contract => contract.ContractId == id, cancellationToken);
        }

        public async Task<ContractModel?> GetContractAndNavigationByIdAsync(Guid id,
            bool isGetViolations = true,
            bool isGetInvoices = false,
            bool isGetRegulations = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract with navigation properties.");
            var query = _db.Contracts.AsNoTracking()
                .AsSplitQuery()
                .Where(contract => contract.ContractId == id);

            if (isGetViolations)
            {
                query = query.Include(contract => contract.ContractViolations);
            }

            if (isGetInvoices)
            {
                query = query.Include(contract => contract.ContractInvoices);
            }

            if (isGetRegulations)
            {
                query = query.Include(contract => contract.ContractRegulations);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingAsync(Guid? cursor,
            int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading contract page.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract paging rejected because the page size is invalid."));
            var query = _db.Contracts.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(contract => contract.ContractId < cursor.Value);
            }

            query = query.OrderByDescending(contract => contract.ContractId).Take(pageSize + 1);
            var contracts = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                contracts, pageSize, contract => contract.ContractId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize, EContractStatus status, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading contract page by status.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract paging rejected because the page size is invalid."));
            var query = _db.Contracts.AsNoTracking()
                .Where(contract => contract.ContractStatus == status);

            if (cursor.HasValue)
            {
                query = query.Where(contract => contract.ContractId < cursor.Value);
            }

            query = query.OrderByDescending(contract => contract.ContractId).Take(pageSize + 1);
            var contracts = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                contracts, pageSize, contract => contract.ContractId, cancellationToken);
        }

        public async Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingWithNavigationAsync(Guid? cursor,
            int pageSize, bool isGetViolations = true, bool isGetInvoices = false,
            bool isGetRegulations = false, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract page with navigation properties.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract paging rejected because the page size is invalid."));
            var query = _db.Contracts.AsNoTracking().AsSplitQuery();

            if (isGetViolations)
            {
                query = query.Include(contract => contract.ContractViolations);
            }

            if (isGetInvoices)
            {
                query = query.Include(contract => contract.ContractInvoices);
            }

            if (isGetRegulations)
            {
                query = query.Include(contract => contract.ContractRegulations);
            }

            if (cursor.HasValue)
            {
                query = query.Where(contract => contract.ContractId < cursor.Value);
            }

            query = query.OrderByDescending(contract => contract.ContractId).Take(pageSize + 1);
            var contracts = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(
                contracts, pageSize, contract => contract.ContractId, cancellationToken);
        }
        
        #endregion

        #region POST

        public async Task AddAsync(ContractModel contractModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing contract creation.");
            if (contractModel.ContractAccountId == Guid.Empty)
            {
                var exception = new ArgumentException("ContractModel account id is required.",
                    nameof(contractModel.ContractAccountId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract creation rejected because the account id is missing.");
                throw exception;
            }

            await _db.Contracts.AddAsync(contractModel, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Contract staged for creation.");
        }

        public async Task AddRangeAsync(IEnumerable<ContractModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing contract range creation.");
            var contractModels = entities.ToList();
            await _db.Contracts.AddRangeAsync(contractModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract range staged for creation.");
        }

        public async Task UpdateAsync(ContractModel contractModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing contract update.");
            if (contractModel.ContractId == Guid.Empty)
            {
                var exception = new ArgumentException("ContractModel id is required.",
                    nameof(contractModel.ContractId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract update rejected because the contract id is missing.");
                throw exception;
            }

            var existedContract = await _db.Contracts.AsNoTracking()
                .FirstOrDefaultAsync(
                    contract => contract.ContractId == contractModel.ContractId,
                    cancellationToken);

            if (existedContract is null)
            {
                var exception = new InvalidOperationException(
                    $"ContractModel not found! \n {contractModel.ContractId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract update rejected because the contract was not found.");
                throw exception;
            }

            _db.Contracts.Update(contractModel);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Contract staged for update.");
        }

        public void UpdateRange(IEnumerable<ContractModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing contract range update.");
            var contractModels = entities.ToList();
            _db.Contracts.UpdateRange(contractModels);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract range staged for update.");
        }

        #endregion
    }
}
