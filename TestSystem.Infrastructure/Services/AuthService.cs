using TestSystem.Core.DTOs.AuthService;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Utils;

namespace TestSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwtService;
    
    public AuthService(IUserRepository userRepository, JwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }
    
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with {request.Email} not found");
        }
        var isPasswordValid = PasswordHasher.VerifyPassword(request.Password, user.HashPassword);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid password");
        }
        var token = _jwtService.GenerateAccessToken(user);
        return new AuthResponse
        {
            accessToken = token
        };
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ArgumentException($"User with {request.Email} already exists");
        }
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            HashPassword = PasswordHasher.HashPassword(request.Password),
            Role = UserRole.User,
        };
        await _userRepository.AddAsync(user);
    }
}