using Microsoft.EntityFrameworkCore;
using Premise.Infrastructures.Persistence.DbContext;
using Premise.Interfaces.Location;
using Premise.Models.Premise;

namespace Premise.Infrastructures.Repository.PremiseRepository
{
    public class LocationRepository(PremiseDbContext context) : ILocationRepository
    {
        private readonly PremiseDbContext _db = context;

        #region GET

        public async Task<IReadOnlyList<LocationModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Locations.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<LocationModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Locations.AsNoTracking()
                .FirstOrDefaultAsync(location => location.LocationId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<LocationModel>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _db.Locations.AsNoTracking()
                .Where(location => ids.Contains(location.LocationId))
                .ToListAsync(cancellationToken);
        }

        public async Task<LocationModel?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Locations
                .FirstOrDefaultAsync(location => location.LocationId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Locations
                .AnyAsync(location => location.LocationId == id, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(LocationModel entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entity.LocationAddress))
            {
                throw new ArgumentException("LocationModel address is required.", nameof(entity.LocationAddress));
            }

            await _db.Locations.AddAsync(entity, cancellationToken);
        }

        public async Task UpdateAsync(LocationModel entity, CancellationToken cancellationToken = default)
        {
            if (entity.LocationId == Guid.Empty)
            {
                throw new ArgumentException("LocationModel id is required.", nameof(entity.LocationId));
            }

            var existedLocation = await _db.Locations.AsNoTracking()
                .FirstOrDefaultAsync(location => location.LocationId == entity.LocationId, cancellationToken);

            if (existedLocation is null)
            {
                throw new InvalidOperationException($"LocationModel not found! \n {entity.LocationId}");
            }

            _db.Locations.Update(entity);
        }

        #endregion
    }
}