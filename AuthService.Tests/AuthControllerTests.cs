using AuthService.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TestSystem.Core.DTOs.AuthService;
using TestSystem.Core.Interfaces;
using Xunit;

namespace AuthService.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _authController = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var result = await _authController.HealthCheck();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(new { status = "Healthy" });
    }
    
    [Fact]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        var loginReq = new LoginRequest
        {
            Email = "",
            Password = "",
        };
        _authController.ModelState.AddModelError("Email", "The Email field is required.");
        _authController.ModelState.AddModelError("Password", "The Password field is required.");
        
        var result = await _authController.Login(loginReq);
        
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_NotFound_ReturnsNotFound()
    {
        var loginReq = new LoginRequest
        {
            Email = "someemailwhichnotexist@mail.ru",
            Password = "oksdkfopskfope"
        };
        _authServiceMock.Setup(x => x.LoginAsync(loginReq))
            .ThrowsAsync(new KeyNotFoundException($"User with {loginReq.Email} not found"));
        var result = await _authController.Login(loginReq);
        result.Should().BeOfType<NotFoundObjectResult>();
    }
    
    [Fact]
    public async Task Register_WithInvalid_Request()
    {
        var registerReq = new RegisterRequest
        {
            Email = "invalidemail",
            Password = "short"
        };
        _authController.ModelState.AddModelError("Email", "Email is invalid.");
        _authController.ModelState.AddModelError("Password", "Password must be at least 6 characters");
        var result = await _authController.Register(registerReq);
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().NotBeNull();
    }
}