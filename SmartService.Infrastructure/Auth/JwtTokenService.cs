using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartService.Application.Abstractions.Auth;
using SmartService.Domain.ValueObjects;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartService.Infrastructure.Auth;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public class JwtTokenService
{
    private readonly TokenConfiguration _config;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtTokenService(IOptions<TokenConfiguration> config)
    {
        _config = config.Value;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public string GenerateAccessToken(Guid userId, string email, string fullName, Domain.Entities.UserRole role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.SecretKey));
        var token = new JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, role.ToAuthorizationRole()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            },
            expires: DateTime.UtcNow.AddMinutes(_config.AccessTokenLifetimeMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return _tokenHandler.WriteToken(token);
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_config.AccessTokenLifetimeMinutes);
    }

    public DateTime GetRefreshTokenExpiration()
    {
        return DateTime.UtcNow.AddDays(_config.RefreshTokenLifetimeDays);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            return _tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config.Issuer,
                ValidAudience = _config.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.SecretKey))
            }, out _);
        }
        catch
        {
            return null;
        }
    }
}
