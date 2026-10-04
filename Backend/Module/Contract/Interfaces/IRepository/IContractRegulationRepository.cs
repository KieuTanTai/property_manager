using Contract.Models.Contract;
using Shared.Interfaces;

namespace Contract.Interfaces.IRepository
{
    public interface IContractRegulationRepository
        : IBaseAssociativeRepository<ContractRegulationModel, Guid>
    {
    }
}
