using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestSystem.Core.DTOs.ClassRoomService;
using TestSystem.Core.Interfaces;

namespace TestSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassroomsController : ControllerBase
{
    private readonly IClassRoomService _classRoomService;

    public ClassroomsController(IClassRoomService classRoomService)
    {
        _classRoomService = classRoomService;
    }
    
    [HttpGet("classrooms")]
    public async Task<IActionResult> GetClassRooms()
    {
        var userIdHeader = Request.Headers["X-User-Id"].ToString();
        Guid.TryParse(userIdHeader, out var userId);

        try
        {
            var classRooms = await _classRoomService.GetClassRoomsAsync(userId);
            return Ok(classRooms);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("classrooms")]
    public async Task<IActionResult> CreateClassRoom([FromBody] ClassRoomCreateRequest request)
    {
        var userIdHeader = Request.Headers["X-User-Id"].ToString();
        Guid.TryParse(userIdHeader, out var userId);
        try
        {
            await _classRoomService.CreateClassRoomAsync(request, userId);
            return StatusCode(201);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}