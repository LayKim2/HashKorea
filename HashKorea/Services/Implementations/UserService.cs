using HashKorea.Common.Constants;
using HashKorea.Data;
using HashKorea.DTOs.User;
using HashKorea.Models;
using HashKorea.Responses;
using System.Text.RegularExpressions;

namespace HashKorea.Services;

public class UserService : IUserService
{
    private readonly DataContext _context; 
    private readonly ILogService _logService;
    private readonly IFileService _fileService;

    public UserService(DataContext context, ILogService logService, IFileService fileService)
    {
        _context = context;
        _logService = logService;
        _fileService = fileService;
    }

    public async Task<ServiceResponse<int>> AddPost(int userId, PostRequestDto model)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var convertedContentResponse = await ConvertContent(model.Content, userId, model.ImageFiles);

            if (!convertedContentResponse.Success)
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Code = convertedContentResponse.Code,
                    Message = convertedContentResponse.Message
                };
            }

            var (content, userPostImages) = convertedContentResponse.Data;

            var userPost = new UserPost
            {
                UserId = userId,
                Title = model.Title,
                Type = POST_TYPE.PROMOTION,
                Category = model.Category,
                Content = content,
                CreatedDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now
            };

            _context.UserPosts.Add(userPost);
            await _context.SaveChangesAsync();

            foreach (var image in userPostImages)
            {
                image.PostId = userPost.Id;
                _context.UserPostImages.Add(image);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            response.Success = true;
            response.Data = userPost.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];
            _logService.LogError("EXCEPTION: AddPost", ex.Message, $"user id: {userId}");
        }

        return response;
    }


    private async Task<ServiceResponse<(string Content, List<UserPostImage> Images)>> ConvertContent(string content, int userId, List<IFormFile> imageFiles)
    {
        var response = new ServiceResponse<(string Content, List<UserPostImage> Images)>();
        var userPostImages = new List<UserPostImage>();

        try
        {
            var regex = new Regex(@"<img[^>]*src=""{{image_(\d+)}}""[^>]*>");
            var matches = regex.Matches(content);

            var folderPath = $"content/{userId}";

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var imageIndex = int.Parse(match.Groups[1].Value);

                if (imageIndex >= imageFiles.Count)
                {
                    continue;
                }

                var file = imageFiles[imageIndex];
                var uploadResult = await _fileService.UploadFile(file, folderPath);

                if (uploadResult.Success)
                {
                    content = content.Replace(match.Value, $"<img src=\"{uploadResult.Data.CloudFrontUrl}\" />");

                    userPostImages.Add(new UserPostImage
                    {
                        UserId = userId,
                        StoragePath = uploadResult.Data.S3Path,
                        PublicUrl = uploadResult.Data.CloudFrontUrl,
                        FileName = file.FileName,
                        FileType = file.ContentType,
                        FileSize = file.Length
                    });
                }
                else
                {
                    response.Success = false;
                    response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
                    response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];
                    _logService.LogError("ConvertContent", uploadResult.Message, $"User ID: {userId}, File Name: {file.FileName}");
                    return response;
                }
            }

            response.Success = true;
            response.Data = (content, userPostImages);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];
            _logService.LogError("EXCEPTION: ConvertContent", ex.Message, $"user id: {userId}");
        }

        return response;
    }

}