using Contract.Models.Regulation;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IApplication
{
    public interface IRegulationApplication
    {
        Task<IReadOnlyList<RegulationModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RegulationModel?> GetByIdAsync(Guid regulationId, CancellationToken cancellationToken = default);
        Task<RegulationModel> GetByNameAsync(string regulationName, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByNameAsync(Guid? cursor, int pageSize,
            string regulationName, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize,
            bool isActive, CancellationToken cancellationToken = default);
        Task<RegulationModel> AddAsync(RegulationModel regulation, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(RegulationModel regulation, CancellationToken cancellationToken = default);
        Task<int> SoftDeleteAsync(Guid regulationId, CancellationToken cancellationToken = default);
    }
}
