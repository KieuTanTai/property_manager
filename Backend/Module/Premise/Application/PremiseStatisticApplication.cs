using Premise.Interfaces.IRepository.Premise;
using Premise.Interfaces.IRepository.Location;
using Shared.Logging;

namespace Statisic.Application
{
    public class PremiseStatisticApplication
    (
        ILogger<PremiseStatisticApplication> logger,
        ILogPool logPool,
        IPremiseRepository premiseRepository,
        ILocationRepository locationRepository
    )
    {
        private readonly ILogger<PremiseStatisticApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        private readonly IPremiseRepository _premiseRepository = premiseRepository;
        private readonly ILocationRepository _locationRepository = locationRepository;
        private const string Module = "Statistic";
        private const string Layer = "Application";

        public class 
    }
}