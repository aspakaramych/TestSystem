using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestSystem.Core.DTOs.AuthService;
using TestSystem.Core.Interfaces;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    [HttpGet("health")]
    public Task<IActionResult> HealthCheck()
    {
        _logger.LogInformation("Health check endpoint called.");
        return Task.FromResult<IActionResult>(Ok(new { status = "Healthy" }));
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation($"Login request called. Email: {request.Email} Password: {request.Password}");  
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Validation failed",
                errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                )
            });
        }

        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e.Message);
            return NotFound(e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e.Message);
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation($"Register request called. Email: {request.Email} Password: {request.Password}");  
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Validation failed",
                errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                )
            });
        }

        try
        {
            await _authService.RegisterAsync(request);
            return StatusCode(201);
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e.Message);
            return Conflict(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> Validate()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                          User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var reqUserId))
        {
            return Unauthorized();
        }
        
        return Ok(reqUserId);
    }
}