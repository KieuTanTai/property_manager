using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Identity.Infrastructure.Repository.AccountRepository
{
    public class AccountRepository(IdentityDbContext context) : IAccountRepository
    {
        private readonly IdentityDbContext _db = context;


        #region GET

        public async Task<IReadOnlyList<AccountModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _db.Accounts.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<AccountModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Accounts.AsNoTracking()
                .FirstOrDefaultAsync(account => account.AccountId == id, cancellationToken);
        }

        public async Task<AccountModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _db.Accounts.FirstOrDefaultAsync(account => account.AccountId == id,
                cancellationToken);
        }

        public async Task<AccountModel> GetAccountByEmailAsync(string email,
            CancellationToken cancellationToken = default)
        {
            return await _db.Accounts.AsNoTracking()
                       .FirstOrDefaultAsync(account => account.AccountEmail == email, cancellationToken)
                   ?? throw new InvalidOperationException("AccountModel not found!");
        }

        public async Task<AccountModel> GetTrackedAccountByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _db.Accounts.FirstOrDefaultAsync(account => account.AccountEmail == email, cancellationToken) ?? throw new InvalidOperationException("AccountModel not found!");
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Accounts.AnyAsync(account => account.AccountId == id, cancellationToken);
        }

        public async Task<AccountModel?> GetAccountAndNavigationByEmailAsync(string email, bool isGetRole = true, bool isGetAdditionalPermission = true, bool isGetProfile = false,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Accounts.AsNoTracking().Where(account => account.AccountEmail == email);
            if (isGetRole)
            {
                query = query.Include(account => account.Roles);
            }
            
            if (isGetAdditionalPermission)
            {
                query = query.Include(account => account.AdditionalPermissions);
            }
            
            if (isGetProfile)
            {
                query = query.Include(account => account.UserProfile);
            }
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        // Paging methods
        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.Accounts.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(account => account.AccountId < cursor.Value);
            }

            query = query.OrderByDescending(account => account.AccountId);
            var accounts = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(accounts, pageSize, account => account.AccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize,
            bool isActive, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.Accounts.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(account => account.AccountId < cursor.Value);
            }

            query = query.Where(account => account.AccountIsActive == isActive);
            query = query.OrderByDescending(account => account.AccountId);
            var accounts = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(accounts, pageSize, account => account.AccountId,
                cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(AccountModel accountModel, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accountModel.AccountEmail))
            {
                throw new ArgumentException("AccountModel email is required.", nameof(accountModel.AccountEmail));
            }

            var isExisted = await _db.Accounts.AnyAsync(
                existedAccount => existedAccount.AccountEmail == accountModel.AccountEmail,
                cancellationToken);

            if (isExisted)
            {
                throw new InvalidOperationException($"Already existed! \n {accountModel.AccountEmail}");
            }
            await _db.Accounts.AddAsync(accountModel, cancellationToken);
        }

        public async Task UpdateAsync(AccountModel accountModel,
            CancellationToken cancellationToken = default)
        {
            if (accountModel.AccountId == Guid.Empty)
            {
                throw new ArgumentException("AccountModel id is required.", nameof(accountModel.AccountId));
            }

            var existedAccount =
                await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(existedAccount => existedAccount.AccountId == accountModel.AccountId,
                    cancellationToken);

            if (existedAccount is null)
            {
                throw new InvalidOperationException($"AccountModel not found! \n {accountModel.AccountId}");
            }
            _db.Accounts.Update(accountModel);
        }

        #endregion
    }
}