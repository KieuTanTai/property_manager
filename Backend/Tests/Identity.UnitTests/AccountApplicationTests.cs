
using System;
using System.Threading.Tasks;
using Identity.Application;
using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Identity.Models.Profile;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Interfaces;
using Shared.Logging;
using Xunit;
using Identity.Models.Role;

namespace Identity.UnitTests
{
    public class AccountApplicationTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IAccountRepository> _accountRepository;
        private readonly Mock<IBaseAssociativeRepository<AccountRoleModel, Guid>>
            _accountRoleRepository;
        private readonly Mock<IAuthorizationApplication> _roleApplication;
        private readonly Mock<IAccountHelper> _accountHelper;
        private readonly Mock<IBaseAssociativeRepository<AccountAdditionalPermissionModel, Guid>>
            _permissionRepository;
        private readonly Mock<IUserProfileRepository> _userProfileRepository;
        private readonly Mock<ILogger<AccountApplication>> _logger;
        private readonly Mock<ILogPool> _logPool;

        private readonly AccountApplication _application;

        public AccountApplicationTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _accountRepository = new Mock<IAccountRepository>();
            _accountRoleRepository =
                new Mock<IBaseAssociativeRepository<AccountRoleModel, Guid>>();
            _roleApplication = new Mock<IAuthorizationApplication>();
            _accountHelper = new Mock<IAccountHelper>();
            _permissionRepository =
                new Mock<IBaseAssociativeRepository<AccountAdditionalPermissionModel, Guid>>();
            _userProfileRepository = new Mock<IUserProfileRepository>();
            _logger = new Mock<ILogger<AccountApplication>>();
            _logPool = new Mock<ILogPool>();

