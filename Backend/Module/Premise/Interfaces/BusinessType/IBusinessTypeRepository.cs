using Premise.Models.Business;
using Shared.Interfaces;

namespace Premise.Interfaces.BusinessType
{
    public interface IBusinessTypeRepository : IBaseReadRepository<BusinessTypeModel, Guid>, IBasePostRepository<BusinessTypeModel> {}
}