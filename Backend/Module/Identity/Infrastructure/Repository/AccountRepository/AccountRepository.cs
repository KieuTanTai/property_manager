using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;
using Shared.Persistence;
using Shared.Persistence.Record;
using Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Repository.AccountRepository
{
    public class AccountRepository(
        IdentityDbContext context,
        ILogger<AccountRepository> logger,
        ILogPool logPool) : IAccountRepository
    {
        private readonly IdentityDbContext _db = context;
        private readonly ILogger<AccountRepository> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        private const string Module = "identity";
        private const string Layer = "repository";


        #region GET

        public async Task<IReadOnlyList<AccountModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all accounts.");
            return await _db.Accounts.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<AccountModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account by id.");
            return await _db.Accounts.AsNoTracking()
                .FirstOrDefaultAsync(account => account.AccountId == id, cancellationToken);
        }
        
        public async Task<IReadOnlyList<AccountModel>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading accounts by ids.");
            return await _db.Accounts.AsNoTracking()
                .Where(account => ids.Contains(account.AccountId))
                .ToListAsync(cancellationToken);
        }

        public async Task<AccountModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked account by id.");
            return await _db.Accounts.FirstOrDefaultAsync(account => account.AccountId == id,
                cancellationToken);
        }

        public async Task<AccountModel> GetAccountByEmailAsync(string email,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account by email.");
            return await _db.Accounts.AsNoTracking()
                       .FirstOrDefaultAsync(account => account.AccountEmail == email, cancellationToken)
                   ?? throw new InvalidOperationException("AccountModel not found!");
        }

        public async Task<AccountModel> GetTrackedAccountByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked account by email.");
            return await _db.Accounts.FirstOrDefaultAsync(account => account.AccountEmail == email, cancellationToken) ?? throw new InvalidOperationException("AccountModel not found!");
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking account existence.");
            return await _db.Accounts.AnyAsync(account => account.AccountId == id, cancellationToken);
        }

        public async Task<AccountModel?> GetAccountAndNavigationByEmailAsync(string email, bool isGetRole = true, bool isGetAdditionalPermission = true, bool isGetProfile = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account with navigation properties.");
            var query = _db.Accounts.AsNoTracking().AsQueryable().Where(account => account.AccountEmail == email);
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
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account page.");
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

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize, bool isGetProfile, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account page with profile.");
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.Accounts.AsNoTracking();
            query = query
                .AsSplitQuery()
                .Include(account => account.Roles)
                    .ThenInclude(role => role.Permissions)
                .Include(account => account.AdditionalPermissions);

            if (isGetProfile)
            {
                query = query.Include(account => account.UserProfile);
            }

            if (cursor.HasValue)
            {
                query = query.Where(account => account.AccountId < cursor.Value);
            }

            query = query.OrderByDescending(account => account.AccountId).Take(pageSize + 1);
            var accounts = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(accounts, pageSize, account => account.AccountId,
                cancellationToken);
            
        }
        
        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize,
            bool isActive, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account page by status.");
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.Accounts.AsNoTracking();
            query = query.Include(account => account.Roles)
                .Include(account => account.UserProfile)
                .Include(account => account.AdditionalPermissions);
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
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account creation.");
            if (string.IsNullOrWhiteSpace(accountModel.AccountEmail))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account creation rejected because the account identifier is missing.");
                throw new ArgumentException("AccountModel email is required.", nameof(accountModel.AccountEmail));
            }

            var isExisted = await _db.Accounts.AnyAsync(
                existedAccount => existedAccount.AccountEmail == accountModel.AccountEmail,
                cancellationToken);

            if (isExisted)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account creation rejected because the account already exists.");
                throw new InvalidOperationException($"Already existed! \n {accountModel.AccountEmail}");
            }
            await _db.Accounts.AddAsync(accountModel, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account staged for creation.");
        }

        public async Task UpdateAsync(AccountModel accountModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account update.");
            if (accountModel.AccountId == Guid.Empty)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account update rejected because the account identifier is missing.");
                throw new ArgumentException("AccountModel id is required.", nameof(accountModel.AccountId));
            }

            var existedAccount =
                await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(existedAccount => existedAccount.AccountId == accountModel.AccountId,
                    cancellationToken);

            if (existedAccount is null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account update rejected because the account was not found.");
                throw new InvalidOperationException($"AccountModel not found! \n {accountModel.AccountId}");
            }
            _db.Accounts.Update(accountModel);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account staged for update.");
        }

        #endregion
    }
}