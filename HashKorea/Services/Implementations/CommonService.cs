using HashKorea.Data;
using HashKorea.DTOs.Common;
using HashKorea.Responses;
using Microsoft.EntityFrameworkCore;

namespace HashKorea.Services;

public class CommonService : ICommonService
{
    private readonly DataContext _context; 
    private readonly ILogService _logService;

    public CommonService(DataContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<ServiceResponse<List<GetPostsResponseDto>>> GetPosts(string type)
    {
        var response = new ServiceResponse<List<GetPostsResponseDto>>();

        try
        {
            var posts = await _context.UserPosts
                .Where(p => p.Type == type)
                .Select(p => new GetPostsResponseDto
                {
                    Id = p.Id,
                    Type = p.Type,
                    Title = p.Title,
                    Category = p.Category,
                    Content = p.Content,
                    UserName = p.User.Name,
                    CreatedDate = p.CreatedDate,
                })
                .ToListAsync();

            response.Data = posts;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = $"Failed to retrieve posts of type: {type}";
            _logService.LogError("GetPosts", ex.Message, ex.StackTrace ?? string.Empty);
        }

        return response;
    }

    public async Task<ServiceResponse<GetPostDetailResponsetDto>> GetPostDetail(int userId, int postId)
    {
        var response = new ServiceResponse<GetPostDetailResponsetDto>();

        try
        {
            var post = await _context.UserPosts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                response.Success = false;
                response.Message = "Post not found";
                return response;
            }

            var postDto = new GetPostDetailResponsetDto
            {
                Id = post.Id,
                Category = post.Category,
                Title = post.Title,
                Content = post.Content,
                CreatedDate = post.CreatedDate,
                UserName = post.User.Name,
                UserCountry = post.User.Country,
                IsOwner = (userId != 0 && userId == post.UserId)
            };

            response.Data = postDto;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = $"Failed to retrieve post with ID: {postId}";
            _logService.LogError("GetPostDetail", ex.Message, ex.StackTrace ?? string.Empty);
        }

        return response;
    }


}