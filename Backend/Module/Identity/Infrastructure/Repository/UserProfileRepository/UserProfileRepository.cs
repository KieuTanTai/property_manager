using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Profile;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Identity.Infrastructure.Repository.UserProfileRepository
{
    public class UserProfileRepository(
        IdentityDbContext context,
        ILogger<UserProfileRepository> logger,
        ILogPool logPool) : IUserProfileRepository
    {
        private const string Module = "Identity";

        private const string Layer = "Infrastructure/Repository/UserProfileRepository";

        private readonly IdentityDbContext _db = context;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<UserProfileRepository> _logger = logger;

        #region GET

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByFirstNameAsync(Guid? cursor, string firstName, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profiles by first name.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile paging rejected because the page size is invalid."));
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileFirstName != null && profile.UserProfileFirstName.Contains(firstName));
            query = query.OrderByDescending(profile => profile.UserProfileId).Take(pageSize + 1);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByLastNameAsync(Guid? cursor, string lastName, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profiles by name.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile paging rejected because the page size is invalid."));
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileLastName != null && profile.UserProfileLastName.Contains(lastName));
            query = query.OrderByDescending(profile => profile.UserProfileId).Take(pageSize + 1);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByNameAsync(Guid? cursor, string name, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profiles by birthday.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile paging rejected because the page size is invalid."));
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileFirstName != null && profile.UserProfileLastName != null && (profile.UserProfileFirstName + profile.UserProfileLastName).Contains(name));
            query = query.OrderByDescending(profile => profile.UserProfileId).Take(pageSize + 1);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingAsync(Guid? cursor, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profiles by phone number.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile paging rejected because the page size is invalid."));
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.OrderByDescending(profile => profile.UserProfileId).Take(pageSize + 1);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByUserBirthdayAsync(Guid? cursor, DateTime birthday, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all profiles.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile paging rejected because the page size is invalid."));
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileDateOfBirth == birthday);
            query = query.OrderByDescending(profile => profile.UserProfileId).Take(pageSize + 1);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByUserGenderAsync(Guid? cursor, string gender, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profiles by ids.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile paging rejected because the page size is invalid."));
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileGender.ToString() == gender);
            query = query.OrderByDescending(profile => profile.UserProfileId).Take(pageSize + 1);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByPhoneNumberAsync(Guid? cursor, string phoneNumber, int pageSize,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profiles by phone number.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile paging rejected because the page size is invalid."));
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfilePhoneNumber != null && profile.UserProfilePhoneNumber.Contains(phoneNumber));
            query = query.OrderByDescending(profile => profile.UserProfileId).Take(pageSize + 1);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByAddressAsync(Guid? cursor, string address, int pageSize, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profiles by address.");
            SharedGetApplyPagingRepository.ValidatePageSize(pageSize,
                exception => _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile paging rejected because the page size is invalid."));
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileAddress != null && profile.UserProfileAddress.Contains(address));
            query = query.OrderByDescending(profile => profile.UserProfileId).Take(pageSize + 1);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<IReadOnlyList<UserProfileModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all profiles.");
            return await _db.UserProfiles.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<UserProfileModel?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profile by id.");
            return await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(profile => profile.UserProfileId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<UserProfileModel>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading profiles by ids.");
            return await _db.UserProfiles.AsNoTracking().Where(profile => ids.Contains(profile.UserProfileId)).ToListAsync(cancellationToken);
        }

        public async Task<UserProfileModel?> GetUserProfileAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading user profile by account id.");
            return await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(profile => profile.UserProfileAccountId == accountId, cancellationToken);
        }

        public async Task<UserProfileModel?> GetTrackedByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tracked profile by id.");
            return await _db.UserProfiles.FirstOrDefaultAsync(profile => profile.UserProfileId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Checking profile existence.");
            return await _db.UserProfiles.AnyAsync(profile => profile.UserProfileId == id, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(UserProfileModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Adding new profile.");
            if (string.IsNullOrWhiteSpace(entity.UserProfileId))
            {
                var exception = new ArgumentException("UserProfileModel id is required.",
                    nameof(entity.UserProfileId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile creation rejected because the profile id is missing.");
                throw exception;
            }
            var existedProfile = await _db.UserProfiles.AnyAsync(existedProfile => existedProfile.UserProfileId == entity.UserProfileId, cancellationToken);
            if (existedProfile)
            {
                var exception = new InvalidOperationException("UserProfileModel already exist!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile creation rejected because the profile already exists.");
                throw exception;
            }

            await _db.UserProfiles.AddAsync(entity, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile staged for creation.");
        }

        public async Task UpdateAsync(UserProfileModel entity, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating profile.");
            if (string.IsNullOrWhiteSpace(entity.UserProfileId))
            {
                var exception = new ArgumentException("UserProfileModel id is required.",
                    nameof(entity.UserProfileId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile update rejected because the profile id is missing.");
                throw exception;
            }

            var existedProfile = await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(existedProfile => existedProfile.UserProfileId == entity.UserProfileId, cancellationToken);
            if (existedProfile is null)
            {
                var exception = new InvalidOperationException("UserProfileModel not found!");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Profile update rejected because the profile was not found.");
                throw exception;
            }
            _db.UserProfiles.Update(entity);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile staged for update.");
        }

        #endregion
    }
}
