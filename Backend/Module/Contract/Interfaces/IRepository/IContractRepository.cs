using Contract.Models.Contract;
using Contract.Utils.Enum;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IRepository
{
    public interface IContractRepository : IBaseReadRepository<ContractModel, Guid>, IBaseUpsertRepository<ContractModel>
    {
        Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            bool isGetViolations, bool isGetInvoices, bool isGetRegulations,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize,
            bool isGetViolations, bool isGetInvoices, bool isGetRegulations,
            EContractStatus status, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingWithNavigationAsync(Guid? cursor, int pageSize,
            bool isGetViolations = true,
            bool isGetInvoices = false,
            bool isGetRegulations = false,
            CancellationToken cancellationToken = default);

        Task<ContractModel?> GetContractAndNavigationByIdAsync(Guid id,
            bool isGetViolations = true,
            bool isGetInvoices = false,
            bool isGetRegulations = false,
            CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(Guid contractId, DateTime deletionDate, CancellationToken cancellationToken = default);
    }
}