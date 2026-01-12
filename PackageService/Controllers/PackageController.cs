using Microsoft.AspNetCore.Mvc;
using TestSystem.Core.DTOs.PackageService;
using TestSystem.Core.Interfaces;

namespace PackageService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackageController : ControllerBase
{
    private readonly IPackageService _packageService;

    public PackageController(IPackageService packageService)
    {
        _packageService = packageService;
    }
    
    [HttpPost("task/{taskId}/send/code")]
    public async Task<IActionResult> SendCode(Guid taskId, [FromBody] PackageRequest request)
    {
        var userIdHeader = Request.Headers["X-User-Id"].ToString();
        Guid.TryParse(userIdHeader, out var userId);
        try
        {
            await _packageService.CreatePackage(taskId, userId, request);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
    }
}