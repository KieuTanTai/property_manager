using Microsoft.EntityFrameworkCore;
using Shared.Caching;
using Shared.Interfaces;
using Shared.Logging;
using TicketAndNotification.Infrastructure.Persistence.DbContext;
using TicketAndNotification.Infrastructure.Repository;
using TicketAndNotification.Infrastructure.Repository.NotificationRepository;
using TicketAndNotification.Infrastructure.Repository.TicketRepository;
using TicketAndNotification.Infrastructure.SignalR;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Interfaces.Services;
using TicketAndNotification.Models.Notification;

namespace TicketAndNotification.Infrastructure.DIContainer
{
    public static class CollectionTicketAndNotificationInfrastructure
    {
        public static IServiceCollection AddTicketAndNotificationInfrastructureCollection(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            #region CONFIG

            var connectionString = configuration["IdentityTest"]
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'IdentityTest' was not found.");

            services.AddDbContext<TicketAndNotificationDbContext>(options => {
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

            #region CACHE

            services.AddSharedHybridCache(configuration);

            #endregion

            #region LOGGING

            services.AddSingleton<ILogPool, LogPool>();

            #endregion

            #region SIGNALR

            services.AddSignalR();
            services.AddScoped<INotificationRealtimePublisher, SignalRNotificationPublisher>();

            #endregion

            #region REPOSITORY

            services.AddScoped<IUnitOfWork, EfTicketAndNotificationUnitOfWork>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketMediaRepository, TicketMediaRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IBaseAssociativeRepository<NotificationRecipientModel, Guid>, NotificationRecipientRepository>();

            #endregion

            return services;
        }
    }
}
