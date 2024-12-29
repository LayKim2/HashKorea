using HashKorea.DTOs.Common;
using HashKorea.DTOs.User;
using HashKorea.Responses;

namespace HashKorea.Services;

public interface ISharedService
{
    Task<ServiceResponse<List<GetCommonCodeResponseDto>>> GetCommonCodes(string type);
}
