using Identity.Application.Record;
using Identity.Interfaces;
using Identity.Interfaces.Credentials;
using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Shared.Enum;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;
using Identity.Presentation.Record.Account;
using Identity.Presentation.Record.Profile;
using Shared.Persistence.Record.Auth;

namespace Identity.Application
{
    public class AccountApplication(
        IUnitOfWork unitOfWork,
        IAccountRepository accountRepository,
        IBaseAssociativeRepository<AccountRoleModel, Guid> accountRoleRepository,
        IAuthorizationApplication roleApplication,
        IIdentityHelper<AccountModel> accountHelper,
        IMappingHelper apiHelper,
        IJwtTokenService jwtTokenService,
        ILogger<AccountApplication> logger,
        ILogPool logPool)
        : IAccountApplication
    {
        private const string Module = "Identity";

        private const string Layer = "Application";

        private readonly IIdentityHelper<AccountModel> _accountHelper = accountHelper;


        private readonly IAccountRepository _accountRepository = accountRepository;

        private readonly IMappingHelper _apiHelper = apiHelper;

        private readonly IBaseAssociativeRepository<AccountRoleModel, Guid> _accountRoleRepository = accountRoleRepository;

        private readonly ILogPool _logPool = logPool;

        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

        private readonly ILogger<AccountApplication> _logger = logger;

        private readonly IAuthorizationApplication _roleApplication = roleApplication;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        #region USER

        public async Task<AccountModel> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Register account operation started.");
            await EnsureEmailAvailableAsync(email, cancellationToken);

            var accountModel = new AccountModel(email, password, true, true);
            var hashedPassword = _accountHelper.GetHashPassword(accountModel, password);
            accountModel.SetHashedPassword(hashedPassword);

            var baseRole = await _roleApplication.GetBaseRolesForUserAsync(cancellationToken);
            var accountRole = new AccountRoleModel(accountModel.AccountId, baseRole.RoleId);
            await _accountRepository.AddAsync(accountModel, cancellationToken);
            await _accountRoleRepository.AddAsync(accountRole, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectRows == 0)
            {
                _logger.LogLayerError(_logPool, Module, Layer, new InvalidOperationException("Account persistence returned zero affected rows."), "Account registration persistence failed.");
                throw new InvalidOperationException("Failed to add account.");
            }
            accountModel.SetRoles([baseRole]);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account registration completed.");
            return accountModel;
        }

