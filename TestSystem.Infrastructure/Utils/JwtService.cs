using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TestSystem.Core.Configuration;
using TestSystem.Core.Entity;

namespace TestSystem.Infrastructure.Utils;

public class JwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
    }
    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };
        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(issuer: _jwtSettings.Issuer, audience: _jwtSettings.Audience, claims: claims,
            signingCredentials: creds, expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}