using Premise.Interfaces.IApplication;
using Premise.Interfaces.IRepository.BusinessType;
using Premise.Models.Business;
using Shared.Logging;

namespace Premise.Application;

public sealed class BusinessTypeApplication(
    ILogger<BusinessTypeApplication> logger,
    ILogPool logPool,
    IBusinessTypeRepository businessTypeRepository) : IBusinessTypeApplication
{
    private const string Module = "Premise";
    private const string Layer = "Application";

    public async Task<IReadOnlyList<BusinessTypeModel>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogLayerDebug(logPool, Module, Layer, "Loading all business types.");
        return await businessTypeRepository.GetAllAsync(cancellationToken);
    }
}
