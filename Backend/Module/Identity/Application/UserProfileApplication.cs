using System.Text.RegularExpressions;
using Identity.Interfaces.IApplication;
using Identity.Interfaces.IRepository;
using Identity.Models.Profile;
using Shared.Enum;
using Shared.Interfaces;
using Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Identity.Application
{
    public partial class UserProfileApplication(
        IUnitOfWork unitOfWork,
        IUserProfileRepository userProfileRepository,
        ILogger<UserProfileApplication> logger,
        ILogPool logPool) : IUserProfileApplication
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private readonly IUserProfileRepository _userProfileRepository = userProfileRepository;
        private readonly ILogger<UserProfileApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        private const string Module = "identity";
        private const string Layer = "application";


        #region GET

        public async Task<UserProfileModel> GetProfileInfoAsync<T>(T id, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Profile lookup operation started.");
            var result = id switch
            {
                string identityCode => await _userProfileRepository.GetByIdAsync(identityCode, cancellationToken),
                Guid accountId => await _userProfileRepository.GetUserProfileAsync(accountId, cancellationToken),
                _ => throw InvalidIdType()
            };
            if (result is null)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Profile lookup returned no profile.");
                throw new InvalidOperationException("Profile was not found.");
            }
            return result;
        }

        #endregion

        #region POST

        public async Task<UserProfileModel> UpdateProfileInfoAsync(UserProfileModel userProfile, CancellationToken cancellationToken = default)
        {
            _logger.LogLayerInformation(_logPool, Module, Layer, "Profile update operation started.");
            if (string.IsNullOrWhiteSpace(userProfile.UserProfileId) || userProfile.UserProfileId.Length != 12)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Profile update rejected because the identity code is invalid.");
                throw new ArgumentException("identity code is required and must be 12 digits");
            }
            if (!string.IsNullOrWhiteSpace(userProfile.UserProfilePhoneNumber))
            {
                var resultFormated = TryProcessPhoneNumber(userProfile.UserProfilePhoneNumber, out var phoneNumber);
                if (resultFormated)
                {
                    userProfile.SetUserProfilePhoneNumber(phoneNumber);
                }
                else
                {
                    _logger.LogLayerWarning(_logPool, Module, Layer, "Profile update rejected because the phone number is invalid.");
                    throw new ArgumentException("Invalid phone number format.");
                }
            }

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
            catch (ArgumentException argumentException)
            {
                _logger.LogLayerWarning(_logPool, Module, Layer, "Profile update operation rejected by validation.");
                throw new ArgumentException("Profile update request is invalid.", argumentException);
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

        #endregion

        private ArgumentException InvalidIdType()
        {
            _logger.LogLayerWarning(_logPool, Module, Layer, "Profile lookup rejected because the identifier type is invalid.");
            return new ArgumentException("Invalid id type");
        }

        #region PRIVATE

        private static bool TryProcessPhoneNumber(string input, out string stringPhoneNumber)
        {
            stringPhoneNumber = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            // 1. Remove all non-digit characters except a leading '+' sign (if present)
            var cleaned = MyRegex().Replace(input, "");

            // 2. Normalize an international format (+84 or 84) to local '0'
            if (cleaned.StartsWith("+84"))
            {
                cleaned = string.Concat("0", cleaned.AsSpan(3));
            }
            else if (cleaned.StartsWith("84") && cleaned.Length == 11)
            {
                cleaned = string.Concat("0", cleaned.AsSpan(2));
            }

            // 3. Regex validation for Vietnamese mobile prefixes (10 digits total)
            // Matches prefixes: 03, 05, 07, 08, 09 followed by exactly 8 digits
            const string pattern = @"^(03|05|07|08|09)\d{8}$";

            if (!Regex.IsMatch(cleaned, pattern))
            {
                return false;
            }
            stringPhoneNumber = cleaned;
            return true;
        }

        [GeneratedRegex(@"[^\d+]")]
        private static partial Regex MyRegex();

        #endregion
    }
}