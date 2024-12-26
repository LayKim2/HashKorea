using HashKorea.Common.Constants;
using HashKorea.Data;
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

    public async Task<ServiceResponse<int>> AddContent(int userId, string content)
    {
        var response = new ServiceResponse<int>();

        try
        {
            var convertedContentResponse = await UploadImage(content, userId);

            if (!convertedContentResponse.Success)
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Code = convertedContentResponse.Code,
                    Message = convertedContentResponse.Message
                };
            }

            var userPost = new UserPost
            {
                UserId = userId,
                Title = string.Empty, 
                Category = string.Empty,
                MainImagePublicUrl = string.Empty,
                Content = convertedContentResponse.Data,
                CreatedDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now
            };

            _context.UserPosts.Add(userPost);
            await _context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: AddContent", ex.Message, $"user id: {userId}");
        }

        return response;
    }


    private async Task<ServiceResponse<string>> UploadImage(string content, int userId)
    {
        var response = new ServiceResponse<string>();

        try
        {
            var regex = new Regex(@"<img[^>]*src=""data:image/(?<type>\w+);base64,(?<data>[^""]*)""\s*[^>]*>");
            var matches = regex.Matches(content);

            var folderPath = $"content/{userId}";

            foreach (Match match in matches)
            {
                var base64Data = match.Groups["data"].Value;
                var imageType = match.Groups["type"].Value;
                var imageBytes = Convert.FromBase64String(base64Data);

                var fileName = $"{Guid.NewGuid()}.{imageType}";
                var contentType = $"image/{imageType}";

                using (var ms = new MemoryStream(imageBytes))
                {
                    var formFile = new FormFile(ms, 0, imageBytes.Length, "image", fileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = contentType
                    };

                    var uploadResult = await _fileService.UploadFile(formFile, folderPath);

                    if (uploadResult.Success)
                    {
                        content = content.Replace(match.Value, $"<img src=\"{uploadResult.Data.CloudFrontUrl}\" />");
                    }
                    else
                    {
                        response.Success = false;
                        response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

                        _logService.LogError("Image Upload", uploadResult.Message, $"User ID: {userId}, Image Type: {imageType}");

                        return response;
                    }
                }
            }

            response.Success = true;
            response.Data = content;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: ProcessBase64Images", ex.Message, $"user id: {userId}");
        }

        return response;
    }



}