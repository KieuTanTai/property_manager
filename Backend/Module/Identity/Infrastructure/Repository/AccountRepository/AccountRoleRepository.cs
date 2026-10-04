using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;
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
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account role association rejected because an id is missing.");
                throw new ArgumentException("AccountModel and RoleModel id is required.", nameof(entity));
            }
            var existedAccountRole = await GetByIdAsync(entity.AccountId, entity.RoleId, cancellationToken);
            if (existedAccountRole is not null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account role association rejected because it already exists.");
                throw new InvalidOperationException("AccountModel role already exist!");
            }
            await _db.AccountRoles.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(List<AccountRoleModel> entities, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account role association batch creation.");
            if (entities.Count == 0)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account role association batch rejected because it is empty.");
                throw new ArgumentException("Entities is required.", nameof(entities));
            }
            // var filteredAccountRoles = await GetNotExistedEntitiesList(entities, cancellationToken);
            // if (!filteredAccountRoles.Any())
            // {
            //     throw new InvalidOperationException("All account roles already exist!");
            // }
            // await _db.AccountRoles.AddRangeAsync(filteredAccountRoles, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account role associations staged for creation.");
            await _db.AccountRoles.AddRangeAsync(entities, cancellationToken);
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
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account role lookup rejected because an id is missing.");
                throw new ArgumentException("First foreign id and second foreign id is required.", nameof(firstForeignId));
            }
            var existedAccountRole = await GetByIdAsync(firstForeignId, secondForeignId, cancellationToken);
            if (existedAccountRole is null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account role association was not found.");
                throw new InvalidOperationException("AccountModel role not found!");
            }
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account role association deletion.");
            _db.AccountRoles.Remove(existedAccountRole);
        }

        public async Task DeleteByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account role association.");
            if (firstForeignId == Guid.Empty)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account role deletion rejected because an id is missing.");
                throw new ArgumentException("First foreign id is required.", nameof(firstForeignId));
            }
            var accountRolesToDelete = await _db.AccountRoles.Where(ar => ar.AccountId == firstForeignId).ToListAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account role association staged for deletion.");
            _db.AccountRoles.RemoveRange(accountRolesToDelete);
        }

        public async Task DeleteBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            if (secondForeignId == Guid.Empty)
            {
                throw new ArgumentException("Second foreign id is required.", nameof(secondForeignId));
            }
            var accountRolesToDelete = await _db.AccountRoles.Where(ar => ar.RoleId == secondForeignId).ToListAsync(cancellationToken);
            _db.AccountRoles.RemoveRange(accountRolesToDelete);
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<AccountRoleModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.AccountRoles.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccountRoleModel>> GetByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.AccountRoles.AsNoTracking().Where(ar => ar.AccountId == firstForeignId).ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccountRoleModel>> GetBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.AccountRoles.AsNoTracking().Where(ar => ar.RoleId == secondForeignId).ToListAsync(cancellationToken);
        }

        public async Task<AccountRoleModel?> GetByIdAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.AccountRoles.AsNoTracking().FirstOrDefaultAsync(ar => ar.AccountId == firstForeignId && ar.RoleId == secondForeignId, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.AccountRoles.AnyAsync(ar => ar.AccountId == firstForeignId && ar.RoleId == secondForeignId, cancellationToken);
        }

        #endregion
    }
}