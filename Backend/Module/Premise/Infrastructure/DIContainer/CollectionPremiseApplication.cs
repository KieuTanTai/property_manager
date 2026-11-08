using Premise.Application;
using Premise.Interfaces.IApplication;

namespace Premise.Infrastructure.DIContainer
{
    public static class CollectionPremiseApplication
    {
        public static IServiceCollection AddPremiseApplicationCollection(this IServiceCollection services)
        {
            services.AddScoped<ILocationApplication, LocationApplication>();
            services.AddScoped<IBusinessTypeApplication, BusinessTypeApplication>();
            services.AddScoped<IPremiseApplication, PremiseApplication>();
            services.AddScoped<IProductApplication, ProductApplication>();
            services.AddScoped<IPremiseStatisticApplication, PremiseStatisticApplication>();

            return services;
        }
    }
}