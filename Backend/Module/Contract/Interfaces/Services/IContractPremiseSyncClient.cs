namespace Contract.Interfaces.Services
{
    public interface IContractPremiseSyncClient
    {
        Task ReleasePremisesAsync(
            Guid contractId,
            CancellationToken cancellationToken = default);
    }
}
