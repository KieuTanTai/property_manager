using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Logging;

namespace Identity.Infrastructure.Repository.AccountRepository
{
    public class AccountRoleRepository(
        IdentityDbContext context,
        ILogger<AccountRoleRepository> logger,
        ILogPool logPool) : IBaseAssociativeRepository<AccountRoleModel, Guid>
    {
        private const string Module = "Identity";

        private const string Layer = "Infrastructure/Repository/AccountRepository";

        private readonly IdentityDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<AccountRoleRepository> _logger = logger;

        #region POST

        public async Task AddAsync(AccountRoleModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account role association creation.");
            if (entity.AccountId == Guid.Empty || entity.RoleId == Guid.Empty)
            {
                var exception = new ArgumentException("AccountModel and RoleModel id is required.", nameof(entity));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account role association rejected because an id is missing.");
                throw exception;
            }
            var existedAccountRole = await GetByIdAsync(entity.AccountId, entity.RoleId, cancellationToken);
            if (existedAccountRole is not null)
            {
                var exception = new InvalidOperationException("AccountModel role already exist!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account role association rejected because it already exists.");
                throw exception;
            }
            await _db.AccountRoles.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account role association staged for creation.");
        }

        public async Task AddRangeAsync(List<AccountRoleModel> entities, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account role association batch creation.");
            // var filteredAccountRoles = await GetNotExistedEntitiesList(entities, cancellationToken);
            // if (!filteredAccountRoles.Any())
            // {
            //     throw new InvalidOperationException("All account roles already exist!");
            // }
            // await _db.AccountRoles.AddRangeAsync(filteredAccountRoles, cancellationToken);
            await _db.AccountRoles.AddRangeAsync(entities, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account role associations staged for creation.");
        }

        #endregion

        //
        // private async Task<List<AccountRoleModel>> GetNotExistedEntitiesList(List<AccountRoleModel> entities, CancellationToken cancellationToken = default)
        // {
        //     var existedAccountRoles = await GetAllAsync(cancellationToken);
        //     return entities.Where(entity => !existedAccountRoles.Any(existedAccountRole =>
        //             existedAccountRole.AccountId == entity.AccountId && existedAccountRole.RoleId == entity.RoleId))
        //         .ToList();
        // }

        #region DELETE

        public async Task DeleteAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all account role associations.");
            if (firstForeignId == Guid.Empty || secondForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("First foreign id and second foreign id is required.", nameof(firstForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account role lookup rejected because an id is missing.");
                throw exception;
            }
            var existedAccountRole = await GetByIdAsync(firstForeignId, secondForeignId, cancellationToken);
            if (existedAccountRole is null)
            {
                var exception = new InvalidOperationException("AccountModel role not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account role association was not found.");
                throw exception;
            }
            _db.AccountRoles.Remove(existedAccountRole);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account role association staged for deletion.");
        }

        public async Task DeleteByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account role association.");
            if (firstForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("First foreign id is required.", nameof(firstForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account role deletion rejected because an id is missing.");
                throw exception;
            }
            var accountRolesToDelete = await _db.AccountRoles.Where(ar => ar.AccountId == firstForeignId).ToListAsync(cancellationToken);
            _db.AccountRoles.RemoveRange(accountRolesToDelete);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account role associations staged for deletion.");
        }

        public async Task DeleteBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account role association by role.");
            if (secondForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("Second foreign id is required.", nameof(secondForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account role deletion rejected because an id is missing.");
                throw exception;
            }
            var accountRolesToDelete = await _db.AccountRoles.Where(ar => ar.RoleId == secondForeignId).ToListAsync(cancellationToken);
            _db.AccountRoles.RemoveRange(accountRolesToDelete);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account role associations staged for deletion.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<AccountRoleModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all account role associations.");
            return await _db.AccountRoles.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccountRoleModel>> GetByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account role associations by account.");
            return await _db.AccountRoles.AsNoTracking().Where(ar => ar.AccountId == firstForeignId).ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccountRoleModel>> GetBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account role associations by role.");
            return await _db.AccountRoles.AsNoTracking().Where(ar => ar.RoleId == secondForeignId).ToListAsync(cancellationToken);
        }

        public async Task<AccountRoleModel?> GetByIdAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account role association by account and role.");
            return await _db.AccountRoles.AsNoTracking().FirstOrDefaultAsync(ar => ar.AccountId == firstForeignId && ar.RoleId == secondForeignId, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking account role association existence.");
            return await _db.AccountRoles.AnyAsync(ar => ar.AccountId == firstForeignId && ar.RoleId == secondForeignId, cancellationToken);
        }

        #endregion
    }
}
