using Microsoft.EntityFrameworkCore;
using Premise.Infrastructure.Persistence.DbContext;
using Premise.Infrastructure.Repository;
using Premise.Infrastructure.Repository.PremiseRepository;
using Premise.Interfaces.IRepository.BusinessType;
using Premise.Interfaces.IRepository.Location;
using Premise.Interfaces.IRepository.Premise;
using Premise.Interfaces.IRepository.Product;
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

            #region CACHE

            services.AddHybridCache();
            var redisConnection = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
            }

            #endregion

            #region REPOSITORY

            services.AddSingleton<ILogPool, LogPool>();
            services.AddScoped<IUnitOfWork, EfPremiseUnitOfWork>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IPremiseRepository, PremiseRepository>();
            services.AddScoped<IPremiseMediaRepository, PremiseMediaRepository>();
            services.AddScoped<IBusinessTypeRepository, BusinessTypeRepository>();
            services.AddScoped<IPremiseBusinessTypeRepository, PremiseBusinessTypeRepository>();
            services.AddScoped<IRentedPremiseRepository, RentedPremiseRepository>();
            services.AddScoped<IWhitelistProductRepository, WhitelistProductRepository>();
            services.AddScoped<IProductBusinessTypeRepository, ProductBusinessTypeRepository>();

            #endregion

            return services;
        }
    }
}
