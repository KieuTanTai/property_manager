using System.Security.Claims;
using System.Text.Json;
using Identity.Presentation.Record.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Persistence.Record.Auth;
using MvcJsonOptions=Microsoft.AspNetCore.Mvc.JsonOptions;

namespace Frontend.Controllers
{
    public class AuthController(IHttpClientFactory clientFactory, IOptions<MvcJsonOptions> jsonOptions) : Controller
    {
        private readonly IHttpClientFactory _clientFactory = clientFactory;

        private readonly JsonSerializerOptions _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;

        #region POST

        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] RecordAuthRequest requestDto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                TempData["AuthError"] = "Please enter a valid email and password.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var client = _clientFactory.CreateClient("BackendApiIdentityHttps");
                var response = await client.PostAsJsonAsync("api/account/login", requestDto, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    TempData["AuthError"] = error.Trim('"');
                    return RedirectToAction("Index", "Home");
                }

                var responseData = await response.Content.ReadFromJsonAsync<RecordLoginResponse>(
                    _jsonSerializerOptions, cancellationToken);

                if (responseData is null)
                {
                    TempData["AuthError"] = "Login response was empty.";
                    return RedirectToAction("Index", "Home");
                }

                await AddClaimsIdentityToContext(responseData);

                return RedirectToAction("Index", "Home");
            }
            catch (OperationCanceledException)
            {
                TempData["AuthError"] = "Login request was canceled.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["AuthError"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region PRIVATE

        private static ClaimsIdentity CreateClaimsIdentity(RecordLoginResponse responseData)
        {
            var claims = responseData.Claims.Select(claim => new Claim(claim.Type, claim.Value));

            return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private async Task AddClaimsIdentityToContext(RecordLoginResponse responseData)
        {
            var identity = CreateClaimsIdentity(responseData);
            // var identity = new ClaimsIdentity(responseData.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        #endregion
    }
}