using Premise.Models.Business;
using Shared.Interfaces;

namespace Premise.Interfaces.IRepository.BusinessType
{
    public interface IBusinessTypeRepository : IBaseReadRepository<BusinessTypeModel, Guid>, IBaseUpsertRepository<BusinessTypeModel> {}
}