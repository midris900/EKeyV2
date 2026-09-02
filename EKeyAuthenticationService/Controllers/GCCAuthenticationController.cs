using EKeyAuthenticationService.Model;
using EKeyAuthenticationService.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EKeyAuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GCCAuthenticationController(IConfiguration configuration, AuthServiceGCC authService, AuthSessionService authSessionService, ILogger<GCCAuthenticationController> logger) : ControllerBase
    {

        private readonly IConfiguration _configuration = configuration;
        private readonly AuthServiceGCC _authService = authService;
        private readonly AuthSessionService _authSessionService = authSessionService;
        private readonly ILogger<GCCAuthenticationController> _logger = logger;

        // Step 1: Get the Login URL
        [HttpGet("login")]
        public async Task<IActionResult> Login([FromQuery] string redirectUrl)
        {
            if (string.IsNullOrEmpty(redirectUrl))
                return BadRequest("Redirect URL is required");

            _logger.LogInformation("Initiating login process. Redirect URL: {RedirectUrl}", redirectUrl);

            var allowedRedirects = _configuration.GetSection("AllowedRedirectUrls").Get<List<string>>();
            if (!allowedRedirects!.Contains(redirectUrl))
            {
                _logger.LogWarning("Invalid Redirect URL: {RedirectUrl}", redirectUrl);
                return BadRequest("Invalid redirect URL");
            }

            var loginUrl = await _authService.GetLoginUrl(redirectUrl);
            return Ok(new { loginUrl });
        }

        [HttpPost("loginForApex")]
        public async Task<IActionResult> loginForApex([FromBody] ApexLoginParameters model)
        {
            if (string.IsNullOrEmpty(model.RedirectUrl))
                return BadRequest("Redirect URL is required");

            _logger.LogInformation("Initiating login process. Redirect URL: {RedirectUrl}", model.RedirectUrl);

            var allowedRedirects = _configuration.GetSection("AllowedRedirectUrls").Get<List<string>>();
            if (!allowedRedirects!.Contains(model.RedirectUrl))
            {
                _logger.LogWarning("Invalid Redirect URL: {RedirectUrl}", model.RedirectUrl);
                return BadRequest("Invalid redirect URL");
            }

            var redirectUrl = $"{model.RedirectUrl}/f?p={model.AppId}:{model.PageId}:{model.AppSessionId}::::auth,sessionId";


            var loginUrl = await _authService.GetLoginUrl(redirectUrl);
            return Ok(new { loginUrl });
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string state, [FromQuery] string? code = null, [FromQuery] string? error = null, [FromQuery] string? error_description = null)
        {
            _logger.LogInformation("Received callback with state: {State}", state);

            var session = await _authSessionService.GetAuthSessionAsync(Guid.Parse(state));

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogInformation("Getting Error Access Denied By the User : {error}", error);

                if (session!.RedirectUrl.Contains("f?p="))
                {
                    var redirectUrl = $"{session.RedirectUrl}:{error},{session.SessionId}";  //+= $"&auth=success&sessionId={session.SessionId}&AccessToken={session.AccessToken}";
                    return Redirect(redirectUrl);
                }
                else
                {
                    string redirectToClient = $"{session!.RedirectUrl}?auth={error}&sessionId={session.SessionId}&error_description={error_description}";
                    return Redirect(redirectToClient);
                }
            }
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogInformation("Getting Error Access Denied By the User : {error}", error);
                if (session!.RedirectUrl.Contains("f?p="))
                {
                    var redirectUrl = $"{session.RedirectUrl}:{error},{session.SessionId}";  //+= $"&auth=success&sessionId={session.SessionId}&AccessToken={session.AccessToken}";
                    return Redirect(redirectUrl);
                }
                else
                {
                    string redirectToClient = $"{session!.RedirectUrl}?auth={error}&sessionId={session.SessionId}&error_description={error_description}";
                    return Redirect(redirectToClient);
                }
            }

            _logger.LogInformation("Token Exchange Calling");

            try
            {
                var userInfo = await _authService.ExchangeTokenAndGetUserInfo(code, session!.CodeVerifier, session.SessionId);
                if (userInfo == null)
                {
                    _logger.LogInformation("Failed to fetch user details");

                    if (session!.RedirectUrl.Contains("f?p="))
                    {
                        var redirectUrl = $"{session.RedirectUrl}:token_exchange_failed,{session.SessionId}";
                        return Redirect(redirectUrl);
                    }
                    else
                    {
                        string redirectToClient = $"{session.RedirectUrl}?auth=failed&sessionId={session.SessionId}&error_description=token_exchange_failed";
                        return Redirect(redirectToClient);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error during token exchange");
                if (session!.RedirectUrl.Contains("f?p="))
                {
                    var redirectUrl = $"{session.RedirectUrl}:token_exchange_failed,{session.SessionId}";
                    return Redirect(redirectUrl);
                }
                else
                {
                    string redirectToClient = $"{session.RedirectUrl}?auth=failed&sessionId={session.SessionId}&error_description=token_exchange_failed";
                    return Redirect(redirectToClient);
                }
            }

            if (session.RedirectUrl.Contains("f?p="))
            {
                var redirectUrl = $"{session.RedirectUrl}:success,{session.SessionId}";
                return Redirect(redirectUrl);
            }
            else
            {
                string clientRedirectUrl = $"{session.RedirectUrl}?auth=success&sessionId={session.SessionId}"; // &AccessToken={session.AccessToken}";
                return Redirect(clientRedirectUrl);
            }

            //// Redirect user back to the client application
            //string clientRedirectUrl = $"{session.RedirectUrl}?auth=success&sessionId={session.SessionId}&access_token={session.UserInfo}";
            //return Redirect(clientRedirectUrl);
        }


        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo([FromQuery] Guid sessionId)
        {
            _logger.LogInformation("Initialting Get User Info with sessionId: {State}", sessionId);
            var session = await _authSessionService.GetAuthStateAsync(sessionId);
            if (session == null || string.IsNullOrEmpty(session.UserInfo))
                return Unauthorized("Invalid session");
            var userInfo = _authService.FetchUserInfo(session.UserInfo);
            return Ok(userInfo);
        }

    }
}