            _application = new AccountApplication(
                _unitOfWork.Object,
                _accountRepository.Object,
                _accountRoleRepository.Object,
                _roleApplication.Object,
                _accountHelper.Object,
                _permissionRepository.Object,
                _userProfileRepository.Object,
                _logger.Object,
                _logPool.Object
            );
        }

        [Fact]
        public async Task LoginAsync_InvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            _accountHelper
                .Setup(x => x.IsPasswordValid("Password123"))
                .Returns(true);

            _accountHelper
                .Setup(x => x.IsEmailValid("invalid-email"))
                .Returns(false);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _application.LoginAsync(
                    "invalid-email",
                    "Password123"
                )
            );

            // Assert
            Assert.Equal("email", exception.ParamName);

            _accountRepository.Verify(
                x => x.GetAccountAndNavigationByEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task LoginAsync_InvalidPasswordFormat_ThrowsArgumentException()
        {
            // Arrange
            _accountHelper
                .Setup(x => x.IsPasswordValid("123"))
                .Returns(false);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _application.LoginAsync(
                    "user@example.com",
                    "123"
                )
            );

            // Assert
            Assert.Equal("password", exception.ParamName);

            _accountRepository.Verify(
                x => x.GetAccountAndNavigationByEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task LoginAsync_InactiveAccount_ThrowsInvalidOperationException()
        {
            // Arrange
            var account = new AccountModel(
                Guid.NewGuid(),
                "user@example.com",
                "hashed-password",
                false
            );

            _accountHelper
                .Setup(x => x.IsPasswordValid("Password123"))
                .Returns(true);

            _accountHelper
                .Setup(x => x.IsEmailValid("user@example.com"))
                .Returns(true);

            _accountRepository
                .Setup(x => x.GetAccountAndNavigationByEmailAsync(
                    "user@example.com",
                    true,
                    true,
                    true,
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(account);

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => _application.LoginAsync(
                        "user@example.com",
                        "Password123"
                    )
                );

            Assert.Equal("Account is not active.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsInvalidOperationException()
        {
            // Arrange
            var account = new AccountModel(
                Guid.NewGuid(),
                "user@example.com",
                "hashed-password",
                true
            );

            _accountHelper
                .Setup(x => x.IsPasswordValid("Password123"))
                .Returns(true);

            _accountHelper
                .Setup(x => x.IsEmailValid("user@example.com"))
                .Returns(true);

            _accountRepository
                .Setup(x => x.GetAccountAndNavigationByEmailAsync(
                    "user@example.com",
                    true,
                    true,
                    true,
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(account);

            _accountHelper
                .Setup(x => x.VerifyPassword(
                    account,
                    "Password123",
                    "hashed-password"
                ))
                .Returns(false);

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => _application.LoginAsync(
                        "user@example.com",
                        "Password123"
                    )
                );

            Assert.Equal("Account password is invalid.", exception.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_SamePassword_ThrowsArgumentException()
        {
            // Arrange
            var password = "Password123";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _application.ChangePasswordAsync(
                    "user@example.com",
                    password,
                    password
                )
            );

            Assert.Equal("newPassword", exception.ParamName);
        }

        [Fact]
        public async Task ChangePasswordAsync_InvalidNewPassword_ThrowsArgumentException()
        {
            // Arrange
            _accountHelper
                .Setup(x => x.IsPasswordValid("OldPassword123"))
                .Returns(true);

            _accountHelper
                .Setup(x => x.IsEmailValid("user@example.com"))
                .Returns(true);

            _accountHelper
                .Setup(x => x.IsPasswordValid("123"))
                .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _application.ChangePasswordAsync(
                    "user@example.com",
                    "OldPassword123",
                    "123"
                )
            );

            Assert.Equal("newPassword", exception.ParamName);

            _accountRepository.Verify(
                x => x.GetTrackedAccountByEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
        }

        
        [Fact]
        public async Task RegisterAsync_Success_ReturnsAccount()
        {
            // Arrange
            string email = "newuser@example.com";
            string password = "Password123";
            var role = new RoleModel(Guid.NewGuid(), "Base role", true);

            _accountHelper.Setup(x => x.IsEmailValid(email)).Returns(true);
            _accountHelper.Setup(x => x.IsPasswordValid(password)).Returns(true);

            // Email chưa tồn tại: repository báo không tìm thấy tài khoản
            _accountRepository
                .Setup(x => x.GetAccountByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Account not found"));

            _accountHelper
                .Setup(x => x.GetHashPassword(It.IsAny<AccountModel>(), password))
                .Returns("hashed-password");

            _roleApplication
                .Setup(x => x.GetBaseRolesForUserAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _accountRepository
                .Setup(x => x.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _accountRoleRepository
                .Setup(x => x.AddAsync(It.IsAny<AccountRoleModel>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _application.RegisterAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.AccountEmail);
            Assert.True(result.AccountIsActive);

            _accountRepository.Verify(
                x => x.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _accountRoleRepository.Verify(
                x => x.AddAsync(It.IsAny<AccountRoleModel>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_InvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            _accountHelper.Setup(x => x.IsEmailValid("invalid-email")).Returns(false);
            _accountHelper.Setup(x => x.IsPasswordValid("Password123")).Returns(true);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _application.RegisterAsync("invalid-email", "Password123"));

            // Assert
            Assert.Equal("email", exception.ParamName);

            _accountRepository.Verify(
                x => x.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_InvalidPassword_ThrowsArgumentException()
        {
            // Arrange
            _accountHelper.Setup(x => x.IsEmailValid("newuser@example.com")).Returns(true);
            _accountHelper.Setup(x => x.IsPasswordValid("123")).Returns(false);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _application.RegisterAsync("newuser@example.com", "123"));

            // Assert
            Assert.Equal("password", exception.ParamName);

            _accountRepository.Verify(
                x => x.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ExistingEmail_ThrowsArgumentException()
        {
            // Arrange
            string email = "existing@example.com";
            string password = "Password123";

            _accountHelper.Setup(x => x.IsEmailValid(email)).Returns(true);
            _accountHelper.Setup(x => x.IsPasswordValid(password)).Returns(true);

            _accountRepository
                .Setup(x => x.GetAccountByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountModel(
                    Guid.NewGuid(), email, "hashed-password", true));

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _application.RegisterAsync(email, password));

            // Assert
            Assert.Equal("email", exception.ParamName);

            _accountRepository.Verify(
                x => x.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_SaveChangesReturnsZero_ThrowsInvalidOperationException()
        {
            // Arrange
            string email = "newuser@example.com";
            string password = "Password123";
            var role = new RoleModel(Guid.NewGuid(), "Base role", true);

            _accountHelper.Setup(x => x.IsEmailValid(email)).Returns(true);
            _accountHelper.Setup(x => x.IsPasswordValid(password)).Returns(true);

            _accountRepository
                .Setup(x => x.GetAccountByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Account not found"));

            _accountHelper
                .Setup(x => x.GetHashPassword(It.IsAny<AccountModel>(), password))
                .Returns("hashed-password");

            _roleApplication
                .Setup(x => x.GetBaseRolesForUserAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _accountRepository
                .Setup(x => x.AddAsync(It.IsAny<AccountModel>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _accountRoleRepository
                .Setup(x => x.AddAsync(It.IsAny<AccountRoleModel>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _application.RegisterAsync(email, password));

            Assert.Equal("Failed to add account.", exception.Message);
        }

    }
}