        public async Task<RecordLoginResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Login account operation started.");
            var accountModel = await GetAccountByEmailAsync(email, true, true, true, cancellationToken);
            if (!accountModel.AccountIsActive)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Login rejected because the account is inactive.");
                throw new InvalidOperationException("Account is not active.");
            }
            var verifyResult = !_accountHelper.VerifyPassword(accountModel, password, accountModel.AccountPassword!);
            if (verifyResult)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Login rejected because the password is invalid.");
                throw new InvalidOperationException("Account password is invalid.");
            }
            var loginResponse = CreateLoginResponse(accountModel);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account login completed.");
            return loginResponse;
        }

        public async Task<bool> LogoutAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerWarning(_logPool, Module, Layer, "Logout requested but operation is not implemented.");
            throw new NotImplementedException();
        }

        public async Task<int> ChangePasswordAsync(string email, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Change password operation started.");
            var accountModel = await GetAccountByEmailAsync(email, true, cancellationToken);
            if (!accountModel.AccountIsActive)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change rejected because the account is inactive.");
                throw new InvalidOperationException("Account is not active.");
            }

            if (!_accountHelper.VerifyPassword(accountModel, oldPassword, accountModel.AccountPassword!))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change rejected because the current password is invalid.");
                throw new InvalidOperationException("Account password is invalid.");
            }

            var hashedPassword = _accountHelper.GetHashPassword(accountModel, newPassword);
            accountModel.SetHashedPassword(hashedPassword);
            await _accountRepository.UpdateAsync(accountModel, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectRows == 0)
            {
                _logger.LogLayerError(_logPool, Module, Layer, new InvalidOperationException("Password change saved no rows."), "Password change failed.");
                throw new InvalidOperationException("Failed to change password.");
            }
            _logger.LogLayerInformation(_logPool, Module, Layer, "Password change completed.");
            return affectRows;
        }

        public async Task<int> InactiveAccountAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Self-deactivate account operation started.");
            var accountModel = await GetAccountByEmailAsync(email, true, cancellationToken);
            if (!_accountHelper.VerifyPassword(accountModel, password, accountModel.AccountPassword!))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account deactivation rejected because the password is invalid.");
                throw new InvalidOperationException("Account password is invalid.");
            }
            if (!accountModel.AccountIsActive)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account deactivation rejected because the account is already inactive.");
                throw new InvalidOperationException("Account is already inactive.");
            }
            accountModel.SetAccountIsActive(false);
            await _accountRepository.UpdateAsync(accountModel, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectRows == 0)
            {
                _logger.LogLayerError(_logPool, Module, Layer, new InvalidOperationException("Account deactivation saved no rows."), "Account deactivation failed.");
                throw new InvalidOperationException("Failed to inactive account.");
            }
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account deactivation completed.");
            return affectRows;
        }

        #endregion

        #region ADMIN

        public Task<IReadOnlyList<AccountModel>> GetAllAccountsByRoleCodeAsync(ESystemRoleCode roleCode, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Get all accounts by role code operation started.");
            var result = _accountRepository.GetAllAccountsByRoleCodeAsync(roleCode, cancellationToken);
            if (result.Result.Count == 0)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "No accounts found for the specified role code.");
            }
            _logger.LogLayerInformation(_logPool, Module, Layer, $"Get all accounts by role code operation completed. Found {result.Result.Count} accounts.");
            return result;
        }

        public async Task<int> ActiveAccountByAdminAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Admin activate account operation started.");

            var result = await _accountRepository.GetAccountByEmailAsync(email, cancellationToken);

            if (result.AccountIsActive)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Admin account activation rejected because the account is already active.");
                throw new InvalidOperationException("Account is already active.");
            }
            result.SetAccountIsActive(true);
            await _accountRepository.UpdateAsync(result, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectRows == 0)
            {
                _logger.LogLayerError(_logPool, Module, Layer, new InvalidOperationException("Failed to active account."), "Account activation failed.");
                throw new InvalidOperationException("Failed to active account.");
            }
            _logger.LogLayerInformation(_logPool, Module, Layer, "Admin account activation completed.");
            return affectRows;
        }

        public async Task<int> InactiveAccountByAdminAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Admin deactivate account operation started.");
            var result = await _accountRepository.GetByIdAsync(accountId, cancellationToken);

            if (result == null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Admin account deactivation rejected because the account was not found.");
                throw new ArgumentException("Account not found.", nameof(accountId));
            }

            if (!result.AccountIsActive)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Admin account deactivation rejected because the account is already inactive.");
                throw new InvalidOperationException("Account is already inactive.");
            }
            result.SetAccountIsActive(false);
            await _accountRepository.UpdateAsync(result, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return affectRows == 0 ? throw new InvalidOperationException("Failed to inactive account.") : affectRows;
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account paging operation started.");
            var result = await _accountRepository.GetApplyPagingAsync(cursor, pageSize, cancellationToken);
            return result;
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize, bool isGetProfile, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account paging with profile option operation started.");
            var result = await _accountRepository.GetApplyPagingAsync(cursor, pageSize, isGetProfile, cancellationToken);
            return result;
        }

        public async Task<AccountModel?> GetAccountByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account lookup operation started.");
            var accountModel = await _accountRepository.GetAccountAndNavigationByEmailAsync(email, true, true, true, cancellationToken);
            return accountModel;
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize, bool isActive, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account status paging operation started.");
            var result = await _accountRepository.GetApplyPagingByStatusAsync(cursor, pageSize, isActive, cancellationToken);
            return result;
        }

        public async Task<int> UpdateRange(IEnumerable<AccountModel> entities, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account update range operation started.");
            var accountModels = entities.ToList();
            _accountRepository.UpdateRange(accountModels, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectRows == 0)
            {
                _logger.LogLayerError(_logPool, Module, Layer, new InvalidOperationException("Account update range saved no rows."), "Account update range failed.");
                throw new InvalidOperationException("Failed to update accounts.");
            }
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account update range completed.");
            return affectRows;
        }

        #endregion

        #region Private

        private RecordLoginResponse CreateLoginResponse(AccountModel account)
        {
            var authResponse = MapLoginAccount(account);
            var profileResponse = MapLoginProfile(account);
            var accessToken = CreateAccessToken(authResponse);
            return new RecordLoginResponse(authResponse, profileResponse, accessToken);
        }

        private RecordAuthResponse MapLoginAccount(AccountModel account) =>
            _apiHelper.MappingAuthResult(account);

        private RecordProfileResponse? MapLoginProfile(AccountModel account) =>
            account.UserProfile is null ? null : _apiHelper.MappingProfileResult(account.UserProfile);

        private string CreateAccessToken(RecordAuthResponse account) =>
            _jwtTokenService.GenerateAccessToken(account);

        private async Task EnsureEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                var existedAccount = await _accountRepository.GetAccountByEmailAsync(email, cancellationToken);
                if (existedAccount != null)
                {
                    throw new ArgumentException("Account email is existed.", nameof(email));
                }
            }
            catch (InvalidOperationException)
            {
                //valid for register
            }
        }

        private async Task<AccountModel> GetAccountByEmailAsync(string email, bool isGetRole = true, bool isGetAdditionalPermission = true, bool isGetProfile = false, CancellationToken cancellationToken = default)
        {
            var existedAccount = await _accountRepository.GetAccountAndNavigationByEmailAsync(email, isGetRole, isGetAdditionalPermission, isGetProfile, cancellationToken);
            return existedAccount ?? throw new InvalidOperationException("AccountModel not found!");
        }

        private async Task<AccountModel> GetAccountByEmailAsync(string email, bool isTracked = false, CancellationToken cancellationToken = default)
        {
            if (isTracked)
            {
                return await _accountRepository.GetTrackedAccountByEmailAsync(email, cancellationToken);
            }

            var existedAccount = await _accountRepository.GetAccountByEmailAsync(email, cancellationToken);
            return existedAccount;
        }



        #endregion
    }
}
