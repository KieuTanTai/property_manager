using Premise.Models.Product;

namespace Premise.Interfaces.Product
{
    public interface IProductBusinessTypeRepository
    {
        Task<IReadOnlyList<ProductBusinessTypeModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductBusinessTypeModel>> GetByProductIdAsync(
            Guid productId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductBusinessTypeModel>> GetByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductBusinessTypeModel>> GetByProductIdsAsync(
            IEnumerable<Guid> productIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductBusinessTypeModel>> GetByBusinessTypeIdsAsync(
            IEnumerable<Guid> businessTypeIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductBusinessTypeModel>> GetTrackedByProductIdAsync(
            Guid productId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductBusinessTypeModel>> GetTrackedByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default);

        Task<ProductBusinessTypeModel?> GetByIdsAsync(
            Guid productId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Guid productId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            ProductBusinessTypeModel entity,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<ProductBusinessTypeModel> entities,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid productId,
            Guid businessTypeId,
            CancellationToken cancellationToken = default);
    }
}
