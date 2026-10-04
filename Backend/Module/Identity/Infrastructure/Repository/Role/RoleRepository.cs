using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Role;
using Identity.Utils.Enum;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;

namespace Identity.Infrastructure.Repository.Role
{
    public class RoleRepository(
        IdentityDbContext context,
        ILogger<RoleRepository> logger,
        ILogPool logPool) : IBaseAuthorizationRepository<RoleModel, ESystemRoleCode, Guid>
    {
        private const string Module = "Identity";

        private const string Layer = "Infrastructure/Repository/Role";

        private readonly IdentityDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<RoleRepository> _logger = logger;

        #region GET

        public async Task<IReadOnlyList<RoleModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all roles.");
            return await _db.Roles
                .Include(role => role.Permissions)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<RoleModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role by id.");
            return await _db.Roles.AsNoTracking().FirstOrDefaultAsync(role => role.RoleId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<RoleModel>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading roles by ids.");
            return await _db.Roles.AsNoTracking().Where(role => ids.Contains(role.RoleId)).ToListAsync(cancellationToken);
        }

        public async Task<RoleModel?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked role.");
            return await _db.Roles.FirstOrDefaultAsync(role => role.RoleId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking role existence.");
            return await _db.Roles.AnyAsync(role => role.RoleId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<RoleModel>> GetByNameAsync(string name,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading roles by name.");
            return await _db.Roles.AsNoTracking().Where(role => role.RoleName == name).ToListAsync(cancellationToken);
        }

        public async Task<RoleModel?> GetByCodeAsync(ESystemRoleCode roleCode, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading role by code.");
            var code = roleCode.ToString().ToLower();
            return await _db.Roles
                .Include(role => role.Permissions)
                .AsNoTracking()
                .FirstOrDefaultAsync(role => role.RoleCode == code, cancellationToken);
        }
        public async Task<IReadOnlyList<RoleModel>> GetByDescriptionAsync(string description,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading roles by description.");
            return await _db.Roles.AsNoTracking().Where(role => role.RoleDescription == description)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RoleModel>> GetByActiveStatus(bool isActive,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading roles by active status.");
            return await _db.Roles.AsNoTracking().Where(role => role.RoleIsActive == isActive)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(RoleModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing to add role.");
            if (string.IsNullOrWhiteSpace(entity.RoleName))
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Preparing to update role.");
                throw new ArgumentException("RoleModel name is required.", nameof(entity.RoleName));
            }

            var isExisted = await _db.Roles.AnyAsync(existedRole => existedRole.RoleName == entity.RoleName,
                cancellationToken);

            if (isExisted)
            {
                throw new ArgumentException("RoleModel name is existed.", nameof(entity.RoleName));
            }

            await _db.Roles.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Role staged for creation.");
        }

        public async Task UpdateAsync(RoleModel entity, CancellationToken cancellationToken = default)
        {
            if (entity.RoleId == Guid.Empty)
            {
                throw new ArgumentException("RoleModel id is required.", nameof(entity.RoleId));
            }

            var existedRole = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(existedRole => existedRole.RoleId == entity.RoleId,
                cancellationToken);

            if (existedRole is null)
            {
                throw new InvalidOperationException("RoleModel not found!");
            }

            if (existedRole.RoleName == entity.RoleName && existedRole.RoleDescription == entity.RoleDescription && existedRole.RoleIsActive == entity.RoleIsActive)
            {
                throw new InvalidOperationException("No changes detected in the RoleModel.");
            }

            if (existedRole.RoleName != entity.RoleName)
            {
                var isExisted = await _db.Roles.AnyAsync(role => role.RoleName == entity.RoleName,
                    cancellationToken);

                if (isExisted)
                {
                    throw new ArgumentException("RoleModel name is existed.", nameof(entity.RoleName));
                }
            }
            _db.Roles.Update(entity);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Role staged for update.");
        }

        #endregion
    }
}