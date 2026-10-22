using Premise.Models.Product;
using Shared.Interfaces;

namespace Premise.Interfaces.IRepository.Product
{
    public interface IWhitelistProductRepository :
        IBaseReadRepository<WhitelistProductModel, Guid>,
        IBaseUpsertRepository<WhitelistProductModel>
    {
        Task<WhitelistProductModel?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WhitelistProductModel>> GetByBusinessTypeIdAsync(
            Guid businessTypeId,
            CancellationToken cancellationToken = default);
    }
}
