using HashKorea.Common.Constants;
using HashKorea.Data;
using HashKorea.DTOs.Auth;
using HashKorea.Models;
using HashKorea.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HashKorea.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _jwtSecretKey;

    private readonly string _kakaoClientId;
    private readonly string _kakaoClientSecret;
    private readonly string _kakaoRedirectUri;
    private readonly string _kakaoAuthUrl;

    public AuthService(DataContext context, ILogService logService, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logService = logService;

        _httpClientFactory = httpClientFactory; // 외부 api 호출을 위해

        _jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? string.Empty;

        _kakaoClientId = Environment.GetEnvironmentVariable("KAKAO_CLIENT_ID") ?? string.Empty;
        _kakaoClientSecret = Environment.GetEnvironmentVariable("KAKAO_CLIENT_SECRET") ?? string.Empty;
        _kakaoRedirectUri = Environment.GetEnvironmentVariable("KAKAO_REDIRECT_URI") ?? string.Empty;
        _kakaoAuthUrl = Environment.GetEnvironmentVariable("KAKAO_AUTH_URL") ?? string.Empty;
        _kakaoClientSecret = Environment.GetEnvironmentVariable("KAKAO_CLIENT_SECRET") ?? string.Empty;
    }

    public string GetKakaoLoginUrl()
    {
        return $"{_kakaoAuthUrl}?client_id={_kakaoClientId}&redirect_uri={Uri.EscapeDataString(_kakaoRedirectUri)}&response_type=code";
    }

    public async Task<ServiceResponse<string>> GetKakaoToken(string code)
    {
        var result = new ServiceResponse<string>();

        try
        {
            using var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync("https://kauth.kakao.com/oauth/token",
                                new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                        {"grant_type", "authorization_code"},
                                        {"client_id", _kakaoClientId},
                                        {"client_secret", _kakaoClientSecret}, // client_secret 추가
                                        {"redirect_uri", _kakaoRedirectUri},
                                        {"code", code}
                                }));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                result.Success = false;
                result.Data = null;
                result.Code = response.StatusCode.ToString();
                result.Message = errorContent;

                _logService.LogError("EXCEPTION: GetKakaoToken", errorContent, "ip: ");

            }
            else
            {
                var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                result.Data = content.GetProperty("access_token").GetString();
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Data = null;
            result.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            result.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetKakaoToken", ex.Message, "ip: ");

            return result;
        }

    }

    public async Task<ServiceResponse<KakaoUserInfoResponseDto>> GetKakaoUserInfo(string accessToken)
    {
        var response = new ServiceResponse<KakaoUserInfoResponseDto>();

        try
        {
            using var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var responseFromKakao = await client.GetFromJsonAsync<JsonElement>("https://kapi.kakao.com/v2/user/me");

            if (responseFromKakao.TryGetProperty("id", out JsonElement idElement) &&
                responseFromKakao.TryGetProperty("connected_at", out JsonElement connectedAtElement))
            {

                var nickname = string.Empty;
                var email = string.Empty;

                // kakao_account에서 nickname과 email 추출
                if (responseFromKakao.TryGetProperty("kakao_account", out JsonElement kakaoAccount))
                {
                    if (kakaoAccount.TryGetProperty("profile", out JsonElement profile) &&
                        profile.TryGetProperty("nickname", out JsonElement nicknameElement))
                    {
                        nickname = nicknameElement.GetString();
                    }

                    if (kakaoAccount.TryGetProperty("email", out JsonElement emailElement))
                    {
                        email = emailElement.GetString();
                    }
                }

                response.Data = new KakaoUserInfoResponseDto()
                {
                    Id = idElement.GetInt64(),
                    ConnectedAt = connectedAtElement.GetString(),
                    name = nickname,
                    email = email
                };


                return response;
            }
            else
            {
                response.Success = false;
                return response;
            }

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetKakaoUserInfo", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<IsCompletedResponseDto>> IsCompleted(KakaoUserInfoResponseDto userInfo, string signinType)
    {
        var response = new ServiceResponse<IsCompletedResponseDto>();

        try
        {
            var userAuth = await _context.UserAuth.FirstOrDefaultAsync(ua => ua.AuthKey == signinType && ua.AuthValue == userInfo.Id.ToString());

            if (userAuth == null)
            {
                var newUser = new User();

                newUser.Name = userInfo.name;
                newUser.Email = userInfo.email;

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                userAuth = new UserAuth
                {
                    UserId = newUser.Id,
                    AuthKey = signinType,
                    AuthValue = userInfo.Id.ToString(),
                    IsCompleted = true
                    //IsCompleted = false
                };

                _context.UserAuth.Add(userAuth);
                await _context.SaveChangesAsync();

                response.Data = new IsCompletedResponseDto()
                {
                    id = userInfo.Id.ToString(),
                    loginType = signinType,
                    isCompleted = true
                    //isCompleted = false
                };
            }
            else
            {
                response.Data = new IsCompletedResponseDto()
                {
                    id = userInfo.Id.ToString(),
                    loginType = signinType,
                    isCompleted = userAuth.IsCompleted
                };
            }

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = new IsCompletedResponseDto();
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: IsCompleted", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<SignInResponseDto>> SignInWithExternalProvider(string loginType, string id)
    {
        var response = new ServiceResponse<SignInResponseDto>();

        try
        {
            var token = string.Empty;

            var userAuth = await _context.UserAuth.FirstOrDefaultAsync(x => x.AuthKey == loginType && x.AuthValue == id);

            // 0. check user auth
            if (userAuth == null)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_REGISTERED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_REGISTERED_USER];

                return response;
            }

            var user = await _context.Users
                            .Include(u => u.UserAuth)
                            .Include(u => u.UserRoles)
                            .FirstOrDefaultAsync(u => u.Id == userAuth.UserId);

            // 1. check email
            if (user == null)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_REGISTERED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_REGISTERED_USER];

                return response;
            }
            else if (user.IsDeleted)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.DELETED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.DELETED_USER];

                return response;
            }

            response = await GetSignInData(user);

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = null;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SignInWithExternalProvider", ex.Message, "ip: ");

            return response;
        }
    }

    private async Task<ServiceResponse<SignInResponseDto>> GetSignInData(User user)
    {
        var response = new ServiceResponse<SignInResponseDto>();

        var token = string.Empty;

        try
        {
            // 3. get role
            var userRole = user.UserRoles.FirstOrDefault()?.Role ?? string.Empty;

            // 4.Prepare response data
            response.Data = new SignInResponseDto()
            {
                id = user.Id,
                //email = user.Email,
                name = user.Name,
                imageUrl = user.PublicUrl,
                role = userRole
            };

            // 5. Set specific ID based on role
            token = GenerateJwtToken(user, userRole);

            response.Data.token = token;

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = null;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SetSignInData", ex.Message, "ip: ");

            return response;
        }

    }

    public string GenerateJwtToken(User user, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, role),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}