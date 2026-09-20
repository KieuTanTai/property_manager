using Identity.Infrastructure.DIContainer;
using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Microsoft.AspNetCore.Identity;

namespace Identity.Utils
{
    namespace Identity.Utils
{
    public static class HashExistingAccounts
    {
        public static async Task RunAsync( IHostEnvironment environment,
            CancellationToken cancellationToken = default)
        {
            var configuration = BuildConfiguration();

            var services = new ServiceCollection();

            // Register toàn bộ Identity dependencies thật
            services.AddIdentityRepositoryCollection(configuration, environment);

            await using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var hasher = new PasswordHasher<AccountModel>();
            var accountHelper = new AccountHelper(null!, hasher);
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            
            var accounts = await accountRepository.GetAllAsync(cancellationToken);

            Console.WriteLine($"Found {accounts.Count} accounts.");

            foreach (var account in accounts)
            {
                if (string.IsNullOrWhiteSpace(account.AccountPassword))
                {
                    Console.WriteLine(
                        $"[SKIP] {account.AccountEmail}: password is empty.");

                    continue;
                }

                var plainPassword = account.AccountPassword;

                var hashedPassword =
                    accountHelper.GetPasswordHash(
                        account,
                        plainPassword);

                account.SetHashedPassword(hashedPassword);

                Console.WriteLine(
                    $"[HASHED] {account.AccountEmail}");
            }
            db.UpdateRange(accounts);
            await db.SaveChangesAsync(cancellationToken);

            Console.WriteLine("Finished hashing account passwords.");
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
}
