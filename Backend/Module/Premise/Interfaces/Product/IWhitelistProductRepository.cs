using Premise.Models.Product;
using Shared.Interfaces;

namespace Premise.Interfaces.Product
{
    public interface IWhitelistProductRepository :
        IBaseReadRepository<WhitelistProductModel, Guid>,
        IBasePostRepository<WhitelistProductModel>
    {
        Task<WhitelistProductModel?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default);
    }
}
