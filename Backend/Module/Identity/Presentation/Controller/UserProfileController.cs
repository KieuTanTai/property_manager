using Identity.Interfaces;
using Identity.Interfaces.IApplication;
using Identity.Models.Profile;
using Identity.Presentation.Record.Profile;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Presentation.Controller
{
    [ApiController]
    [Route("api/profile/profile")]
    public class ProfileController(
        IUserProfileApplication userProfileApplication,
        IApiHelper apiHelper) : ControllerBase
    {
        private readonly IApiHelper _apiHelper = apiHelper;

        private readonly IUserProfileApplication _userProfileApplication = userProfileApplication;

        #region GET

        [RequireHttps]
        [HttpGet]
        public async Task<ActionResult<RecordProfileResponse>> GetProfileAsync([FromQuery] RecordGetProfileRequest requestDto, CancellationToken cancellationToken)
        {
            if (requestDto.IdentityCode == null && requestDto.AccountId == null)
            {
                return BadRequest("must have at least one id");
            }
            try
            {
                UserProfileModel? result;
                if (requestDto.AccountId != null)
                {
                    result = await _userProfileApplication.GetProfileInfoAsync(requestDto.AccountId, cancellationToken);
                }
                else
                {
                    result = await _userProfileApplication.GetProfileInfoAsync(requestDto.IdentityCode, cancellationToken);
                }
                var response = _apiHelper.MappingProfileResult(result);
                return Ok(response);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"Operation canceled \n {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        #endregion

        #region POST

        [RequireHttps]
        [HttpPost("create")]
        public async Task<ActionResult<RecordProfileResponse>> CreateProfileAsync([FromBody] RecordCreateBaseProfileRequest requestDto,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(requestDto.IdentityCode) || requestDto.IdentityCode.Length != 12)
            {
                return BadRequest("identity code is required and must be 12 digits");
            }

            try
            {
                var result = await _userProfileApplication.CreateBaseProfileInfoAsync(requestDto.IdentityCode, requestDto.AccountId, cancellationToken);
                var response = _apiHelper.MappingProfileResult(result);
                return Ok(response);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        [RequireHttps]
        [HttpPost("update")]
        public async Task<ActionResult<RecordProfileResponse>> UpdateUserProfileAsync([FromBody] RecordUpdateProfileRequest requestDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var newProfileModel = new UserProfileModel(requestDto.IdentityCode, requestDto.AccountId, requestDto.FirstName, requestDto.LastName,
                    requestDto.DateOfBirth, requestDto.UserGender, requestDto.PhoneNumber, requestDto.Address, requestDto.AvatarUrl);
                Console.WriteLine(newProfileModel.UserProfileGender);
                var result = await _userProfileApplication.UpdateProfileInfoAsync(newProfileModel, cancellationToken);
                var response = _apiHelper.MappingProfileResult(result);
                return Ok(response);
            }
            catch (OperationCanceledException ex)
            {
                return BadRequest($"request canceled! \n {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred \n {ex.Message}");
            }
        }

        #endregion
    }
}