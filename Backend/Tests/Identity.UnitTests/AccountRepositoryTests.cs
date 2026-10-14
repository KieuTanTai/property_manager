
using System;
using System.Linq;
using System.Threading.Tasks;
using Identity.Infrastructure.Persistence.DbContext;
using Identity.Infrastructure.Repository.AccountRepository;
using Identity.Models.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Logging;
using Xunit;

namespace Identity.UnitTests
{
    public class AccountRepositoryTests
    {
        private readonly IdentityDbContext _context;
        private readonly AccountRepository _repository;

        public AccountRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new IdentityDbContext(options);

            var logger = new Mock<ILogger<AccountRepository>>();
            var logPool = new Mock<ILogPool>();

            _repository = new AccountRepository(
                _context,
                logger.Object,
                logPool.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllAccounts()
        {
            // Arrange: chuẩn bị dữ liệu
            var account1 = new AccountModel(
                Guid.NewGuid(), "user1@example.com", "hash1", true);

            var account2 = new AccountModel(
                Guid.NewGuid(), "user2@example.com", "hash2", true);

            _context.Accounts.AddRange(account1, account2);
            await _context.SaveChangesAsync();

            // Act: gọi hàm cần kiểm thử
            var result = await _repository.GetAllAsync();

            // Assert: kiểm tra kết quả
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.AccountEmail == "user1@example.com");
            Assert.Contains(result, a => a.AccountEmail == "user2@example.com");
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsAccount()
        {
            // Arrange
            var id = Guid.NewGuid();

            var account = new AccountModel(
                id, "user@example.com", "hash", true);

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.AccountId);
            Assert.Equal("user@example.com", result.AccountEmail);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            var id = Guid.NewGuid();

            var account = new AccountModel(
                id, "user@example.com", "hash", true);

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            var result = await _repository.ExistsAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_ExistingEmail_ReturnsAccount()
        {
            // Arrange
            var account = new AccountModel(
                Guid.NewGuid(), "user@example.com", "hash", true);

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAccountByEmailAsync(
                "user@example.com");

            // Assert
            Assert.Equal("user@example.com", result.AccountEmail);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_NonExistingEmail_ThrowsException()
        {
            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.GetAccountByEmailAsync("missing@example.com"));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
