using Microsoft.EntityFrameworkCore;
using Premise.Infrastructures.Persistence.DbContext;
using Premise.Interfaces.BusinessType;
using Premise.Models.Business;

namespace Premise.Infrastructures.Repository.PremiseRepository
{
    public class BusinessTypeRepository(PremiseDbContext context) : IBusinessTypeRepository
    {
        private readonly PremiseDbContext _db = context;

        #region GET

        public async Task<IReadOnlyList<BusinessTypeModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.BusinessTypes.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<BusinessTypeModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.BusinessTypes.AsNoTracking()
                .FirstOrDefaultAsync(businessType => businessType.BusinessTypeId == id, cancellationToken);
        }

        public async Task<BusinessTypeModel?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.BusinessTypes
                .FirstOrDefaultAsync(businessType => businessType.BusinessTypeId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.BusinessTypes
                .AnyAsync(businessType => businessType.BusinessTypeId == id, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(BusinessTypeModel entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entity.BusinessTypeName))
            {
                throw new ArgumentException("BusinessTypeModel name is required.", nameof(entity.BusinessTypeName));
            }

            var isExisted = await _db.BusinessTypes.AnyAsync(
                businessType => businessType.BusinessTypeName == entity.BusinessTypeName,
                cancellationToken);

            if (isExisted)
            {
                throw new InvalidOperationException($"Already existed! \n {entity.BusinessTypeName}");
            }

            await _db.BusinessTypes.AddAsync(entity, cancellationToken);
        }

        public async Task UpdateAsync(BusinessTypeModel entity, CancellationToken cancellationToken = default)
        {
            if (entity.BusinessTypeId == Guid.Empty)
            {
                throw new ArgumentException("BusinessTypeModel id is required.", nameof(entity.BusinessTypeId));
            }

            var existedBusinessType = await _db.BusinessTypes.AsNoTracking()
                .FirstOrDefaultAsync(
                    businessType => businessType.BusinessTypeId == entity.BusinessTypeId,
                    cancellationToken);

            if (existedBusinessType is null)
            {
                throw new InvalidOperationException(
                    $"BusinessTypeModel not found! \n {entity.BusinessTypeId}");
            }

            var isNameUsed = await _db.BusinessTypes.AnyAsync(
                businessType => businessType.BusinessTypeId != entity.BusinessTypeId &&
                                businessType.BusinessTypeName == entity.BusinessTypeName,
                cancellationToken);

            if (isNameUsed)
            {
                throw new InvalidOperationException($"Already existed! \n {entity.BusinessTypeName}");
            }

            _db.BusinessTypes.Update(entity);
        }

        #endregion
    }
}