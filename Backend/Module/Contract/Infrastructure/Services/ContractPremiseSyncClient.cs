using Contract.Interfaces.Services;

namespace Contract.Infrastructure.Services
{
    public sealed class ContractPremiseSyncClient(
        HttpClient httpClient,
        IConfiguration configuration) : IContractPremiseSyncClient
    {
        public async Task ReleasePremisesAsync(
            Guid contractId,
            CancellationToken cancellationToken = default)
        {
            var baseUrl = configuration["ModuleUrls:Premise"] ?? "https://localhost:7081";
            var apiKey = configuration["InternalApi:Key"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "InternalApi:Key must be configured in both Contract and Premise services.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"{baseUrl.TrimEnd('/')}/api/Premise/rental-sync/{contractId}");
            request.Headers.Add("X-Internal-Api-Key", apiKey);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Premise release failed with HTTP {(int)response.StatusCode}: {detail}",
                null,
                response.StatusCode);
        }
    }
}
