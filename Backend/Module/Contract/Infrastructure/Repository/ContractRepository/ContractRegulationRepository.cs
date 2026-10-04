using Contract.Infrastructure.Persistence.DbContext;
using Contract.Interfaces.IRepository;
using Contract.Models.Contract;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using Shared.ModelHelper;

namespace Contract.Infrastructure.Repository.ContractRepository
{
    public class ContractRegulationRepository(
        ContractDbContext context,
        ILogger<ContractRegulationRepository> logger,
        ILogPool logPool) : IContractRegulationRepository
    {
        private const string Module = "Contract";

        private const string Layer = "Infrastructure/Repository/ContractRepository";

        private readonly ContractDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ContractRegulationRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<ContractRegulationModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading all contract regulation associations.");
            return await _db.ContractRegulations.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ContractRegulationModel>> GetByFirstForeignIdAsync(Guid contractId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract regulation associations by contract id.");
            return await _db.ContractRegulations.AsNoTracking()
                .Where(association => association.ContractId == contractId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ContractRegulationModel>> GetBySecondForeignIdAsync(Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract regulation associations by regulation id.");
            return await _db.ContractRegulations.AsNoTracking()
                .Where(association => association.RegulationId == regulationId)
                .ToListAsync(cancellationToken);
        }

        public async Task<ContractRegulationModel?> GetByIdAsync(Guid contractId, Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract regulation association by contract and regulation id.");
            return await _db.ContractRegulations.AsNoTracking()
                .FirstOrDefaultAsync(
                    association => association.ContractId == contractId
                                  && association.RegulationId == regulationId,
                    cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid contractId, Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Checking contract regulation association existence.");
            return await _db.ContractRegulations.AnyAsync(
                association => association.ContractId == contractId
                              && association.RegulationId == regulationId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(ContractRegulationModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing contract regulation association creation.");
            var validation = ModelFieldGuard.ValidateIds(entity.ContractId, new List<Guid> { entity.RegulationId });
            if (!validation)
            {
                var exception = new ArgumentException(
                    "ContractModel and RegulationModel id is required.", nameof(entity));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract regulation association creation rejected because an id is missing.");
                throw exception;
            }
            
            var existedAssociation = await GetByIdAsync(
                entity.ContractId, entity.RegulationId, cancellationToken);
            if (existedAssociation is not null)
            {
                var exception = new InvalidOperationException(
                    "ContractModel regulation already exists!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract regulation association creation rejected because it already exists.");
                throw exception;
            }

            await _db.ContractRegulations.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract regulation association staged for creation.");
        }

        public async Task AddRangeAsync(List<ContractRegulationModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing contract regulation association batch creation.");
            if (entities.Any(entity => entity.ContractId == Guid.Empty || entity.RegulationId == Guid.Empty))
            {
                var exception = new ArgumentException(
                    "ContractModel and RegulationModel id is required.", nameof(entities));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract regulation association batch rejected because an id is missing.");
                throw exception;
            }

            await _db.ContractRegulations.AddRangeAsync(entities, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract regulation associations staged for creation.");
        }

        #endregion

        #region DELETE

        public async Task DeleteByFirstForeignIdAsync(Guid contractId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract regulation associations by contract id for deletion.");
            if (contractId == Guid.Empty)
            {
                var exception = new ArgumentException("Contract id is required.", nameof(contractId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract regulation deletion rejected because the contract id is missing.");
                throw exception;
            }

            var associations = await _db.ContractRegulations
                .Where(association => association.ContractId == contractId)
                .ToListAsync(cancellationToken);
            _db.ContractRegulations.RemoveRange(associations);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract regulation associations staged for deletion.");
        }

        public async Task DeleteBySecondForeignIdAsync(Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading contract regulation associations by regulation id for deletion.");
            if (regulationId == Guid.Empty)
            {
                var exception = new ArgumentException("Regulation id is required.", nameof(regulationId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract regulation deletion rejected because the regulation id is missing.");
                throw exception;
            }

            var associations = await _db.ContractRegulations
                .Where(association => association.RegulationId == regulationId)
                .ToListAsync(cancellationToken);
            _db.ContractRegulations.RemoveRange(associations);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract regulation associations staged for deletion.");
        }

        public async Task DeleteAsync(Guid contractId, Guid regulationId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing contract regulation association deletion.");
            if (contractId == Guid.Empty || regulationId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "Contract id and regulation id are required.", nameof(contractId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract regulation deletion rejected because an id is missing.");
                throw exception;
            }

            var association = await GetByIdAsync(contractId, regulationId, cancellationToken);
            if (association is null)
            {
                var exception = new InvalidOperationException(
                    "ContractModel regulation association not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Contract regulation association deletion rejected because it was not found.");
                throw exception;
            }

            _db.ContractRegulations.Remove(association);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Contract regulation association staged for deletion.");
        }

        #endregion

    }
}
