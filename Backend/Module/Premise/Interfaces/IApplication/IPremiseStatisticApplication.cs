namespace Premise.Interfaces.IApplication;

public interface IPremiseStatisticApplication
{
    Task<long> GetTotalPremiseCountAsync(CancellationToken cancellationToken = default);

    Task<long> GetTotalRentedPremiseCountAsync(CancellationToken cancellationToken = default);

    Task<long> GetTotalLocationCountAsync(CancellationToken cancellationToken = default);
}
