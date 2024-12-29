using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HashKorea.Models;
using HashKorea.Common.Constants;
using HashKorea.Services;

namespace HashKorea.Controllers;

[Route("api/community")]
[ApiController]
public class CommunityController : Controller
{
    private readonly ILogger<CommunityController> _logger;
    private readonly ICommonService _commonService;

    public CommunityController(ILogger<CommunityController> logger, ICommonService commonService)
    {
        _logger = logger;
        _commonService = commonService;
    }

    public IActionResult Index()
    {
        return View();
    }


    [HttpGet("notice")]
    public async Task<IActionResult> Notice()
    {
        var response = await _commonService.GetPosts(POST_TYPE.NOTICE);

        if (!response.Success)
        {
            return StatusCode(500, response);
        }

        return View(response.Data);
    }

    [HttpGet("freeboard")]
    public async Task<IActionResult> FreeBoard()
    {
        var response = await _commonService.GetPosts(POST_TYPE.FREEBOARD);

        if (!response.Success)
        {
            return StatusCode(500, response);
        }

        return View(response.Data);
    }

    [HttpGet("qna")]
    public async Task<IActionResult> QnA()
    {
        var response = await _commonService.GetPosts(POST_TYPE.QNA);

        if (!response.Success)
        {
            return StatusCode(500, response);
        }

        return View(response.Data);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
