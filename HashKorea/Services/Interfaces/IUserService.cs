using HashKorea.DTOs.User;
using HashKorea.Responses;

namespace HashKorea.Services;

public interface IUserService
{
    Task<ServiceResponse<int>> AddPost(int userId, PostRequestDto model);
    Task<ServiceResponse<int>> UpdatePost(int userId, PostRequestDto model);
    Task<ServiceResponse<bool>> DeletePost(int userId, int postId);
}
