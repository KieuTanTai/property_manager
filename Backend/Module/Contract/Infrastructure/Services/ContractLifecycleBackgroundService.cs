using Contract.Interfaces.IRepository;
using Contract.Interfaces.Services;
using Shared.Interfaces;
using Shared.Logging;

namespace Contract.Infrastructure.Services
{
    public sealed class ContractLifecycleBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ContractLifecycleBackgroundService> logger,
        ILogPool logPool) : BackgroundService
    {
        private const string Module = "Contract";

        private const string Layer = "Infrastructure/Services";

        private readonly ILogPool _logPool = logPool ?? throw new ArgumentNullException(nameof(logPool));

        private readonly ILogger<ContractLifecycleBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

        private readonly HashSet<Guid> _synchronizedContracts = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            do
            {
                await ProcessContractsAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task ProcessContractsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var contractRepository = scope.ServiceProvider.GetRequiredService<IContractRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var premiseSyncClient = scope.ServiceProvider.GetRequiredService<IContractPremiseSyncClient>();

            var expiredCount = await contractRepository.ExpireDueSignedContractsAsync(
                DateTime.UtcNow, cancellationToken);
            if (expiredCount > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Expired {expiredCount} signed contracts.");
            }

            var endedContractIds = await contractRepository.GetEndedContractIdsAsync(cancellationToken);
            foreach (var contractId in endedContractIds)
            {
                if (_synchronizedContracts.Contains(contractId))
                {
                    continue;
                }

                try
                {
                    await premiseSyncClient.ReleasePremisesAsync(contractId, cancellationToken);
                    _synchronizedContracts.Add(contractId);
                }
                catch (HttpRequestException exception)
                {
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        $"Could not release premises for ended contract {contractId}; it will be retried.");
                }
                catch (InvalidOperationException exception)
                {
                    _logger.LogLayerError(_logPool, Module, Layer, exception,
                        $"Premise release is not configured for ended contract {contractId}; it will be retried.");
                }
            }
        }
    }
}