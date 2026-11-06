using Premise.Models.Business;

namespace Premise.Interfaces.IApplication
{
    public interface IBusinessTypeApplication
    {
        Task<IReadOnlyList<BusinessTypeModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<BusinessTypeModel> AddAsync(
            string name,
            string? description,
            bool isActive,
            CancellationToken cancellationToken = default);

        Task<BusinessTypeModel> UpdateAsync(
            Guid id,
            string name,
            string? description,
            bool isActive,
            CancellationToken cancellationToken = default);

        Task<BusinessTypeModel> SetActiveAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken = default);
    }
}