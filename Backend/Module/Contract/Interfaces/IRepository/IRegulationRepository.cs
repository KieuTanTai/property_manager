using Contract.Models.Contract;
using Contract.Models.Regulation;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IRepository
{
    public interface IRegulationRepository : IBaseReadRepository<RegulationModel, Guid>, IBasePostRepository<RegulationModel>
    {
        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByNameAsync(Guid? cursor, int pageSize,
            string regulationName, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize,
            bool isActive, CancellationToken cancellationToken = default);
        
        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByRangeFineAmountAsync(Guid? cursor, int pageSize, decimal minFineAmount, decimal maxFineAmount,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByRangeCreatedAtAsync(Guid? cursor, int pageSize, DateTime minCreatedAt, DateTime maxCreatedAt,
            CancellationToken cancellationToken = default);
 
        Task<RegulationModel> GetRegulationByNameAsync(string regulationName,
            CancellationToken cancellationToken = default); 
        
        Task<RegulationModel> GetTrackedRegulationByNameAsync(string regulationName,
            CancellationToken cancellationToken = default);
    }
}
