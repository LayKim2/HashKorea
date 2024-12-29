using HashKorea.Data;
using HashKorea.DTOs.Common;
using HashKorea.Responses;
using Microsoft.EntityFrameworkCore;

namespace HashKorea.Services;

public class SharedService : ISharedService
{
    private readonly DataContext _context; 
    private readonly ILogService _logService;

    public SharedService(DataContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<ServiceResponse<List<GetCommonCodeResponseDto>>> GetCommonCodes(string type)
    {
        var response = new ServiceResponse<List<GetCommonCodeResponseDto>>();

        try
        {
            var commonCodes = await _context.CommonCodes
                .Where(cc => cc.Type == type)
                .Select(cc => new GetCommonCodeResponseDto
                {
                    Id = cc.Id,
                    Type = cc.Type,
                    Code = cc.Code,
                    Name = cc.Name,
                })
                .ToListAsync();

            if (commonCodes.Count == 0)
            {
                commonCodes.Add(new GetCommonCodeResponseDto()
                {
                    Id = 0,
                    Code = "00",
                    Name = "Default",
                    Type = type
                });
            }

            response.Data = commonCodes;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            _logService.LogError("GetCommonCodes", ex.Message, ex.StackTrace ?? string.Empty);
        }

        return response;
    }

}