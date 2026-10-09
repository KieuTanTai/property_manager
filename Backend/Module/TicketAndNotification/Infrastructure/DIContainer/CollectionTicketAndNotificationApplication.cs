using TicketAndNotification.Application;
using TicketAndNotification.Infrastructure.SignalR;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Interfaces.Services;

namespace TicketAndNotification.Infrastructure.DIContainer
{
    public static class CollectionTicketAndNotificationApplication
    {
        public static IServiceCollection AddTicketAndNotificationApplicationCollection(
            this IServiceCollection services)
        {
            services.AddScoped<ITicketApplication, TicketApplication>();
            services.AddScoped<ITicketMediaApplication, TicketMediaApplication>();
            services.AddScoped<INotificationApplication, NotificationApplication>();
            services.AddScoped<INotificationRecipientApplication, NotificationRecipientApplication>();
            services.AddScoped<INotificationRealtimePublisher, SignalRNotificationPublisher>();

            return services;
        }
    }
}
