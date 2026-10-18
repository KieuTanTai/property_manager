using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Interfaces;
using Shared.ModelHelper;

namespace Shared.DIContainer
{
    public static class SharedIdentityCollection
    {
        public static IServiceCollection AddSharedIdentityHelper<TAccountModel>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TAccountModel : class
        {
            using var stream = typeof(IdentityRulesModel).Assembly
                .GetManifestResourceStream("Shared.Configuration.account-rules.json")
                ?? throw new InvalidOperationException("The shared account rules configuration resource was not found.");

            var identityRulesModel = JsonSerializer.Deserialize<IdentityRulesModel>(stream)
                ?? throw new InvalidOperationException("The shared account rules configuration is invalid.");
            configuration.GetSection("AccountRules").Bind(identityRulesModel);

            services.TryAddSingleton(identityRulesModel);
            services.TryAddSingleton<IPasswordHasher<TAccountModel>, PasswordHasher<TAccountModel>>();
            services.TryAddSingleton<IIdentityHelper<TAccountModel>, IdentityHelper<TAccountModel>>();
            return services;
        }
    }
}
