using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HashKorea.Models;
using HashKorea.Services;
using HashKorea.Common.Constants;

namespace HashKorea.Controllers;

[Route("api/infocenter")]
[ApiController]
public class InfoCenterController : Controller
{
    private readonly ILogger<InfoCenterController> _logger;
    private readonly ICommonService _commonService;

    public InfoCenterController(ILogger<InfoCenterController> logger, ICommonService commonService)
    {
        _logger = logger;
        _commonService = commonService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("tourmap")]
    public IActionResult TourMap()
    {
        return View();
    }

    [HttpGet("koreais")]
    public IActionResult KoreaIs()
    {
        return View();
    }

    [HttpGet("promotion")]
    public async Task<IActionResult> Promotions()
    {
        var response = await _commonService.GetPosts(POST_TYPE.PROMOTION);

        if (!response.Success)
        {
            return StatusCode(500, response);
        }

        return View(response.Data);
    }

    [HttpGet("promotion/post/detail")]
    public async Task<IActionResult> GetPostDetail(int id)
    {
        var response = await _commonService.GetPostDetail(id);

        if (!response.Success)
        {
            return NotFound();
        }

        return View("PostDetail", response.Data);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}