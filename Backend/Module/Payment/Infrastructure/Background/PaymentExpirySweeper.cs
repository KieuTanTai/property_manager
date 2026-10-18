using Payment.Interfaces.IRepository;
using Shared.Logging;

namespace Payment.Infrastructure.Background
{
    /// <summary>
    /// Moves abandoned pending transactions to <c>Expired</c>.
    /// <para>
    /// Nothing else expires them: VNPAY only settles through the IPN, so an attempt where the
    /// callback never arrives (VNPAY could not reach the app, the user closed the payment page,
    /// the tunnel was down) would stay <c>Pending</c> forever and keep answering "not paid".
    /// </para>
    /// </summary>
    public sealed class PaymentExpirySweeper(
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentExpirySweeper> logger,
        ILogPool logPool) : BackgroundService
    {
        private const string Module = "Payment";

        private const string Layer = "Infrastructure/Background/PaymentExpirySweeper";

        // Transactions live 15 minutes, so a sweep every 5 minutes is well inside one lifetime.
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        private readonly ILogger<PaymentExpirySweeper> _logger = logger;

        private readonly ILogPool _logPool = logPool;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Payment expiry sweeper started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await SweepAsync(stoppingToken);

                try
                {
                    await Task.Delay(SweepInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task SweepAsync(CancellationToken cancellationToken)
        {
            try
            {
                // The repository and its DbContext are scoped, so every sweep opens its own scope.
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();

                await repository.ExpireStalePendingAsync(DateTime.UtcNow, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Shutting down.
            }
            catch (Exception exception)
            {
                // A failed sweep must not kill the loop: the next tick can still recover.
                _logger.LogLayerError(_logPool, Module, Layer, exception, "Payment expiry sweep failed.");
            }
        }
    }
}
