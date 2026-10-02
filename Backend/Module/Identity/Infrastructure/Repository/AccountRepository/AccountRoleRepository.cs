using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repository.AccountRepository
{
    public class AccountRoleRepository(IdentityDbContext context) : IBaseAssociativeRepository<AccountRoleModel, Guid>
    {
        private readonly IdentityDbContext _db = context;

        #region POST

        public async Task AddAsync(AccountRoleModel entity, CancellationToken cancellationToken = default)
        {
            if (entity.AccountId == Guid.Empty || entity.RoleId == Guid.Empty)
            {
                throw new ArgumentException("AccountModel and RoleModel id is required.", nameof(entity));
            }
            var existedAccountRole = await GetByIdAsync(entity.AccountId, entity.RoleId, cancellationToken);
            if (existedAccountRole is not null)
            {
                throw new InvalidOperationException("AccountModel role already exist!");
            }
            await _db.AccountRoles.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(List<AccountRoleModel> entities, CancellationToken cancellationToken = default)
        {
            if (entities is null || entities.Count == 0)
            {
                throw new ArgumentException("Entities is required.", nameof(entities));
            }
            // var filteredAccountRoles = await GetNotExistedEntitiesList(entities, cancellationToken);
            // if (!filteredAccountRoles.Any())
            // {
            //     throw new InvalidOperationException("All account roles already exist!");
            // }
            // await _db.AccountRoles.AddRangeAsync(filteredAccountRoles, cancellationToken);
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
            if (firstForeignId == Guid.Empty || secondForeignId == Guid.Empty)
            {
                throw new ArgumentException("First foreign id and second foreign id is required.", nameof(firstForeignId));
            }
            var existedAccountRole = await GetByIdAsync(firstForeignId, secondForeignId, cancellationToken);
            if (existedAccountRole is null)
            {
                throw new InvalidOperationException("AccountModel role not found!");
            }
            _db.AccountRoles.Remove(existedAccountRole);
        }

        public async Task DeleteByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            if (firstForeignId == Guid.Empty)
            {
                throw new ArgumentException("First foreign id is required.", nameof(firstForeignId));
            }
            var accountRolesToDelete = await _db.AccountRoles.Where(ar => ar.AccountId == firstForeignId).ToListAsync(cancellationToken);
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