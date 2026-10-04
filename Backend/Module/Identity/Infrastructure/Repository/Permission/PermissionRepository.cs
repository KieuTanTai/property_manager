using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Permission;
using Identity.Utils.Enum;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;

namespace Identity.Infrastructure.Repository.Permission
{
    public class PermissionRepository(
        IdentityDbContext context,
        ILogger<PermissionRepository> logger,
        ILogPool logPool)
        : IBaseAuthorizationRepository<PermissionModel, ESystemPermissionCode, Guid>
    {
        private const string Module = "Identity";

        private const string Layer = "Infrastructure/Repository/Permission";

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<PermissionRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<PermissionModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all permissions.");
            return await context.Permissions.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<PermissionModel?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading permission by id.");
            return await context.Permissions.AsNoTracking()
                .FirstOrDefaultAsync(permission => permission.PermissionId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<PermissionModel>> GetByIdsAsync(IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading permissions by ids.");
            return await context.Permissions.AsNoTracking()
                .Where(permission => ids.Contains(permission.PermissionId)).ToListAsync(cancellationToken);
        }

        public async Task<PermissionModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked permission.");
            return await context.Permissions.FirstOrDefaultAsync(permission => permission.PermissionId == id,
                cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking permission existence.");
            return await context.Permissions.AnyAsync(permission => permission.PermissionId == id, cancellationToken);
        }

        public async Task<PermissionModel?> GetByCodeAsync(ESystemPermissionCode permissionCode, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading permission by code.");
            var code = permissionCode.ToString().ToLower();
            return await context.Permissions.AsNoTracking().FirstOrDefaultAsync(permission => permission.PermissionCode.Contains(code), cancellationToken);
        }

        public async Task<IReadOnlyList<PermissionModel>> GetByNameAsync(string name,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading permissions by name.");
            return await context.Permissions.AsNoTracking().Where(permission => permission.PermissionName == name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PermissionModel>> GetByDescriptionAsync(string description,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading permissions by description.");
            return await context.Permissions.AsNoTracking()
                .Where(permission => permission.PermissionDescription == description).ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PermissionModel>> GetByActiveStatus(bool isActive,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading permissions by active status.");
            return await context.Permissions.AsNoTracking()
                .Where(permission => permission.PermissionIsActive == isActive).ToListAsync(cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(PermissionModel entity,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing to add permission.");
            if (string.IsNullOrWhiteSpace(entity.PermissionName))
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing to update permission.");
                throw new ArgumentException("PermissionModel name is required.", nameof(entity.PermissionName));
            }

            var isExisted = await context.Permissions.AnyAsync(
                existedPermission => existedPermission.PermissionName == entity.PermissionName, cancellationToken);

            if (isExisted)
            {
                throw new ArgumentException("PermissionModel name is already existed.", nameof(entity.PermissionName));
            }

            await context.Permissions.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Permission staged for creation.");
        }

        public async Task UpdateAsync(PermissionModel entity,
            CancellationToken cancellationToken = default)
        {
            if (entity.PermissionId == Guid.Empty)
            {
                throw new ArgumentException("PermissionModel id is required.", nameof(entity.PermissionId));
            }

            var existedPermission = await context.Permissions.AsNoTracking().FirstOrDefaultAsync(
                existedPermission => existedPermission.PermissionId == entity.PermissionId, cancellationToken);

            if (existedPermission is null)
            {
                throw new InvalidOperationException("PermissionModel not found!");
            }

            context.Permissions.Update(entity);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Permission staged for update.");
        }

        #endregion
    }
}