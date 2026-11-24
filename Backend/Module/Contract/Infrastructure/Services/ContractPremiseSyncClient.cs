using Contract.Interfaces.Services;
using Contract.Infrastructure.Services.Options;
using Microsoft.Extensions.Options;
using Shared.Logging;

namespace Contract.Infrastructure.Services
{
    public sealed class ContractPremiseSyncClient(
        HttpClient httpClient,
        IOptions<PremiseSyncOptions> options,
        ILogger<ContractPremiseSyncClient> logger,
        ILogPool logPool) : IContractPremiseSyncClient
    {
        private const string Module = "Contract";
        private const string Layer = "Infrastructure/Services";
        private readonly PremiseSyncOptions _options = options.Value;

        public async Task ReleasePremisesAsync(
            Guid contractId,
            CancellationToken cancellationToken = default)
        {
            var baseUrl = _options.Premise;
            var apiKey = _options.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "InternalApi:Key must be configured in both Contract and Premise services.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"{baseUrl.TrimEnd('/')}/api/Premise/rental-sync/{contractId}");
            request.Headers.Add("X-Internal-Api-Key", apiKey);

            try
            {
                using var response = await httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogLayerInformation(logPool, Module, Layer,
                        $"Premises released for ended contract {contractId}.");
                    return;
                }

                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Premise release failed with HTTP {(int)response.StatusCode}: {detail}",
                    null,
                    response.StatusCode);
            }
            catch (Exception exception)
            {
                logger.LogLayerError(logPool, Module, Layer, exception,
                    $"Failed to release premises for ended contract {contractId}.");
                throw;
            }
        }
    }
}
