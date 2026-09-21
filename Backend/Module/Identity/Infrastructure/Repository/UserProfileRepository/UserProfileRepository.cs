using Identity.Infrastructure.Persistence.DbContext;
using Identity.Interfaces.IRepository;
using Identity.Models.Profile;
using Microsoft.EntityFrameworkCore;
using Shared.Persistence;
using Shared.Persistence.Record;

namespace Identity.Infrastructure.Repository.UserProfileRepository
{
    public class UserProfileRepository(IdentityDbContext context) : IUserProfileRepository
    {
        private readonly IdentityDbContext _db = context;

        #region GET

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByFirstNameAsync(Guid? cursor, string firstName, int pageSize, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileFirstName != null && profile.UserProfileFirstName.Contains(firstName));
            query = query.OrderByDescending(profile => profile.UserProfileId);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByLastNameAsync(Guid? cursor, string lastName, int pageSize, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileLastName != null && profile.UserProfileLastName.Contains(lastName));
            query = query.OrderByDescending(profile => profile.UserProfileId);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByNameAsync(Guid? cursor, string name, int pageSize, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileFirstName != null && profile.UserProfileLastName != null && (profile.UserProfileFirstName + profile.UserProfileLastName).Contains(name));
            query = query.OrderByDescending(profile => profile.UserProfileId);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingAsync(Guid? cursor, int pageSize, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.OrderByDescending(profile => profile.UserProfileId);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByUserBirthdayAsync(Guid? cursor, DateTime birthday, int pageSize, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileDateOfBirth == birthday);
            query = query.OrderByDescending(profile => profile.UserProfileId);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByUserGenderAsync(Guid? cursor, string gender, int pageSize, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileGender.ToString() == gender);
            query = query.OrderByDescending(profile => profile.UserProfileId);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByPhoneNumberAsync(Guid? cursor, string phoneNumber, int pageSize,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfilePhoneNumber != null && profile.UserProfilePhoneNumber.Contains(phoneNumber));
            query = query.OrderByDescending(profile => profile.UserProfileId);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByAddressAsync(Guid? cursor, string address, int pageSize, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            var query = _db.UserProfiles.AsNoTracking();
            if (cursor.HasValue)
            {
                query = query.Where(profile => profile.UserProfileAccountId < cursor.Value);
            }
            query = query.Where(profile => profile.UserProfileAddress != null && profile.UserProfileAddress.Contains(address));
            query = query.OrderByDescending(profile => profile.UserProfileId);
            var profiles = query.ToAsyncEnumerable();
            return await SharedGetApplyPagingRepository.ApplyPaging(profiles, pageSize, profile => profile.UserProfileAccountId,
                cancellationToken);
        }

        public async Task<IReadOnlyList<UserProfileModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.UserProfiles.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<UserProfileModel?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(profile => profile.UserProfileId == id, cancellationToken);
        }

        public async Task<UserProfileModel?> GetUserProfileAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            return await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(profile => profile.UserProfileAccountId == accountId, cancellationToken);
        }

        public async Task<UserProfileModel?> GetTrackedByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _db.UserProfiles.FirstOrDefaultAsync(profile => profile.UserProfileId == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _db.UserProfiles.AnyAsync(profile => profile.UserProfileId == id, cancellationToken);
        }

        #endregion

        #region POST

        public async Task AddAsync(UserProfileModel entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entity.UserProfileId))
            {
                throw new ArgumentException("UserProfileModel id is required.", nameof(entity.UserProfileId));
            }
            var existedProfile = await _db.UserProfiles.AnyAsync(existedProfile => existedProfile.UserProfileId == entity.UserProfileId, cancellationToken);
            if (existedProfile)
            {
                throw new InvalidOperationException("UserProfileModel already exist!");
            }

            await _db.UserProfiles.AddAsync(entity, cancellationToken);
        }

        public async Task UpdateAsync(UserProfileModel entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entity.UserProfileId))
            {
                throw new ArgumentException("UserProfileModel id is required.", nameof(entity.UserProfileId));
            }

            var existedProfile = await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(existedProfile => existedProfile.UserProfileId == entity.UserProfileId, cancellationToken);
            if (existedProfile is null)
            {
                throw new InvalidOperationException("UserProfileModel not found!");
            }
            Console.WriteLine("UserProfileModel found!");
            _db.UserProfiles.Update(entity);
        }

        #endregion
    }
}