using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repository.AccountRepository
{
    public class AccountAdditionalPermissionRepository(IdentityDbContext context) : IBaseAssociativeRepository<AccountAdditionalPermissionModel, Guid>
    {
        private readonly IdentityDbContext _db = context;

        #region POST

        public async Task AddAsync(AccountAdditionalPermissionModel entity, CancellationToken cancellationToken = default)
        {
            if (entity.AccountId == Guid.Empty || entity.PermissionId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(entity), "Entity is required.");
            }
            var existedAccountPermission = await GetByIdAsync(entity.AccountId, entity.PermissionId, cancellationToken);
            if (existedAccountPermission is not null)
            {
                throw new InvalidOperationException("AccountModel permission already exist!");
            }
            await _db.AccountPermissions.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(List<AccountAdditionalPermissionModel> entities, CancellationToken cancellationToken = default)
        {
            if (entities is null || entities.Count == 0)
            {
                throw new ArgumentException("Entities is required.", nameof(entities));
            }
            await _db.AccountPermissions.AddRangeAsync(entities, cancellationToken);
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<AccountAdditionalPermissionModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.AccountPermissions.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AccountAdditionalPermissionModel>> GetByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.AccountPermissions.AsNoTracking().Where(ap => ap.AccountId == firstForeignId).ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<AccountAdditionalPermissionModel>> GetBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.AccountPermissions.AsNoTracking().Where(ap => ap.PermissionId == secondForeignId).ToListAsync(cancellationToken);
        }
        
        public async Task<AccountAdditionalPermissionModel?> GetByIdAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.AccountPermissions.AsNoTracking().FirstOrDefaultAsync(ap => ap.AccountId == firstForeignId && ap.PermissionId == secondForeignId, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.AccountPermissions.AnyAsync(ap => ap.AccountId == firstForeignId && ap.PermissionId == secondForeignId, cancellationToken);
        }

        #endregion

        #region DELETE

        public async Task DeleteByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            if (firstForeignId == Guid.Empty)
            {
                throw new ArgumentException("First foreign id is required.", nameof(firstForeignId));
            }
            var accountPermissionsToDelete = await _db.AccountPermissions.Where(ap => ap.AccountId == firstForeignId).ToListAsync(cancellationToken);
            _db.AccountPermissions.RemoveRange(accountPermissionsToDelete);
        }

        public async Task DeleteBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            if (secondForeignId == Guid.Empty)
            {
                throw new ArgumentException("Second foreign id is required.", nameof(secondForeignId));
            }
            var accountPermissionsToDelete = await _db.AccountPermissions.Where(ap => ap.PermissionId == secondForeignId).ToListAsync(cancellationToken);
            _db.AccountPermissions.RemoveRange(accountPermissionsToDelete);
        }

        public async Task DeleteAsync(Guid firstForeignId, Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            if (firstForeignId == Guid.Empty || secondForeignId == Guid.Empty)
            {
                throw new ArgumentException("First foreign id and second foreign id is required.", nameof(firstForeignId));
            }
            var existedAccountPermission = await GetByIdAsync(firstForeignId, secondForeignId, cancellationToken);
            if (existedAccountPermission is null)
            {
                throw new InvalidOperationException("AccountModel permission not found!");
            }
            _db.AccountPermissions.Remove(existedAccountPermission);
        }

        #endregion
    }
}