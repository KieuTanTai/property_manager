using Contract.Models.Contract;
using Shared.Interfaces;

namespace Contract.Interfaces.IRepository
{
    public interface IContractRegulationRepository
        : IBaseAssociativeRepository<ContractRegulationModel, Guid>
    {
        Task DeleteAsync(Guid contractId, Guid regulationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ContractRegulationModel>> GetByContractIdAsync(Guid contractId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ContractRegulationModel>> GetByRegulationIdAsync(Guid regulationId,
            CancellationToken cancellationToken = default);
        Task DeleteByContractIdAsync(Guid contractId, CancellationToken cancellationToken = default);
    }
}
