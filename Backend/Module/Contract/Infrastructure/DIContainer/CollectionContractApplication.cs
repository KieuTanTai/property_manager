using Contract.Application;
using Contract.Interfaces.IApplication;

namespace Contract.Infrastructure.DIContainer
{
    public static class CollectionContractApplication
    {
        public static IServiceCollection AddContractApplicationCollection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRegulationApplication, RegulationApplication>();

            return services;
        }   
    }
}