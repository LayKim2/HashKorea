using HashKorea.DTOs.User;
using HashKorea.Responses;

namespace HashKorea.Services;

public interface IUserService
{
    Task<ServiceResponse<int>> AddPost(int userId, PostRequestDto model);
}
