using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestSystem.Core.DTOs.ClassRoomService;
using TestSystem.Core.Interfaces;

namespace TestSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestSystemController : ControllerBase
{
    private readonly IClassRoomService _classRoomService;

    public TestSystemController(IClassRoomService classRoomService)
    {
        _classRoomService = classRoomService;
    }
    
    [HttpGet("classrooms")]
    [Authorize]
    public async Task<IActionResult> GetClassRooms()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                          User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var reqUserId))
        {
            return Unauthorized();
        }

        try
        {
            var classRooms = await _classRoomService.GetClassRoomsAsync(reqUserId);
            return Ok(classRooms);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("classrooms")]
    [Authorize("AdminOnly")]
    public async Task<IActionResult> CreateClassRoom([FromBody] ClassRoomCreateRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                          User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var reqUserId))
        {
            return Unauthorized();
        }
        try
        {
            await _classRoomService.CreateClassRoomAsync(request);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}