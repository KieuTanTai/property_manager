using Premise.Models.Product;
using Shared.Interfaces;

namespace Premise.Interfaces.IRepository.Product
{
    public interface IProductBusinessTypeRepository : IBaseAssociativeRepository<ProductBusinessTypeModel, Guid>
    {
        Task DeleteAsync(Guid productId, Guid businessTypeId, CancellationToken cancellationToken = default);

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
    }
}
