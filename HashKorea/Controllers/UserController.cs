using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HashKorea.Models;
using HashKorea.Services;
using System.Security.Claims;
using HashKorea.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using HashKorea.DTOs.Common;
using HashKorea.Responses;

namespace HashKorea.Controllers;

[Authorize]
[Route("api/user")]
[ApiController]
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly ICommonService _commonService;

    public UserController(IUserService userService, ICommonService commonService)
    {
        _userService = userService;
        _commonService = commonService;
    }

    public IActionResult Index()
    {
        return View();
    }

    // postId => O : edit, X : new
    [HttpGet("post")]
    public async Task<IActionResult> Post(string type, int? postId)
    {
        ViewBag.ReturnUrl = Request.Headers["Referer"].ToString();
        ViewBag.Type = type;

        if (postId.HasValue)
        {
            // edit old one
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdClaim.Value);

            var response = await _commonService.GetPostDetail(userId, postId.Value);
            
            return View(response.Data);
        }
        else
        {
            var response = new ServiceResponse<GetPostDetailResponsetDto>();

            // add new one
            return View(response.Data);
        }
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

    [HttpPut("post")]
    public async Task<IActionResult> UpdatePost([FromForm] PostRequestDto model)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdClaim.Value);

        var response = await _userService.UpdatePost(userId, model);

        if (!response.Success)
        {
            return StatusCode(500, response);
        }

        return Ok(response);
    }

    [HttpDelete("post")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdClaim.Value);

        var response = await _userService.DeletePost(userId, postId);

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
