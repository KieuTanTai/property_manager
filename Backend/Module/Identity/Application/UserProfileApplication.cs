using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Models.Profile;
using Shared.Interfaces;
using Shared.Logging;

namespace Identity.Application
{
    public class UserProfileApplication(
        IUnitOfWork unitOfWork,
        IUserProfileRepository userProfileRepository,
        ILogger<UserProfileApplication> logger,
        ILogPool logPool) : IUserProfileApplication
    {
        private const string Module = "Identity";

        private const string Layer = "Application";

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<UserProfileApplication> _logger = logger;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private readonly IUserProfileRepository _userProfileRepository = userProfileRepository;


        #region GET

        public async Task<UserProfileModel> GetProfileInfoAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Profile lookup operation started.");
            var result = await _userProfileRepository.GetUserProfileAsync(accountId, cancellationToken);
            return EnsureProfileFound(result);
        }

        public async Task<UserProfileModel> GetProfileInfoAsync(string identityCode, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Profile lookup operation started.");
            var result = await _userProfileRepository.GetByIdAsync(identityCode, cancellationToken);
            return EnsureProfileFound(result);
        }

        #endregion

        #region UPSERT

        public async Task<UserProfileModel> UpdateProfileInfoAsync(UserProfileModel userProfile, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update operation started.");
            try
            {
                await _userProfileRepository.UpdateAsync(userProfile, cancellationToken);
                var affectRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
                if (affectRows == 0)
                {
                    _logger.LogLayerError(_logPool, Module, Layer, new InvalidOperationException("Profile update saved no rows."), "Profile update persistence failed.");
                    throw new InvalidOperationException("Failed to update user profile.");
                }
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
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile creation completed.");
            return baseProfile;
        }

        public async Task<int> UpdateRangeAsync(IEnumerable<UserProfileModel> userProfiles, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update range operation started.");
            var userProfileModels = userProfiles.ToList();
            _userProfileRepository.UpdateRange(userProfileModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update range operation completed.");
            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        #endregion

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
    }
}
