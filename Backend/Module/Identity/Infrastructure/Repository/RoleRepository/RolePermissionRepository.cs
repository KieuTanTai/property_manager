using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Role;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;

namespace Identity.Infrastructure.Repository.RoleRepository
{
    public class RolePermissionRepository(
        IdentityDbContext context,
        ILogger<RolePermissionRepository> logger,
        ILogPool logPool) : IBaseAssociativeRepository<RolePermissionModel, Guid>
    {
        private const string Module = "Identity";

        private const string Layer = "Infrastructure/Repository/Role";

        private readonly IdentityDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<RolePermissionRepository> _logger = logger;

        #region POST

        public async Task AddAsync(RolePermissionModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing role permission association creation.");
            if (entity.RoleId == Guid.Empty || entity.PermissionId == Guid.Empty)
            {
                var exception = new ArgumentException("RoleModel and PermissionModel id is required.", nameof(entity));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Role permission association rejected because an id is missing.");
                throw exception;
            }
            var existedRolePermission = await GetByIdAsync(entity.RoleId, entity.PermissionId, cancellationToken);
            if (existedRolePermission is not null)
            {
                var exception = new InvalidOperationException("RoleModel permission already exist!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Role permission association rejected because it already exists.");
                throw exception;
            }
            await _db.RolePermissions.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Role permission association staged for creation.");
        }

        public async Task AddRangeAsync(List<RolePermissionModel> entities, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing role permission association batch creation.");
            if (entities.Count == 0)
            {
                var exception = new ArgumentException("Entities is required.", nameof(entities));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Role permission association batch rejected because it is empty.");
                throw exception;
            }
            // var filteredRolePermissions = await GetNotExistedEntitiesList(entities, cancellationToken);
            // if (!filteredRolePermissions.Any())
            //     throw new InvalidOperationException("All role permissions already exist!");
            // await _db.RolePermissions.AddRangeAsync(filteredRolePermissions, cancellationToken);

            await _db.RolePermissions.AddRangeAsync(entities, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Role permission associations staged for creation.");
        }

        #endregion

        // private check if role and permission are existed
        // private async Task<List<RolePermissionModel>> GetNotExistedEntitiesList(List<RolePermissionModel> entities,
        //     CancellationToken cancellationToken = default)
        // {
        //     var existedRolePermissions = await GetAllAsync(cancellationToken);
        //     return entities.Where(entity => !existedRolePermissions.Any(existedRolePermission =>
        //             existedRolePermission.RoleId == entity.RoleId && existedRolePermission.PermissionId == entity.PermissionId))
        //         .ToList();
        // }

        #region DELETE

        public async Task DeleteAsync(Guid firstForeignId, Guid secondForeignId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all role permission associations.");
            if (firstForeignId == Guid.Empty || secondForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("First foreign id and second foreign id is required.", nameof(firstForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Role permission deletion rejected because an id is missing.");
                throw exception;
            }
            var existedRolePermission = await GetByIdAsync(firstForeignId, secondForeignId, cancellationToken);
            if (existedRolePermission is null)
            {
                var exception = new InvalidOperationException("RoleModel permission not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Role permission association was not found.");
                throw exception;
            }
            _db.RolePermissions.Remove(existedRolePermission);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Role permission association staged for deletion.");
        }

        public async Task DeleteByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role permission association.");
            if (firstForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("First foreign id is required.", nameof(firstForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Role permission deletion rejected because an id is missing.");
                throw exception;
            }
            var rolePermissionsToDelete = await _db.RolePermissions.Where(rp => rp.RoleId == firstForeignId).ToListAsync(cancellationToken);
            _db.RolePermissions.RemoveRange(rolePermissionsToDelete);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Role permission associations staged for deletion.");
        }

        public async Task DeleteBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role permission associations by permission.");
            if (secondForeignId == Guid.Empty)
            {
                var exception = new ArgumentException("Second foreign id is required.", nameof(secondForeignId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Role permission deletion rejected because an id is missing.");
                throw exception;
            }
            var rolePermissionsToDelete = await _db.RolePermissions.Where(rp => rp.PermissionId == secondForeignId).ToListAsync(cancellationToken);
            _db.RolePermissions.RemoveRange(rolePermissionsToDelete);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Role permission associations staged for deletion.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<RolePermissionModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all role permission associations.");
            return await _db.RolePermissions.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RolePermissionModel>> GetByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role permission associations by first foreign id.");
            return await _db.RolePermissions.AsNoTracking().Where(rolePermission => rolePermission.RoleId == firstForeignId).ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RolePermissionModel>> GetBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role permission associations by second foreign id.");
            return await _db.RolePermissions.AsNoTracking().Where(rolePermission => rolePermission.PermissionId == secondForeignId).ToListAsync(cancellationToken);
        }

        public async Task<RolePermissionModel?> GetByIdAsync(Guid firstForeignId, Guid secondForeignId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role permission association by first and second foreign id.");
            return await _db.RolePermissions.AsNoTracking().FirstOrDefaultAsync(
                rolePermission =>
                    rolePermission.RoleId == firstForeignId && rolePermission.PermissionId == secondForeignId,
                cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid firstForeignId, Guid secondForeignId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking role permission association existence.");
            return await _db.RolePermissions.AnyAsync(
                rolePermission =>
                    rolePermission.RoleId == firstForeignId && rolePermission.PermissionId == secondForeignId,
                cancellationToken);
        }

        #endregion
    }
}
