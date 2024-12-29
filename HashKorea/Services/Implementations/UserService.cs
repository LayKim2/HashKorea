using HashKorea.Common.Constants;
using HashKorea.Data;
using HashKorea.DTOs.User;
using HashKorea.Models;
using HashKorea.Responses;
using Microsoft.EntityFrameworkCore;
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

    private async Task<bool> IsExistUser(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user != null;
    }

    public async Task<ServiceResponse<int>> AddPost(int userId, PostRequestDto model)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            bool isExistUser = await IsExistUser(userId);

            if (isExistUser == false)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];

                return response;
            }

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
                Type = model.Type,
                Category = model.Category,
                CategoryCD = model.CategoryCD,
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

    public async Task<ServiceResponse<int>> UpdatePost(int userId, PostRequestDto model)
    {
        var response = new ServiceResponse<int>();

        var postId = model.PostId ?? 0;
        bool isExistUser = await IsExistUser(userId);

        if (isExistUser == false)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
            return response;
        }

        bool isOwner = await CheckOwner(userId, postId);

        if (isOwner == false)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNAUTHORIZED.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
            return response;
        }

        var existingPost = await _context.UserPosts
            .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId);

        if (existingPost == null)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.NOT_FOUND_POST.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_POST];
            return response;
        }

        var existingImageUrls = await _context.UserPostImages
                                        .Where(i => i.PostId == postId)
                                        .Select(i => i.StoragePath)
                                        .ToListAsync();

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

            // Update existing post
            existingPost.Title = model.Title;
            existingPost.Category = model.Category;
            existingPost.CategoryCD = model.CategoryCD;
            existingPost.Content = content;
            existingPost.LastUpdatedDate = DateTime.Now;

            // Remove existing images
            var existingImages = await _context.UserPostImages
                .Where(i => i.PostId == postId)
                .ToListAsync();
            _context.UserPostImages.RemoveRange(existingImages);

            // Add new images
            foreach (var image in userPostImages)
            {
                image.PostId = postId;
                _context.UserPostImages.Add(image);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            foreach (var imageUrl in existingImageUrls)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var deleteResult = await _fileService.DeleteFile(imageUrl);
                    if (!deleteResult.Success)
                    {
                        _logService.LogError("UpdatePost", $"Failed to delete file from S3: {imageUrl}", "");
                    }
                }
            }

            response.Success = true;
            response.Data = postId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];
            _logService.LogError("EXCEPTION: UpdatePost", ex.Message, $"user id: {userId}, post id: {postId}");
        }

        return response;
    }

    private async Task<bool> CheckOwner(int userId, int postId)
    {
        var userPost = await _context.UserPosts
            .FirstOrDefaultAsync(u => u.Id == postId && u.UserId == userId);

        return userPost != null;
    }

    public async Task<ServiceResponse<bool>> DeletePost(int userId, int postId)
    {
        var response = new ServiceResponse<bool>();

        bool isExistUser = await IsExistUser(userId);

        if (isExistUser == false)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
            return response;
        }

        bool isOwner = await CheckOwner(userId, postId);

        if (isOwner == false)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNAUTHORIZED.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
            return response;
        }

        var existingPost = await _context.UserPosts
            .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId);

        if (existingPost == null)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.NOT_FOUND_POST.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_POST];
            return response;
        }

        var existingImageUrls = await _context.UserPostImages
            .Where(i => i.PostId == postId)
            .Select(i => i.StoragePath)
            .ToListAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Remove existing images from DB
            var existingImages = await _context.UserPostImages
                .Where(i => i.PostId == postId)
                .ToListAsync();
            _context.UserPostImages.RemoveRange(existingImages);

            // Remove post
            _context.UserPosts.Remove(existingPost);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Delete S3 files after successful transaction
            foreach (var imageUrl in existingImageUrls)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var deleteResult = await _fileService.DeleteFile(imageUrl);
                    if (!deleteResult.Success)
                    {
                        _logService.LogError("DeletePost", $"Failed to delete file from S3: {imageUrl}", "");
                    }
                }
            }

            response.Success = true;
            response.Data = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];
            _logService.LogError("EXCEPTION: DeletePost", ex.Message, $"user id: {userId}, post id: {postId}");
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
                    //content = content.Replace(match.Value, $"<img src=\"{uploadResult.Data.CloudFrontUrl}\" />");
                    content = content.Replace(match.Value, $"<img src=\"{uploadResult.Data.S3Path}\" />");

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