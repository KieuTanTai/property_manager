using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Application.Record.Cache;
using Identity.Models.Profile;
using Microsoft.Extensions.Caching.Memory;
using Shared.Interfaces;
using Shared.Logging;

namespace Identity.Application
{
    public class UserProfileApplication(
        IUnitOfWork unitOfWork,
        IUserProfileRepository userProfileRepository,
        IMemoryCache memoryCache,
        ILogger<UserProfileApplication> logger,
        ILogPool logPool) : IUserProfileApplication
    {
        private const string Module = "Identity";

        private const string Layer = "Application";

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<UserProfileApplication> _logger = logger;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private readonly IUserProfileRepository _userProfileRepository = userProfileRepository;

        private readonly IMemoryCache _memoryCache = memoryCache;

        private static string ProfileByAccountKey(Guid accountId) => $"identity:profile:account:{accountId}";

        private static string ProfileByCodeKey(string identityCode) => $"identity:profile:code:{identityCode}";

        private static readonly MemoryCacheEntryOptions CacheOptions = new() { SlidingExpiration = TimeSpan.FromMinutes(20) };

        #region PRIVATE

        private UserProfileModel EnsureProfileFound(UserProfileModel? result)
        {
            if (result is not null)
            {
                return result;
            }
            _logger.LogLayerWarning(_logPool, Module, Layer, "Profile lookup returned no profile.");
            throw new InvalidOperationException("Profile was not found.");
        }

        #endregion


        #region GET

        public async Task<UserProfileModel> GetProfileInfoAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Profile lookup operation started.");
            var cachedProfile = await _memoryCache.GetOrCreateAsync(ProfileByAccountKey(accountId), async entry => {
                entry.SetOptions(CacheOptions);
                var result = await _userProfileRepository.GetUserProfileAsync(accountId, cancellationToken);
                var profile = EnsureProfileFound(result);
                return new RecordIdentityCacheEntry<UserProfileModel>(profile.UserProfileAccountId, profile);
            });
            return cachedProfile?.Fields ?? throw new InvalidOperationException("Profile was not found.");
        }

        public async Task<UserProfileModel> GetProfileInfoAsync(string identityCode, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Profile lookup operation started.");
            return await _memoryCache.GetOrCreateAsync(ProfileByCodeKey(identityCode), async entry => {
                entry.SetOptions(CacheOptions);
                var result = await _userProfileRepository.GetByIdAsync(identityCode, cancellationToken);
                return EnsureProfileFound(result);
            }) ?? throw new InvalidOperationException("Profile was not found.");
        }

        #endregion

        #region UPSERT

        public async Task<UserProfileModel> UpdateProfileInfoAsync(UserProfileModel userProfile, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update operation started.");
            try
            {
                _memoryCache.TryGetValue(ProfileByAccountKey(userProfile.UserProfileAccountId), out RecordIdentityCacheEntry<UserProfileModel>? previousEntry);
                await _userProfileRepository.UpdateAsync(userProfile, cancellationToken);
                var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
                if (affectRows == 0)
                {
                    _logger.LogLayerError(_logPool, Module, Layer, new InvalidOperationException("Profile update saved no rows."), "Profile update persistence failed.");
                    throw new InvalidOperationException("Failed to update user profile.");
                }
                _memoryCache.Remove(ProfileByAccountKey(userProfile.UserProfileAccountId));
                if (previousEntry is not null)
                    _memoryCache.Remove(ProfileByCodeKey(previousEntry.Fields.UserProfileId));
                _memoryCache.Remove(ProfileByCodeKey(userProfile.UserProfileId));
                InvalidateAccountProfile(userProfile.UserProfileAccountId);
                CacheProfile(userProfile);
                _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update completed.");
                return userProfile;
            }
            catch (OperationCanceledException canceledException)
            {
                _logger.LogLayerDebug(_logPool, Module, Layer, "Profile update operation was canceled.");
                throw new OperationCanceledException("Profile update was canceled.", canceledException);
            }
            catch (Exception ex)
            {
                _logger.LogLayerError(_logPool, Module, Layer, ex, "Profile update operation failed.");
                throw new InvalidOperationException("Failed to update user profile.", ex);
            }
        }

        public async Task<UserProfileModel> CreateBaseProfileInfoAsync(string identityCode, Guid accountId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile creation operation started.");
            // method will raise an argument exception or argument null exception if failed to create a user profile. (check on constructor)
            var baseProfile = new UserProfileModel(identityCode, accountId);
            await _userProfileRepository.AddAsync(baseProfile, cancellationToken);

            var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectRows == 0)
            {
                _logger.LogLayerError(_logPool, Module, Layer, new InvalidOperationException("Profile creation saved no rows."), "Profile creation persistence failed.");
                throw new InvalidOperationException("Failed to create user profile.");
            }
            CacheProfile(baseProfile);
            InvalidateAccountProfile(baseProfile.UserProfileAccountId);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile creation completed.");
            return baseProfile;
        }

        public async Task<int> UpdateRangeAsync(IEnumerable<UserProfileModel> userProfiles, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update range operation started.");
            var userProfileModels = userProfiles.ToList();
            var previousCodes = userProfileModels.Select(profile => {
                _memoryCache.TryGetValue(ProfileByAccountKey(profile.UserProfileAccountId), out RecordIdentityCacheEntry<UserProfileModel>? previous);
                return previous?.Fields.UserProfileId;
            }).Where(code => code is not null).ToList();
            _userProfileRepository.UpdateRange(userProfileModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update range operation completed.");
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows > 0) foreach (var profile in userProfileModels)
            {
                _memoryCache.Remove(ProfileByAccountKey(profile.UserProfileAccountId));
                _memoryCache.Remove(ProfileByCodeKey(profile.UserProfileId));
                InvalidateAccountProfile(profile.UserProfileAccountId);
                CacheProfile(profile);
            }
            foreach (var previousCode in previousCodes)
                _memoryCache.Remove(ProfileByCodeKey(previousCode!));
            return affectedRows;
        }

        private void CacheProfile(UserProfileModel profile)
        {
            _memoryCache.Set(ProfileByAccountKey(profile.UserProfileAccountId), new RecordIdentityCacheEntry<UserProfileModel>(profile.UserProfileAccountId, profile), CacheOptions);
            _memoryCache.Set(ProfileByCodeKey(profile.UserProfileId), profile, CacheOptions);
        }

        private void InvalidateAccountProfile(Guid accountId)
        {
            var accountKey = $"identity:account:id:{accountId}";
            _memoryCache.TryGetValue(accountKey, out RecordIdentityCacheEntry<Identity.Models.Account.AccountModel>? cachedAccountEntry);
            _memoryCache.Remove(accountKey);
            if (cachedAccountEntry is null) return;
            var cachedAccount = cachedAccountEntry.Fields;
            var normalizedEmail = cachedAccount.AccountEmail.Trim().ToUpperInvariant();
            foreach (var roles in new[] {false, true})
            foreach (var permissions in new[] {false, true})
            foreach (var includeProfile in new[] {false, true})
                _memoryCache.Remove($"identity:account:email:{normalizedEmail}:roles:{roles}:permissions:{permissions}:profile:{includeProfile}");
        }

        #endregion
    }
}
