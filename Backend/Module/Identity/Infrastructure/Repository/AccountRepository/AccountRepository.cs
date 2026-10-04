using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Identity.Infrastructure.Repository.AccountRepository
{
    public class AccountRepository(
        IdentityDbContext context,
        ILogger<AccountRepository> logger,
        ILogPool logPool) : IAccountRepository
    {
        private const string Module = "Identity";

        private const string Layer = "Infrastructure/Repository/AccountRepository";

        private readonly IdentityDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<AccountRepository> _logger = logger;


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
            var account = await _db.Accounts.AsNoTracking()
                .FirstOrDefaultAsync(account => account.AccountEmail == email, cancellationToken);
            if (account is null)
            {
                var exception = new InvalidOperationException("AccountModel not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account lookup rejected because the account was not found by email.");
                throw exception;
            }

            return account;
        }

        public async Task<AccountModel> GetTrackedAccountByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked account by email.");
            var account = await _db.Accounts.FirstOrDefaultAsync(
                account => account.AccountEmail == email, cancellationToken);
            if (account is null)
            {
                var exception = new InvalidOperationException("AccountModel not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Tracked account lookup rejected because the account was not found by email.");
                throw exception;
            }

            return account;
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
            var query = _db.Accounts.AsNoTracking()
                .AsSplitQuery().Where(account => account.AccountEmail == email);
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
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account paging rejected because the page size is invalid."));
            var query = _db.Accounts.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(account => account.AccountId < cursor.Value);
            }

            query = query.OrderByDescending(account => account.AccountId).Take(pageSize + 1);
            var accounts = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(accounts, pageSize, account => account.AccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize, bool isGetProfile, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading account page with profile.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account paging rejected because the page size is invalid."));
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
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account paging rejected because the page size is invalid."));
            var query = _db.Accounts.AsNoTracking();
            query = query.Include(account => account.Roles)
                .Include(account => account.UserProfile)
                .Include(account => account.AdditionalPermissions);
            if (cursor.HasValue)
            {
                query = query.Where(account => account.AccountId < cursor.Value);
            }

            query = query.Where(account => account.AccountIsActive == isActive);
            query = query.OrderByDescending(account => account.AccountId).Take(pageSize + 1);
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
                var exception = new ArgumentException("AccountModel email is required.", nameof(accountModel.AccountEmail));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account creation rejected because the account identifier is missing.");
                throw exception;
            }

            var isExisted = await _db.Accounts.AnyAsync(
                existedAccount => existedAccount.AccountEmail == accountModel.AccountEmail,
                cancellationToken);

            if (isExisted)
            {
                var exception = new InvalidOperationException($"Already existed! \n {accountModel.AccountEmail}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account creation rejected because the account already exists.");
                throw exception;
            }
            await _db.Accounts.AddAsync(accountModel, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account staged for creation.");
        }

        public async Task AddRangeAsync(IEnumerable<AccountModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account range creation.");
            var accountModels = entities.ToList();
            if (!accountModels.Any())
            {
                var exception = new ArgumentException("AccountModel collection is required.", nameof(entities));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account range creation rejected because the collection is empty.");
                throw exception;
            }

            await _db.Accounts.AddRangeAsync(accountModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account range staged for creation.");
        }

        public async Task UpdateAsync(AccountModel accountModel,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account update.");
            if (accountModel.AccountId == Guid.Empty)
            {
                var exception = new ArgumentException("AccountModel id is required.", nameof(accountModel.AccountId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account update rejected because the account identifier is missing.");
                throw exception;
            }

            var existedAccount =
                await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(existedAccount => existedAccount.AccountId == accountModel.AccountId,
                    cancellationToken);

            if (existedAccount is null)
            {
                var exception = new InvalidOperationException($"AccountModel not found! \n {accountModel.AccountId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account update rejected because the account was not found.");
                throw exception;
            }
            _db.Accounts.Update(accountModel);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account staged for update.");
        }

        public void UpdateRange(IEnumerable<AccountModel> entities,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing account range update.");
            var accountModels = entities.ToList();
            if (!accountModels.Any())
            {
                var exception = new ArgumentException("AccountModel collection is required.", nameof(entities));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Account range update rejected because the collection is empty.");
                throw exception;
            }

            _db.Accounts.UpdateRange(accountModels);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Account range staged for update.");
        }

        #endregion
    }
}
