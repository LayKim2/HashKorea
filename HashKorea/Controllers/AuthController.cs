using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HashKorea.Models;
using HashKorea.Services;
using HashKorea.Common.Constants;
using System.Security.Claims;

namespace HashKorea.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("signin/{loginType}")]
    public IActionResult SignIn(string loginType)
    {

        string loginUrl;

        string formattedLoginType = loginType.Length > 1
                                    ? char.ToUpper(loginType[0]) + loginType.Substring(1).ToLower()
                                    : loginType.ToUpper();

        switch (formattedLoginType)
        {
            case USER_AUTH.KAKAO:
                loginUrl = _authService.GetKakaoLoginUrl();
                break;
            case USER_AUTH.NAVER:
                //loginUrl = _authService.GetNaverLoginUrl();
                break;
            default:
                return BadRequest("Unsupported login type.");
        }

        loginUrl = _authService.GetKakaoLoginUrl();

        return Redirect(loginUrl);
    }


    [HttpGet("signin/kakao/callback")]
    public async Task<IActionResult> SignInKakaoCallback([FromQuery] string code)
    {
        var getTokenResponse = await _authService.GetKakaoToken(code);

        if (getTokenResponse.Success && getTokenResponse.Data != null)
        {
            var getKakaoUserInfo = await _authService.GetKakaoUserInfo(getTokenResponse.Data.ToString());

            if (getKakaoUserInfo.Success && getKakaoUserInfo.Data != null && getKakaoUserInfo.Data.Id != null)
            {
                var isCompletedResponse = await _authService.IsCompleted(getKakaoUserInfo.Data, USER_AUTH.KAKAO);

                if (isCompletedResponse.Success && isCompletedResponse.Data != null)
                {
                    // 1. get user info and token
                    if (isCompletedResponse.Data.isCompleted)
                    {
                        var response = await _authService.SignInWithExternalProvider(USER_AUTH.KAKAO, getKakaoUserInfo.Data.Id.ToString());

                        if (response.Success)
                        {
                            return Content($@"
                                <script>
                                    window.opener.postMessage({{ token: '{response.Data.token}' }}, '{Request.Scheme}://{Request.Host}');
                                    window.close();
                                </script>", "text/html");
                        }

                        //if (!response.Success)
                        //{
                        //    return StatusCode(500, response);
                        //}

                        //return Ok(response);
                    }
                    // 2. response user info from Naver
                    else
                    {
                        return Json(isCompletedResponse);
                        //return Ok(isCompletedResponse);
                    }
                }

                return Json(isCompletedResponse);
                //return StatusCode(500, isCompletedResponse);
            }
        }

        return Json(getTokenResponse);
        //return Ok(new { string.Empty });
    }

    [HttpGet("check-login-status")]
    public IActionResult CheckLoginStatus()
    {
        if (User.Identity.IsAuthenticated)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null)
            {
                var userId = int.Parse(userIdClaim.Value);
                var userData = new
                {
                    Id = userId,
                };
                return Json(new { isLoggedIn = true, userData });
            }
        }
        return Json(new { isLoggedIn = false });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}