using Microsoft.EntityFrameworkCore;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Interfaces.Premise;
using Premise.Models.Premise;
using Shared.Logging;

namespace Premise.Infrastructure.Repository.PremiseRepository
{
    public class RentedPremiseRepository(
        PremiseDbContext context,
        ILogger<RentedPremiseRepository> logger,
        ILogPool logPool) : IRentedPremiseRepository
    {
        private const string Module = "Premise";
        private const string Layer = "Infrastructures/Repository/RentedPremiseRepository";

        private readonly PremiseDbContext _db = context;
        private readonly ILogPool _logPool = logPool;
        private readonly ILogger<RentedPremiseRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<RentedPremiseModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all rented premises.");
            return await _db.RentedPremiseModels.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RentedPremiseModel>> GetByPremiseIdAsync(
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading rental records by premise id.");
            return await _db.RentedPremiseModels.AsNoTracking()
                .Where(rental => rental.PremiseId == premiseId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RentedPremiseModel>> GetByContractIdAsync(
            Guid contractId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading rental records by contract id.");
            return await _db.RentedPremiseModels.AsNoTracking()
                .Where(rental => rental.ContractId == contractId)
                .ToListAsync(cancellationToken);
        }

        public async Task<RentedPremiseModel?> GetByIdsAsync(
            Guid contractId,
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading rental record by composite id.");
            return await _db.RentedPremiseModels.AsNoTracking()
                .FirstOrDefaultAsync(
                    rental => rental.ContractId == contractId && rental.PremiseId == premiseId,
                    cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            Guid contractId,
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking rented premise existence.");
            return await _db.RentedPremiseModels.AnyAsync(
                rental => rental.ContractId == contractId && rental.PremiseId == premiseId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(
            RentedPremiseModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing rented premise creation.");
            Validate(entity);

            if (await ExistsAsync(entity.ContractId, entity.PremiseId, cancellationToken))
            {
                var exception = new InvalidOperationException(
                    $"Rented premise already exists: {entity.ContractId}, {entity.PremiseId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Rented premise creation rejected because the association already exists.");
                throw exception;
            }

            await _db.RentedPremiseModels.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Rented premise staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<RentedPremiseModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing rented premise range creation.");
            var rentals = entities.ToList();
            var keys = new HashSet<(Guid ContractId, Guid PremiseId)>();

            foreach (var rental in rentals)
            {
                Validate(rental);
                if (!keys.Add((rental.ContractId, rental.PremiseId)) ||
                    await ExistsAsync(rental.ContractId, rental.PremiseId, cancellationToken))
                {
                    var exception = new InvalidOperationException(
                        $"Rented premise already exists: {rental.ContractId}, {rental.PremiseId}.");
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        "Rented premise range creation rejected because it contains an existing association.");
                    throw exception;
                }
            }

            await _db.RentedPremiseModels.AddRangeAsync(rentals, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Rented premise range staged for creation.");
        }

        public async Task DeleteAsync(
            Guid contractId,
            Guid premiseId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing rented premise deletion.");
            var rental = await _db.RentedPremiseModels.FirstOrDefaultAsync(
                item => item.ContractId == contractId && item.PremiseId == premiseId,
                cancellationToken);
            if (rental is null)
            {
                var exception = new InvalidOperationException(
                    $"Rented premise not found: {contractId}, {premiseId}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Rented premise deletion rejected because the association was not found.");
                throw exception;
            }

            _db.RentedPremiseModels.Remove(rental);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Rented premise staged for deletion.");
        }

        #endregion

        private void Validate(RentedPremiseModel entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (entity.ContractId != Guid.Empty && entity.PremiseId != Guid.Empty)
            {
                return;
            }

            var exception = new ArgumentException(
                "Both contract id and premise id are required.", nameof(entity));
            _logger.LogLayerError(_logPool, Module, Layer, exception,
                "Rented premise rejected because an id is missing.");
            throw exception;
        }
    }
}
