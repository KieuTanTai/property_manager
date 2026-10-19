using Contract.Application;
using Contract.Interfaces.IApplication;
using Contract.Interfaces.Services;
using Contract.Infrastructure.Services;

namespace Contract.Infrastructure.DIContainer
{
    public static class CollectionContractApplication
    {
        public static IServiceCollection AddContractApplicationCollection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRegulationApplication, RegulationApplication>();
            services.AddScoped<IContractApplication, ContractApplication>();
            services.AddScoped<IInvoiceApplication, InvoiceApplication>();
            services.AddScoped<IReceiptApplication, ReceiptApplication>();
            services.AddScoped<IViolationApplication, ViolationApplication>();
            services.AddScoped<IContactInfoApplication, ContactInfoApplication>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            return services;
        }   
    }
}
