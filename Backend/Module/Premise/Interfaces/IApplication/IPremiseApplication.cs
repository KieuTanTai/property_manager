using Premise.Models.Business;
using Premise.Models.Premise;
using Shared.Enum;
using Shared.Persistence.Record;

namespace Premise.Interfaces.IApplication;

public interface IPremiseApplication
{
    Task<IReadOnlyList<PremiseModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PremiseModel> GetByIdAsync(Guid premiseId, CancellationToken cancellationToken = default);

    Task<PremiseModel> GetByNameAsync(string premiseName, CancellationToken cancellationToken = default);

    Task<PremiseModel> GetDetailsByIdAsync(
        Guid premiseId,
        bool includeMedia = true,
        bool includeBusinessTypes = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PremiseModel>> GetByFloorAsync(int floor, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PremiseModel>> GetByLocationIdAsync(
        Guid locationId,
        CancellationToken cancellationToken = default);

    Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingAsync(
        Guid? cursor,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByStatusAsync(
        Guid? cursor,
        int pageSize,
        EPremiseStatus status,
        CancellationToken cancellationToken = default);

    Task<RecordBaseCursorPage<PremiseModel>> GetApplyPagingByNameAsync(
        Guid? cursor,
        int pageSize,
        string premiseName,
        CancellationToken cancellationToken = default);

    Task<PremiseModel> AddAsync(PremiseModel premise, CancellationToken cancellationToken = default);

    Task<PremiseModel> UpdateAsync(PremiseModel premise, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid premiseId, CancellationToken cancellationToken = default);

    Task<PremiseMediaModel> AddImageAsync(
        Guid premiseId,
        string imageUrl,
        CancellationToken cancellationToken = default);

    Task<PremiseMediaModel> UpdateImageAsync(
        Guid premiseId,
        int imageId,
        string imageUrl,
        CancellationToken cancellationToken = default);

    Task DeleteImageAsync(
        Guid premiseId,
        int imageId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PremiseMediaModel>> GetImagesAsync(
        Guid premiseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessTypeModel>> GetBusinessTypesAsync(
        Guid premiseId,
        CancellationToken cancellationToken = default);

    Task<PremiseBusinessTypeModel> AddBusinessTypeAsync(
        Guid premiseId,
        Guid businessTypeId,
        CancellationToken cancellationToken = default);

    Task RemoveBusinessTypeAsync(
        Guid premiseId,
        Guid businessTypeId,
        CancellationToken cancellationToken = default);
}
