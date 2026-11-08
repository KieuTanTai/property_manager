using TicketAndNotification.Application;
using TicketAndNotification.Interfaces.Application;

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

            return services;
        }
    }
}
