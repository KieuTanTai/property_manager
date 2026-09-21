using Identity.Models.Profile;

namespace Identity.Interfaces.IApplication
{
    public interface IUserProfileApplication
    {
        Task<UserProfileModel> GetProfileInfoAsync<T>(T id, CancellationToken cancellationToken = default);

        Task<UserProfileModel> UpdateProfileInfoAsync(UserProfileModel userProfile, CancellationToken cancellationToken = default);
        Task<UserProfileModel> CreateBaseProfileInfoAsync(string identityCode, Guid accountId, CancellationToken cancellationToken = default);
    }
}