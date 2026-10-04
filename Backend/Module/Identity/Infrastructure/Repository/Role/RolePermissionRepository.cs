using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Role;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Logging;

namespace Identity.Infrastructure.Repository.Role
{
    public class RolePermissionRepository(
        IdentityDbContext context,
        ILogger<RolePermissionRepository> logger,
        ILogPool logPool) : IBaseAssociativeRepository<RolePermissionModel, Guid>
    {
        private readonly IdentityDbContext _db = context;
        private readonly ILogger<RolePermissionRepository> _logger = logger;
        private readonly ILogPool _logPool = logPool;
        private const string Module = "identity";
        private const string Layer = "repository";

        #region POST

        public async Task AddAsync(RolePermissionModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing role permission association creation.");
            if (entity.RoleId == Guid.Empty || entity.PermissionId == Guid.Empty)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing role permission association batch creation.");
                throw new ArgumentException("RoleModel and PermissionModel id is required.", nameof(entity));
            }
            var existedRolePermission = await GetByIdAsync(entity.RoleId, entity.PermissionId, cancellationToken);
            if (existedRolePermission is not null)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing role permission association deletion.");
                throw new InvalidOperationException("RoleModel permission already exist!");
            }
            await _db.RolePermissions.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(List<RolePermissionModel> entities, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Role permission associations staged for deletion by role.");
            if (entities.Count == 0)
            {
                _logger.LogLayerInformation(_logPool, Module, Layer, "Role permission associations staged for deletion by permission.");
                throw new ArgumentException("Entities is required.", nameof(entities));
            }
            // var filteredRolePermissions = await GetNotExistedEntitiesList(entities, cancellationToken);
            // if (!filteredRolePermissions.Any())
            //     throw new InvalidOperationException("All role permissions already exist!");
            // await _db.RolePermissions.AddRangeAsync(filteredRolePermissions, cancellationToken);

            await _db.RolePermissions.AddRangeAsync(entities, cancellationToken);
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
                _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role permission associations by role.");
                throw new ArgumentException("First foreign id and second foreign id is required.", nameof(firstForeignId));
            }
            var existedRolePermission = await GetByIdAsync(firstForeignId, secondForeignId, cancellationToken);
            if (existedRolePermission is null)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role permission associations by permission.");
                throw new InvalidOperationException("RoleModel permission not found!");
            }
            _db.RolePermissions.Remove(existedRolePermission);
        }

        public async Task DeleteByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role permission association.");
            if (firstForeignId == Guid.Empty)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Checking role permission association.");
                throw new ArgumentException("First foreign id is required.", nameof(firstForeignId));
            }
            var rolePermissionsToDelete = await _db.RolePermissions.Where(rp => rp.RoleId == firstForeignId).ToListAsync(cancellationToken);
            _db.RolePermissions.RemoveRange(rolePermissionsToDelete);
        }

        public async Task DeleteBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            if (secondForeignId == Guid.Empty)
            {
                throw new ArgumentException("Second foreign id is required.", nameof(secondForeignId));
            }
            var rolePermissionsToDelete = await _db.RolePermissions.Where(rp => rp.PermissionId == secondForeignId).ToListAsync(cancellationToken);
            _db.RolePermissions.RemoveRange(rolePermissionsToDelete);
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<RolePermissionModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.RolePermissions.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RolePermissionModel>> GetByFirstForeignIdAsync(Guid firstForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.RolePermissions.AsNoTracking().Where(rolePermission => rolePermission.RoleId == firstForeignId).ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<RolePermissionModel>> GetBySecondForeignIdAsync(Guid secondForeignId, CancellationToken cancellationToken = default)
        {
            return await _db.RolePermissions.AsNoTracking().Where(rolePermission => rolePermission.PermissionId == secondForeignId).ToListAsync(cancellationToken);
        }
        
        public async Task<RolePermissionModel?> GetByIdAsync(Guid firstForeignId, Guid secondForeignId,
            CancellationToken cancellationToken = default)
        {
            return await _db.RolePermissions.AsNoTracking().FirstOrDefaultAsync(
                rolePermission =>
                    rolePermission.RoleId == firstForeignId && rolePermission.PermissionId == secondForeignId,
                cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid firstForeignId, Guid secondForeignId,
            CancellationToken cancellationToken = default)
        {
            return await _db.RolePermissions.AnyAsync(
                rolePermission =>
                    rolePermission.RoleId == firstForeignId && rolePermission.PermissionId == secondForeignId,
                cancellationToken);
        }

        #endregion
    }
}