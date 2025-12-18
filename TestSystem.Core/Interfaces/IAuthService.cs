using TestSystem.Core.DTOs.AuthService;

namespace TestSystem.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task RegisterAsync(RegisterRequest request);
}