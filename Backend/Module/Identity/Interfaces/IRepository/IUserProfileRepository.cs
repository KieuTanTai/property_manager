using Identity.Models.Profile;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Identity.Interfaces.IRepository
{
    public interface IUserProfileRepository : IBaseReadRepository<UserProfileModel, string>, IBaseUpsertRepository<UserProfileModel>
    {
        Task<UserProfileModel?> GetUserProfileAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByFirstNameAsync(Guid? cursor, string firstName, int pageSize, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByLastNameAsync(Guid? cursor, string lastName, int pageSize, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingByNameAsync(Guid? cursor, string name, int pageSize, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<UserProfileModel>> GetProfilePagingAsync(Guid? cursor, int pageSize, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByUserBirthdayAsync(Guid? cursor, DateTime birthday, int pageSize, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByUserGenderAsync(Guid? cursor, string gender, int pageSize, CancellationToken cancellationToken = default);
        Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByPhoneNumberAsync(Guid? cursor, string phoneNumber, int pageSize,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<UserProfileModel>> GetProfileByAddressAsync(Guid? cursor, string address, int pageSize, CancellationToken cancellationToken = default);
    }
}