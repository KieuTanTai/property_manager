using Contract.Interfaces.IRepository;
using Contract.Interfaces.Services;
using Shared.Interfaces;

namespace Contract.Infrastructure.Services
{
    public sealed class ContractLifecycleBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ContractLifecycleBackgroundService> logger) : BackgroundService
    {
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
            using var scope = scopeFactory.CreateScope();
            var contractRepository = scope.ServiceProvider.GetRequiredService<IContractRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var premiseSyncClient = scope.ServiceProvider.GetRequiredService<IContractPremiseSyncClient>();

            var expiredCount = await contractRepository.ExpireDueSignedContractsAsync(
                DateTime.UtcNow, cancellationToken);
            if (expiredCount > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Expired {ContractCount} signed contracts.", expiredCount);
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
                    logger.LogError(exception,
                        "Could not release premises for ended contract {ContractId}; it will be retried.",
                        contractId);
                }
                catch (InvalidOperationException exception)
                {
                    logger.LogError(exception,
                        "Premise release is not configured for ended contract {ContractId}; it will be retried.",
                        contractId);
                }
            }
        }
    }
}
