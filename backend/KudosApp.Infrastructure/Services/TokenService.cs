using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KudosApp.Application.Interfaces;
using KudosApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KudosApp.Infrastructure.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(User user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
