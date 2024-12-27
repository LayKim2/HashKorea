using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HashKorea.Models;
using HashKorea.Services;
using HashKorea.Common.Constants;

namespace HashKorea.Controllers;

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

    public IActionResult TourMap()
    {
        return View();
    }

    public IActionResult KoreaIs()
    {
        return View();
    }

    public async Task<IActionResult> Promotions()
    {
        var response = await _commonService.GetPosts(POST_TYPE.PROMOTION);

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