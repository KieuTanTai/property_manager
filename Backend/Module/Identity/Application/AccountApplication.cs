using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;

namespace Identity.Application
{
    public class AccountApplication(
        IUnitOfWork unitOfWork,
        IAccountRepository accountRepository,
        IBaseAssociativeRepository<AccountRoleModel, Guid> accountRoleRepository,
        IAuthorizationApplication roleApplication,
        IAccountHelper accountHelper,
        ILogger<AccountApplication> logger,
        ILogPool logPool)
        : IAccountApplication
    {
        private const string Module = "Identity";

        private const string Layer = "Application";

        private readonly IAccountHelper _accountHelper = accountHelper;


        private readonly IAccountRepository _accountRepository = accountRepository;

        private readonly IBaseAssociativeRepository<AccountRoleModel, Guid> _accountRoleRepository = accountRoleRepository;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<AccountApplication> _logger = logger;

        private readonly IAuthorizationApplication _roleApplication = roleApplication;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;


        #region USER

        public async Task<AccountModel> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Register account operation started.");
            await IsValidForRegisterAsync(email, password, cancellationToken);

            var accountModel = new AccountModel(email, password, true, true);
            var hashedPassword = _accountHelper.GetPasswordHash(accountModel, password);
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

        public async Task<AccountModel> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Login account operation started.");
            CheckValidEmailAndPassword(email, password); // throw exception if email or password is invalid
            var accountModel = await GetAccountByEmailAsync(email, true, true, true, cancellationToken);
            if (!accountModel.AccountIsActive)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Login rejected because the account is inactive.");
                throw new InvalidOperationException("Account is not active.");
            }
            var result = !_accountHelper.PasswordVerify(accountModel, password, accountModel.AccountPassword!)
                ? throw InvalidPassword()
                : accountModel;
            _logger.LogLayerInformation(_logPool, Module, Layer, "Account login completed.");
            return result;
        }

        public async Task<bool> LogoutAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerWarning(_logPool, Module, Layer, "Logout requested but operation is not implemented.");
            throw new NotImplementedException();
        }

        public async Task<int> ChangePasswordAsync(string email, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Change password operation started.");
            if (string.CompareOrdinal(oldPassword, newPassword) == 0)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change rejected because passwords are equal.");
                throw new ArgumentException("New password must be different from old password.", nameof(newPassword));
            }

            CheckValidEmailAndPassword(email, oldPassword);
            if (!_accountHelper.IsPasswordValid(newPassword))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change rejected because the new password is invalid.");
                throw new ArgumentException("Account password is invalid.", nameof(newPassword));
            }

            var accountModel = await GetAccountByEmailAsync(email, true, cancellationToken);
            if (!accountModel.AccountIsActive)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change rejected because the account is inactive.");
                throw new InvalidOperationException("Account is not active.");
            }

            if (!_accountHelper.PasswordVerify(accountModel, oldPassword, accountModel.AccountPassword!))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Password change rejected because the current password is invalid.");
                throw new InvalidOperationException("Account password is invalid.");
            }

            var hashedPassword = _accountHelper.GetPasswordHash(accountModel, newPassword);
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
            CheckValidEmailAndPassword(email, password);
            var accountModel = await GetAccountByEmailAsync(email, true, cancellationToken);
            if (!_accountHelper.PasswordVerify(accountModel, password, accountModel.AccountPassword!))
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
            if (!_accountHelper.IsEmailValid(email))
            {
                throw new InvalidOperationException("Invalid email format.");
            }
            var accountModel = await _accountRepository.GetAccountAndNavigationByEmailAsync(email, true, true, true, cancellationToken);
            return accountModel;
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize, bool isActive, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Account status paging operation started.");
            var result = await _accountRepository.GetApplyPagingByStatusAsync(cursor, pageSize, isActive, cancellationToken);
            return result;
        }

        #endregion

        #region Private

        private async Task IsValidForRegisterAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            if (!CheckValidEmailAndPassword(email, password))
            {
                return;
            }

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
            //! not check email and password, call IsValidEmailAndPassword method before calling this method

            if (isTracked)
            {
                return await _accountRepository.GetTrackedAccountByEmailAsync(email, cancellationToken);
            }

            var existedAccount = await _accountRepository.GetAccountByEmailAsync(email, cancellationToken);
            return existedAccount;
        }

        private bool CheckValidEmailAndPassword(string email, string password)
        {
            if (!_accountHelper.IsPasswordValid(password))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account operation rejected because the password is invalid.");
                throw new ArgumentException("Account password is invalid.", nameof(password));
            }
            if (!_accountHelper.IsEmailValid(email))
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Account operation rejected because the account identifier is invalid.");
                throw new ArgumentException("Account email is invalid.", nameof(email));
            }
            return true;
        }

        private InvalidOperationException InvalidPassword()
        {
            _logger.LogLayerWarning(_logPool, Module, Layer, "Login rejected because the password is invalid.");
            return new InvalidOperationException("Account password is invalid.");
        }

        #endregion
    }
}