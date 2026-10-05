using Contract.Models.Contract;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IRepository
{
    public interface IContractViolationRepository :
        IBaseReadRepository<ContractViolationModel, Guid>,
        IBasePostRepository<ContractViolationModel>
    {
        Task<IReadOnlyList<ContractViolationModel>> GetByContractIdAsync(Guid contractId,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingAsync(Guid? cursor,
            int pageSize, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByViolationContentAsync(
            Guid? cursor, int pageSize, string violationContent,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByRangeViolationDateAsync(
            Guid? cursor, int pageSize, DateTime minViolationDate, DateTime maxViolationDate,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize, bool isResolved, CancellationToken cancellationToken = default);
    }
}
