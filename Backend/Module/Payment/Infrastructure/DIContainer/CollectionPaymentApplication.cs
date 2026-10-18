using Payment.Application;
using Payment.Interfaces.IApplication;
using Payment.Models.Payment;
using VNPAY.NET;

namespace Payment.Infrastructure.DIContainer
{
    public static class CollectionPaymentApplication
    {
        public static IServiceCollection AddPaymentApplicationCollection(this IServiceCollection services,
            IConfiguration configuration)
        {
            #region CONFIG

            // Never commit TmnCode/HashSecret: use user-secrets or appsettings.Local.json (gitignored).
            var vnpaySettings = new VnpaySettingsModel();
            configuration.GetSection("VNPAY").Bind(vnpaySettings);

            if (string.IsNullOrWhiteSpace(vnpaySettings.TmnCode))
            {
                throw new InvalidOperationException("Configuration 'VNPAY:TmnCode' was not found.");
            }

            if (string.IsNullOrWhiteSpace(vnpaySettings.HashSecret))
            {
                throw new InvalidOperationException("Configuration 'VNPAY:HashSecret' was not found.");
            }

            if (string.IsNullOrWhiteSpace(vnpaySettings.CallbackUrl))
            {
                throw new InvalidOperationException("Configuration 'VNPAY:CallbackUrl' was not found.");
            }

            services.AddSingleton(vnpaySettings);

            // VNPAY.NET 8.5.0 has no DI extension: the client is configured through Initialize().
            services.AddScoped<IVnpay>(_ => {
                var vnpay = new Vnpay();
                vnpay.Initialize(
                    vnpaySettings.TmnCode,
                    vnpaySettings.HashSecret,
                    vnpaySettings.BaseUrl,
                    vnpaySettings.CallbackUrl,
                    vnpaySettings.Version,
                    vnpaySettings.OrderType);
                return vnpay;
            });

            #endregion

            services.AddScoped<IPaymentApplication, PaymentApplication>();

            return services;
        }
    }
}
