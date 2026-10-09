using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Shared.Interfaces;
using Shared.Logging;
using TicketAndNotification.Infrastructure.Persistence.DbContext;
using TicketAndNotification.Infrastructure.Repository;
using TicketAndNotification.Infrastructure.Repository.NotificationRepository;
using TicketAndNotification.Infrastructure.Repository.TicketRepository;
using TicketAndNotification.Interfaces.Repository;

namespace TicketAndNotification.Infrastructure.DIContainer
{
    public static class CollectionTicketAndNotificationRepository
    {
        public static IServiceCollection AddTicketAndNotificationRepositoryCollection(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var connectionString = configuration.GetConnectionString("IdentityTest")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'IdentityTest' was not found.");

            services.AddDbContext<TicketAndNotificationDbContext>(options =>
            {
                options.UseMySQL(connectionString);
                options.EnableThreadSafetyChecks();

                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors()
                        .EnableSensitiveDataLogging()
                        .LogTo(Console.WriteLine, LogLevel.Information);
                }
            });

            services.AddSignalR();
            services.AddSingleton<ILogPool, LogPool>();
            services.AddScoped<IUnitOfWork, EfTicketAndNotificationUnitOfWork>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketMediaRepository, TicketMediaRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationRecipientRepository,
                NotificationRecipientRepository>();

            return services;
        }
    }
}
