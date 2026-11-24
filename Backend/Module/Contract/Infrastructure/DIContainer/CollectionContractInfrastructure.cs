using Contract.Infrastructure.Persistence.DbContext;
using Contract.Infrastructure.Repository;
using Contract.Infrastructure.Repository.ContactRepository;
using Contract.Infrastructure.Repository.ContractRepository;
using Contract.Infrastructure.Repository.InvoiceRepository;
using Contract.Infrastructure.Repository.ReceiptRepository;
using Contract.Infrastructure.Repository.RegulationRepository;
using Contract.Infrastructure.Services;
using Contract.Infrastructure.Services.Options;
using Contract.Interfaces.IRepository;
using Contract.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Shared.DIContainer;
using Shared.Interfaces;
using Shared.Logging;

namespace Contract.Infrastructure.DIContainer
{
    public static class CollectionContractInfrastructure
    {
        public static IServiceCollection AddContractInfrastructureCollection(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            #region CONFIG

            var connectionString = configuration["IdentityTest"]
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

            #region CACHE

            services.AddSharedHybridCache(configuration);

            #endregion

            #region HELPERS

            services.AddSingleton<ILogPool, LogPool>();

            services.AddOptions<EmailOptions>()
                .Bind(configuration.GetSection(EmailOptions.SectionName))
                .Configure(options => options.SecureSocketOptions =
                    configuration["Email:SecureSocketOptions"] ?? options.SecureSocketOptions);
            services.AddOptions<PremiseSyncOptions>()
                .Configure(options =>
                {
                    options.Premise = configuration["ModuleUrls:Premise"] ?? options.Premise;
                    options.ApiKey = Environment.GetEnvironmentVariable("INTERNAL_GATEWAY_API_KEY")
                                     ?? configuration["InternalApi:Key"]
                                     ?? string.Empty;
                });
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddHttpClient<IContractPremiseSyncClient, ContractPremiseSyncClient>();
            services.AddHostedService<ContractLifecycleBackgroundService>();

            #endregion

            #region REPOSITORY

            services.AddScoped<IUnitOfWork, EfContractUnitOfWork>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IContractViolationRepository, ContractViolationRepository>();
            services.AddScoped<IContractRegulationRepository, ContractRegulationRepository>();
            services.AddScoped<IRegulationRepository, RegulationRepository>();
            services.AddScoped<IInvoiceDetailRepository, InvoiceDetailRepository>();
            services.AddScoped<IMonthlyInvoiceRepository, MonthlyInvoiceRepository>();
            services.AddScoped<IReceiptRepository, ReceiptRepository>();
            services.AddScoped<IContactInfoRepository, ContactInfoRepository>();

            #endregion

            return services;
        }
    }
}
