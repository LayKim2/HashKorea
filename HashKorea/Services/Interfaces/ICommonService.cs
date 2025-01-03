﻿using HashKorea.DTOs.Common;
using HashKorea.Responses;

namespace HashKorea.Services;

public interface ICommonService
{
    Task<ServiceResponse<List<GetPostsResponseDto>>> GetPosts(string type);
    Task<ServiceResponse<GetPostDetailResponsetDto>> GetPostDetail(int userId, int postId);
}
