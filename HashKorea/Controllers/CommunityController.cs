using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HashKorea.Models;

namespace HashKorea.Controllers;

public class CommunityController : Controller
{
    private readonly ILogger<CommunityController> _logger;

    public CommunityController(ILogger<CommunityController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Notice()
    {
        return View();
    }

    public IActionResult FreeBoard()
    {
        return View();
    }

    public IActionResult QnA()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
