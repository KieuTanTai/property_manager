using Premise.Interfaces.IApplication;
using Premise.Interfaces.IRepository.BusinessType;
using Premise.Models.Business;
using Shared.Interfaces;
using Shared.Logging;

namespace Premise.Application
{
    public sealed class BusinessTypeApplication(
        ILogger<BusinessTypeApplication> logger,
        ILogPool logPool,
        IBusinessTypeRepository businessTypeRepository,
        IUnitOfWork unitOfWork) : IBusinessTypeApplication
    {
        private const string Module = "Premise";

        private const string Layer = "Application";

        public async Task<BusinessTypeModel> AddAsync(
            string name,
            string? description,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Business type name is required.", nameof(name));
            }

            var businessType = new BusinessTypeModel(name);
            if (!string.IsNullOrWhiteSpace(description))
            {
                businessType.SetBusinessTypeDescription(description);
            }
            businessType.SetBusinessTypeIsActive(isActive);

            await businessTypeRepository.AddAsync(businessType, cancellationToken);
            await SaveChangesOrThrowAsync(cancellationToken);
            return businessType;
        }

        public async Task<BusinessTypeModel> UpdateAsync(
            Guid id,
            string name,
            string? description,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Business type id is required.", nameof(id));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Business type name is required.", nameof(name));
            }

            var businessType = await businessTypeRepository.GetTrackedByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Business type not found: {id}.");
            businessType.SetBusinessTypeName(name);
            if (string.IsNullOrWhiteSpace(description))
            {
                businessType.ClearBusinessTypeDescription();
            }
            else
            {
                businessType.SetBusinessTypeDescription(description);
            }
            businessType.SetBusinessTypeIsActive(isActive);

            await businessTypeRepository.UpdateAsync(businessType, cancellationToken);
            await SaveChangesOrThrowAsync(cancellationToken);
            return businessType;
        }

        public async Task<BusinessTypeModel> SetActiveAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Business type id is required.", nameof(id));
            }

            var businessType = await businessTypeRepository.GetTrackedByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Business type not found: {id}.");
            businessType.SetBusinessTypeIsActive(isActive);
            await businessTypeRepository.UpdateAsync(businessType, cancellationToken);
            await SaveChangesOrThrowAsync(cancellationToken);
            return businessType;
        }

        public async Task<IReadOnlyList<BusinessTypeModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            logger.LogLayerDebug(logPool, Module, Layer, "Loading all business types.");
            return await businessTypeRepository.GetAllAsync(cancellationToken);
        }

        private async Task SaveChangesOrThrowAsync(CancellationToken cancellationToken)
        {
            if (await unitOfWork.SaveChangesAsync(cancellationToken) == 0)
            {
                throw new InvalidOperationException("Failed to save business type.");
            }
        }
    }
}