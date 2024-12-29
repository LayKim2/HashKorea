using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HashKorea.Models;
using HashKorea.Services;

namespace HashKorea.Controllers;

[Route("api/shared")]
[ApiController]
public class SharedController : Controller
{
    private readonly ILogger<SharedController> _logger;
    private readonly ISharedService _sharedService;

    public SharedController(ILogger<SharedController> logger, ISharedService sharedService)
    {
        _logger = logger;
        _sharedService = sharedService;
    }


    [HttpGet("commoncodes")]
    public async Task<IActionResult> GetCommonCodes(string type)
    {
        var response = await _sharedService.GetCommonCodes(type);

        if (!response.Success)
        {
            return StatusCode(500, response);
        }

        return Ok(response);
    }

}
