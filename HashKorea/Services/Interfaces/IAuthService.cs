using HashKorea.DTOs.Auth;
using HashKorea.Responses;

namespace HashKorea.Services;

public interface IAuthService
{
    string GetKakaoLoginUrl();
    Task<ServiceResponse<string>> GetKakaoToken(string code);
    Task<ServiceResponse<KakaoUserInfoResponseDto>> GetKakaoUserInfo(string accessToken);
    Task<ServiceResponse<IsCompletedResponseDto>> IsCompleted(KakaoUserInfoResponseDto userInfo, string signinType); // only 간편 로그인
    Task<ServiceResponse<SignInResponseDto>> SignInWithExternalProvider(string loginType, string id);
}
