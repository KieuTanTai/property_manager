using Microsoft.EntityFrameworkCore;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Infrastructure.Repository;
using Shared.Interfaces;
using Shared.Logging;

namespace Premise.Infrastructure.DIContainer
{
    public static class CollectionPremiseRepository
    {
        public static IServiceCollection AddPremiseRepositoryCollection(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            #region CONFIG

            var connectionString = configuration.GetConnectionString("IdentityTest")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'IdentityTest' was not found.");

            services.AddDbContext<PremiseDbContext>(options => {
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
            services.AddScoped<IUnitOfWork, EfPremiseUnitOfWork>();

            #endregion

            return services;
        }
    }
}