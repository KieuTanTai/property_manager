using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Background;
using Payment.Infrastructure.Persistence.DbContext;
using Payment.Infrastructure.Repository;
using Payment.Infrastructure.Repository.PaymentTransactionRepository;
using Payment.Interfaces.IRepository;
using Shared.Interfaces;
using Shared.Logging;

namespace Payment.Infrastructure.DIContainer
{
    public static class CollectionPaymentRepository
    {
        public static IServiceCollection AddPaymentRepositoryCollection(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment environment)
        {
            #region CONFIG

            // Payment owns its own tables but shares the single database used by the other modules.
            var connectionString = configuration.GetConnectionString("IdentityTest")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'IdentityTest' was not found.");

            services.AddDbContext<PaymentDbContext>(options => {
                options.UseMySQL(connectionString);

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
            services.AddScoped<IUnitOfWork, EfPaymentUnitOfWork>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

            // Expires abandoned attempts so a missing IPN cannot leave a row pending forever.
            services.AddHostedService<PaymentExpirySweeper>();

            #endregion

            return services;
        }
    }
}
