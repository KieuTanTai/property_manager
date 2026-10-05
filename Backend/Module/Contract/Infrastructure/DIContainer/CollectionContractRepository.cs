using Contract.Infrastructure.Persistence.DbContext;
using Contract.Infrastructure.Repository;
using Contract.Infrastructure.Repository.ContractRepository;
using Contract.Infrastructure.Repository.InvoiceRepository;
using Contract.Infrastructure.Repository.ReceiptRepository;
using Contract.Infrastructure.Repository.RegulationRepository;
using Contract.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Logging;

namespace Contract.Infrastructure.DIContainer
{
    public static class CollectionContractRepository
    {
        public static IServiceCollection AddContractRepositoryCollection(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            #region CONFIG

            var connectionString = configuration.GetConnectionString("IdentityTest")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'IdentityTest' was not found.");

            services.AddDbContext<ContractDbContext>(options => {
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
            services.AddScoped<IUnitOfWork, EfContractUnitOfWork>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IContractViolationRepository, ContractViolationRepository>();
            services.AddScoped<IContractRegulationRepository, ContractRegulationRepository>();
            services.AddScoped<IRegulationRepository, RegulationRepository>();
            services.AddScoped<IInvoiceDetailRepository, InvoiceDetailRepository>();
            services.AddScoped<IMonthlyInvoiceRepository, MonthlyInvoiceRepository>();
            services.AddScoped<IReceiptRepository, ReceiptRepository>();
            
            #endregion

            return services;
        }
    }
}