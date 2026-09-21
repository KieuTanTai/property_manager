using Identity.Application;
using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Models;
using Identity.Models.Account;
using Identity.Utils;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.DIContainer
{
    public static class CollectionIdentityApplication
    {
        public static IServiceCollection AddIdentityApplicationCollection(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            #region CONFIG

            var accountRules = new AccountRulesModel();
            configuration.GetSection("AccountRules").Bind(accountRules);

            services.Configure<PasswordHasherOptions>(options => {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                options.IterationCount = configuration.GetValue<int>("PasswordHasherOptions:IterationCount");
            });

            #endregion

            services.AddSingleton(accountRules);
            services.AddSingleton<IAccountHelper, AccountHelper>();
            services.AddSingleton<IApiHelper, ApiHelper>();
            services.AddSingleton<IPasswordHasher<AccountModel>, PasswordHasher<AccountModel>>();
            services.AddScoped<IRoleApplication, RoleApplication>();
            services.AddScoped<IAccountApplication, AccountApplication>();
            services.AddScoped<IUserProfileApplication, UserProfileApplication>();
            // HashExistingAccounts.RunAsync(environment).Wait();


            return services;
        }
    }
}