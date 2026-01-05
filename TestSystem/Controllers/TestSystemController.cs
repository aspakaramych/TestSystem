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
    private readonly ILogger<ClassroomsController> _logger;

    public ClassroomsController(IClassRoomService classRoomService, ILogger<ClassroomsController> logger)
    {
        _classRoomService = classRoomService;
        _logger = logger;
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
    [HttpGet("health")]
    public Task<IActionResult> HealthCheck()
    {
        _logger.LogInformation("Health check endpoint called.");
        return Task.FromResult<IActionResult>(Ok(new { status = "Healthy" }));
    }
}