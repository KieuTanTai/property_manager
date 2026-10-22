using Premise.Models.Business;

namespace Premise.Interfaces.IApplication;

public interface IBusinessTypeApplication
{
    Task<IReadOnlyList<BusinessTypeModel>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
