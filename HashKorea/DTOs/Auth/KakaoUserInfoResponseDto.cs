namespace HashKorea.DTOs.Auth;

public class KakaoUserInfoResponseDto
{
    public long Id { get; set; }
    public string ConnectedAt { get; set; }
    public string name { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
}