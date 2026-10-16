using Contract.Models.Regulation;
using Shared.Persistence.Record;

namespace Contract.Interfaces.IApplication
{
    public interface IRegulationApplication
    {
        Task<RegulationModel?> GetByRegulationIdAsync(Guid regulationId, CancellationToken cancellationToken = default);
        Task<RegulationModel> GetByRegulationNameAsync(string regulationName, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<RegulationModel>> GetApplyPagingByRegulationStatusAsync(Guid? cursor, int pageSize,
            bool isActive, CancellationToken cancellationToken = default);
        Task<RegulationModel> AddRegulationAsync(RegulationModel regulation, CancellationToken cancellationToken = default);
        Task<int> UpdateRegulationAsync(RegulationModel regulation, CancellationToken cancellationToken = default);
        Task<int> SoftDeleteRegulationAsync(Guid regulationId, CancellationToken cancellationToken = default);
    }
}
