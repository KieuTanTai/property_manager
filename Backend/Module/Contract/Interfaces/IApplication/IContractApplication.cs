using Contract.Models.Contract;
using Contract.Utils.Enum;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IApplication
{
    public interface IContractApplication
    {
        Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            bool isGetViolations = false, bool isGetInvoices = false,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingByContractStatusAsync(Guid? cursor, int pageSize,
            EContractStatus status, bool isGetViolations = false, bool isGetInvoices = false,
            CancellationToken cancellationToken = default);

        Task<ContractModel?> GetByContractIdAsync(Guid contractId, bool isGetViolations = false,
            bool isGetInvoices = false, CancellationToken cancellationToken = default);

        Task<ContractModel> AddContractAsync(ContractModel contract,
            IReadOnlyCollection<Guid>? regulationIds = null,
            CancellationToken cancellationToken = default);

        Task<ContractModel> UpdateContractAsync(ContractModel contract,
            IReadOnlyCollection<Guid>? regulationIds = null,
            CancellationToken cancellationToken = default);
    }
}
