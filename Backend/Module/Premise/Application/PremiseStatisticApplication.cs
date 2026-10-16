using Premise.Interfaces.IRepository.Location;
using Premise.Interfaces.IRepository.Premise;
using Premise.Interfaces.IApplication;
using Shared.Logging;

namespace Premise.Application
{
    public class PremiseStatisticApplication
    (
        ILogger<PremiseStatisticApplication> logger,
        ILogPool logPool,
        IPremiseRepository premiseRepository,
        ILocationRepository locationRepository
    ) : IPremiseStatisticApplication
    {
        private readonly ILogger<PremiseStatisticApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        private readonly IPremiseRepository _premiseRepository = premiseRepository;
        private readonly ILocationRepository _locationRepository = locationRepository;
        private const string Module = "Statistic";
        private const string Layer = "Application";

        public async Task<long> GetTotalPremiseCountAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading total premise count.");
            return await _premiseRepository.CountTotalPremiseAsync(cancellationToken);
        }

        public async Task<long> GetTotalRentedPremiseCountAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading total rented premise count.");
            return await _premiseRepository.CountTotalRentedPremiseAsync(cancellationToken);
        }

        public async Task<long> GetTotalLocationCountAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading total location count.");
            return await _locationRepository.CountTotalAsync(cancellationToken);
        }
    }
}