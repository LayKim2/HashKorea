using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HashKorea.Models;
using HashKorea.Services;
using System.Security.Claims;
using HashKorea.DTOs.User;

namespace HashKorea.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("post")]
    public IActionResult Post()
    {
        return View();
    }

    [HttpPost("post")]
    public async Task<IActionResult> AddPost([FromForm] PostRequestDto model)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdClaim.Value);

        var response = await _userService.AddPost(userId, model);

        if (!response.Success)
        {
            return StatusCode(500, response);
        }

        return Ok(response);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
