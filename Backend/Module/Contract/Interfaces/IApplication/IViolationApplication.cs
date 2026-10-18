using Contract.Models.Contract;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IApplication
{
    public interface IViolationApplication
    {
        Task<IReadOnlyList<ContractViolationModel>> GetViolationsByContractIdAsync(Guid contractId,
            CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByViolationContentAsync(
            Guid? cursor, int pageSize, string violationContent,
            CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByViolationDateRangeAsync(
            Guid? cursor, int pageSize, DateTime minViolationDate, DateTime maxViolationDate,
            CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<ContractViolationModel>> GetApplyPagingByViolationStatusAsync(
            Guid? cursor, int pageSize, bool isResolved,
            CancellationToken cancellationToken = default);

        Task<int> AddViolationAsync(ContractViolationModel violation, CancellationToken cancellationToken = default);
        Task<int> UpdateViolationAsync(ContractViolationModel violation, CancellationToken cancellationToken = default);
        Task<int> DeleteViolationAsync(Guid violationId, CancellationToken cancellationToken = default);
    }
}