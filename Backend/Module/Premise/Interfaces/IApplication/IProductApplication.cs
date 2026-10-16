using Premise.Models.Business;
using Premise.Models.Product;

namespace Premise.Interfaces.IApplication;

public interface IProductApplication
{
    Task<IReadOnlyList<WhitelistProductModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<WhitelistProductModel> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<WhitelistProductModel> AddAsync(
        string productName,
        string? productDescription,
        CancellationToken cancellationToken = default);

    Task<WhitelistProductModel> UpdateAsync(
        Guid productId,
        string productName,
        string? productDescription,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessTypeModel>> GetBusinessTypesAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WhitelistProductModel>> GetByBusinessTypeIdAsync(
        Guid businessTypeId,
        CancellationToken cancellationToken = default);

    Task<ProductBusinessTypeModel> AddBusinessTypeAsync(
        Guid productId,
        Guid businessTypeId,
        CancellationToken cancellationToken = default);

    Task RemoveBusinessTypeAsync(
        Guid productId,
        Guid businessTypeId,
        CancellationToken cancellationToken = default);
}
