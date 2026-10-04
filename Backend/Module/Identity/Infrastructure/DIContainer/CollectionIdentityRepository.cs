using Identity.Infrastructure.Persistence.DbContext;
using Identity.Infrastructure.Repository;
using Identity.Infrastructure.Repository.AccountRepository;
using Identity.Infrastructure.Repository.PermissionRepository;
using Identity.Infrastructure.Repository.RoleRepository;
using Identity.Infrastructure.Repository.UserProfileRepository;
using Identity.Interfaces.IRepository;
using Identity.Models.Account;
using Identity.Models.Permission;
using Identity.Models.Role;
using Identity.Utils.Enum;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Logging;

namespace Identity.Infrastructure.DIContainer
{
    public static class CollectionIdentityRepository
    {
        public static IServiceCollection AddIdentityRepositoryCollection(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            #region CONFIG

            var connectionString = configuration.GetConnectionString("IdentityTest")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'IdentityTest' was not found.");

            services.AddDbContext<IdentityDbContext>(options => {
                options.UseMySQL(connectionString);

                // options.EnableServiceProviderCaching();
                options.EnableThreadSafetyChecks();

                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors()
                        .EnableSensitiveDataLogging()
                        .LogTo(Console.WriteLine, LogLevel.Information);
                }
            });

            #endregion

            #region REPOSITORY

            services.AddSingleton<ILogPool, LogPool>();
            services.AddScoped<IUnitOfWork, EfIdentityUnitOfWork>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IBaseAuthorizationRepository<RoleModel, ESystemRoleCode, Guid>, RoleRepository>();
            services.AddScoped<IBaseAuthorizationRepository<PermissionModel, ESystemPermissionCode, Guid>, PermissionRepository>();
            services.AddScoped<IBaseAssociativeRepository<AccountAdditionalPermissionModel, Guid>, AccountAdditionalPermissionRepository>();
            services.AddScoped<IBaseAssociativeRepository<AccountRoleModel, Guid>, AccountRoleRepository>();
            services.AddScoped<IBaseAssociativeRepository<RolePermissionModel, Guid>, RolePermissionRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();

            #endregion

            return services;
        }
    }
}