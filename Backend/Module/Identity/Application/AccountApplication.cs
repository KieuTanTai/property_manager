using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Identity.Application
{
    public class AccountApplication(
        IUnitOfWork unitOfWork,
        IAccountRepository accountRepository,
        IUserProfileRepository userProfileRepository,
        IBaseAssociativeRepository<AccountRoleModel, Guid> accountRoleRepository,
        IRoleApplication roleApplication,
        IAccountHelper accountHelper)
        : IAccountApplication
    {
        private readonly IAccountHelper _accountHelper = accountHelper;


        private readonly IAccountRepository _accountRepository = accountRepository;

        private readonly IBaseAssociativeRepository<AccountRoleModel, Guid> _accountRoleRepository = accountRoleRepository;

        private readonly IRoleApplication _roleApplication = roleApplication;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private readonly IUserProfileRepository _userProfileRepository = userProfileRepository;

        #region USER

        public async Task<AccountModel> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
        {
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
                throw new InvalidOperationException("Failed to add account.");
            }
            accountModel.SetRoles([baseRole]);
            return accountModel;
        }

        public async Task<AccountModel> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            CheckValidEmailAndPassword(email, password); // throw exception if email or password is invalid
            var accountModel = await GetAccountByEmailAsync(email, true, true, true, cancellationToken);
            if (!accountModel.AccountIsActive)
            {
                throw new InvalidOperationException("Account is not active.");
            }
            return !_accountHelper.PasswordVerify(accountModel, password, accountModel.AccountPassword!)
                ? throw new InvalidOperationException($@"Account password is invalid: {accountModel.AccountPassword}")
                : accountModel;
        }

        public async Task<bool> LogoutAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ChangePasswordAsync(string email, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            if (string.CompareOrdinal(oldPassword, newPassword) == 0)
            {
                throw new ArgumentException("New password must be different from old password.", nameof(newPassword));
            }

            CheckValidEmailAndPassword(email, oldPassword);
            if (!_accountHelper.IsPasswordValid(newPassword))
            {
                throw new ArgumentException("Account password is invalid.", nameof(newPassword));
            }

            var accountModel = await GetAccountByEmailAsync(email, true, cancellationToken);
            if (!accountModel.AccountIsActive)
            {
                throw new InvalidOperationException("Account is not active.");
            }

            if (!_accountHelper.PasswordVerify(accountModel, oldPassword, accountModel.AccountPassword!))
            {
                throw new InvalidOperationException("Account password is invalid.");
            }

            var hashedPassword = _accountHelper.GetPasswordHash(accountModel, newPassword);
            accountModel.SetHashedPassword(hashedPassword);
            await _accountRepository.UpdateAsync(accountModel, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return affectRows == 0 ? throw new InvalidOperationException("Failed to change password.") : affectRows;
        }

        public async Task<int> InactiveAccountAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            CheckValidEmailAndPassword(email, password);
            var accountModel = await GetAccountByEmailAsync(email, true, cancellationToken);
            if (!_accountHelper.PasswordVerify(accountModel, password, accountModel.AccountPassword!))
            {
                throw new InvalidOperationException("Account password is invalid.");
            }
            if (!accountModel.AccountIsActive)
            {
                throw new InvalidOperationException("Account is already inactive.");
            }
            accountModel.SetAccountIsActive(false);
            await _accountRepository.UpdateAsync(accountModel, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return affectRows == 0 ? throw new InvalidOperationException("Failed to inactive account.") : affectRows;
        }

        #endregion

        #region ADMIN

        public async Task<int> InactiveAccountByAdminAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            var result = await _accountRepository.GetByIdAsync(accountId, cancellationToken);

            if (result == null)
            {
                throw new ArgumentException("Account not found.", nameof(accountId));
            }

            if (!result.AccountIsActive)
            {
                throw new InvalidOperationException("Account is already inactive.");
            }
            result.SetAccountIsActive(false);
            await _accountRepository.UpdateAsync(result, cancellationToken);
            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return affectRows == 0 ? throw new InvalidOperationException("Failed to inactive account.") : affectRows;
        }

        public async Task<IReadOnlyList<AccountModel>> GetAllAccountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingAsync(Guid? cursor, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize, bool isActive, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<RecordBaseCursorPage<AccountModel>> GetAccountByPhoneNumberAsync(Guid? cursor, string phoneNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
                throw new ArgumentException("Account password is invalid.", nameof(password));
            }
            return !_accountHelper.IsEmailValid(email) ? throw new ArgumentException("Account email is invalid.", nameof(email)) : true;
        }

        #endregion
    }
}