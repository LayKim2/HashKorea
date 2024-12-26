using HashKorea.Responses;

namespace HashKorea.Services;

public interface IUserService
{
    Task<ServiceResponse<int>> AddContent(int userId, string content);
}
