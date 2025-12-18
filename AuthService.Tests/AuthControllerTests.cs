using AuthService.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
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
}