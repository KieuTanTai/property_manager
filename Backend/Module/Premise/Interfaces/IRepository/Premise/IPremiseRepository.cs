using Premise.Models.Premise;
using Shared.Enum;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Premise.Interfaces.IRepository.Premise
{
    public interface IPremiseRepository : IBaseReadRepository<PremiseModel, Guid>, IBaseUpsertRepository<PremiseModel>
    {
        Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByStatusAsync(Guid? cursor, int pageSize,
            EPremiseStatus status, CancellationToken cancellationToken = default);

        Task<PremiseModel> GetPremiseByNameAsync(string premiseName, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByNameAsync(Guid? cursor, int pageSize, string premiseName,
            CancellationToken cancellationToken = default);

        Task<PremiseModel> GetTrackedPremiseByNameAsync(string premiseName, CancellationToken cancellationToken = default);

        Task<PremiseModel?> GetPremiseAndNavigationByIdAsync(Guid id, bool isGetLocation = true, bool isGetMedia = false,
            bool isGetBusinessTypes = false, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseModel>> GetPremisesByFloorAsync(int floor, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PremiseModel>> GetPremisesByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default);

        Task DeletePremiseBusinessTypesByPremiseIdAsync(Guid premiseId, CancellationToken cancellationToken = default);

        Task DeleteAsync(PremiseModel premise, CancellationToken cancellationToken = default);

        Task<long> CountTotalPremiseAsync(CancellationToken cancellationToken = default);

        Task<long> CountTotalRentedPremiseAsync(CancellationToken cancellationToken = default);
    }
}