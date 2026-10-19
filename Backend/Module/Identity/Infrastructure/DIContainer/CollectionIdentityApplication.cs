using Identity.Application;
using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Models;
using Identity.Models.Account;
using Identity.Utils;
using Microsoft.AspNetCore.Identity;
using Shared.DIContainer;

namespace Identity.Infrastructure.DIContainer
{
    public static class CollectionIdentityApplication
    {
        public static IServiceCollection AddIdentityApplicationCollection(this IServiceCollection services, IConfiguration configuration)
        {
            #region CONFIG

            services.AddSharedIdentityHelper<AccountModel>(configuration);

            services.Configure<PasswordHasherOptions>(options => {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                options.IterationCount = configuration.GetValue<int>("PasswordHasherOptions:IterationCount");
            });

            #endregion

            services.AddSingleton<IApiHelper, ApiHelper>();
            services.AddSingleton<IPasswordHasher<AccountModel>, PasswordHasher<AccountModel>>();
            services.AddScoped<IAuthorizationApplication, AuthorizationApplication>();
            services.AddScoped<IAccountApplication, AccountApplication>();
            services.AddScoped<IUserProfileApplication, UserProfileApplication>();
            // HashExistingAccounts.RunAsync(environment).Wait();


            return services;
        }
    }
}
