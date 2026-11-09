using Identity.Models.Profile;

namespace Identity.Interfaces.IApplication
{
    public interface IUserProfileApplication
    {
        Task<UserProfileModel> GetProfileInfoAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<UserProfileModel> GetProfileInfoAsync(string identityCode, CancellationToken cancellationToken = default);

        Task<UserProfileModel> UpdateProfileInfoAsync(UserProfileModel userProfile, CancellationToken cancellationToken = default);
        Task<UserProfileModel> CreateBaseProfileInfoAsync(string identityCode, Guid accountId, CancellationToken cancellationToken = default);
        Task<int> UpdateRangeAsync(IEnumerable<UserProfileModel> userProfiles, CancellationToken cancellationToken = default);
    }
}
