using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Logging;

namespace Identity.Infrastructure.Repository.AccountRepository
{
    public class AccountAdditionalPermissionRepository(
        IdentityDbContext context,
        ILogger<AccountAdditionalPermissionRepository> logger,
        ILogPool logPool) : IBaseAssociativeRepository<AccountAdditionalPermissionModel, Guid>
    {
        private const string Module = "Identity";

        private const string Layer = "Infrastructure/Repository/AccountRepository";

        private readonly IdentityDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<AccountAdditionalPermissionRepository> _logger = logger;

        #region POST

        public async Task AddAsync(AccountAdditionalPermissionModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account permission association creation.");
            if (entity.AccountId == Guid.Empty || entity.PermissionId == Guid.Empty)
            {
                var exception = new ArgumentException("AccountModel and PermissionModel id is required.", nameof(entity));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account permission association rejected because an id is missing.");
                throw exception;
            }
            var existedAccountPermission = await GetByIdAsync(entity.AccountId, entity.PermissionId, cancellationToken);
            if (existedAccountPermission is not null)
            {
                var exception = new InvalidOperationException("AccountModel permission already exist!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account permission association rejected because it already exists.");
                throw exception;
            }
            await _db.AccountPermissions.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account permission association staged for creation.");
        }

        public async Task AddRangeAsync(List<AccountAdditionalPermissionModel> entities, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account permission associations by account.");
            await _db.AccountPermissions.AddRangeAsync(entities, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account permission associations staged for creation.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<AccountAdditionalPermissionModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account permission association.");
            return await _db.AccountPermissions.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccountAdditionalPermissionModel>> GetByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking account permission association.");
            return await _db.AccountPermissions.AsNoTracking().Where(ap => ap.AccountId == firstForeignId).ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccountAdditionalPermissionModel>> GetBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account permission associations by permission.");
            return await _db.AccountPermissions.AsNoTracking().Where(ap => ap.PermissionId == secondForeignId).ToListAsync(cancellationToken);
        }

        public async Task<AccountAdditionalPermissionModel?> GetByIdAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account permission association.");
            return await _db.AccountPermissions.AsNoTracking().FirstOrDefaultAsync(ap => ap.AccountId == firstForeignId && ap.PermissionId == secondForeignId, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking account permission association.");
            return await _db.AccountPermissions.AnyAsync(ap => ap.AccountId == firstForeignId && ap.PermissionId == secondForeignId, cancellationToken);
        }

        #endregion

        #region DELETE

        public async Task DeleteByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account permission associations by account.");
            if (firstForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("First foreign id is required.", nameof(firstForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account permission deletion rejected because an id is missing.");
                throw exception;
            }
            var accountPermissionsToDelete = await _db.AccountPermissions.Where(ap => ap.AccountId == firstForeignId).ToListAsync(cancellationToken);
            _db.AccountPermissions.RemoveRange(accountPermissionsToDelete);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account permission associations staged for deletion.");
        }

        public async Task DeleteBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account permission associations by permission.");
            if (secondForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("Second foreign id is required.", nameof(secondForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account permission deletion rejected because an id is missing.");
                throw exception;
            }
            var accountPermissionsToDelete = await _db.AccountPermissions.Where(ap => ap.PermissionId == secondForeignId).ToListAsync(cancellationToken);
            _db.AccountPermissions.RemoveRange(accountPermissionsToDelete);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account permission associations staged for deletion.");
        }

        public async Task DeleteAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account permission association by account and permission.");
            if (firstForeignId == Guid.Empty || secondForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("First foreign id and second foreign id is required.", nameof(firstForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account permission deletion rejected because an id is missing.");
                throw exception;
            }
            var existedAccountPermission = await GetByIdAsync(firstForeignId, secondForeignId, cancellationToken);
            if (existedAccountPermission is null)
            {
                var exception = new InvalidOperationException("AccountModel permission not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account permission association was not found.");
                throw exception;
            }
            _db.AccountPermissions.Remove(existedAccountPermission);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account permission association staged for deletion.");
        }

        #endregion
    }
}
