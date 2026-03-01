using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.Exceptions;
using System.Security.Cryptography;

namespace SmartService.Infrastructure.Auth;

/// <summary>
/// Authentication service implementation.
/// Handles registration, login, token generation, and refresh token management.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IAuthRepository _authRepository;
    private readonly JwtTokenService _jwtService;
    private readonly AesEncryptionService _encryptionService;
    private readonly TokenConfiguration _config;

    public AuthService(
        IAppDbContext context,
        IAuthRepository authRepository,
        JwtTokenService jwtService,
        AesEncryptionService encryptionService,
        IOptions<TokenConfiguration> config)
    {
        _context = context;
        _authRepository = authRepository;
        _jwtService = jwtService;
        _encryptionService = encryptionService;
        _config = config.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingAuth = await _authRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingAuth != null)
            throw new AuthException.EmailAlreadyRegisteredException();

        var user = request.Role switch
        {
            UserRole.Customer => User.CreateCustomer(request.FullName, request.Email, request.PhoneNumber),
            UserRole.Staff => User.CreateStaff(request.FullName, request.Email, request.PhoneNumber),
            UserRole.Agent => User.CreateAgent(request.FullName, request.Email, request.PhoneNumber),
            _ => throw new AuthException.InvalidRoleException()
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var authData = new AuthData
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _authRepository.AddAsync(authData, cancellationToken);

        return await GenerateTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var authData = await _authRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AuthException.InvalidCredentialsException();

        if (!BCrypt.Net.BCrypt.Verify(request.Password, authData.PasswordHash))
            throw new AuthException.InvalidCredentialsException();

        var user = await _context.Users.FindAsync(new object[] { authData.UserId }, cancellationToken)
            ?? throw new AuthException.InvalidCredentialsException();

        return await GenerateTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var encryptedToken = _encryptionService.Encrypt(refreshToken);
        var authData = await _authRepository.GetByRefreshTokenAsync(encryptedToken, cancellationToken);
        
        if (authData == null || authData.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new AuthException.InvalidRefreshTokenException();

        var user = await _context.Users.FindAsync(new object[] { authData.UserId }, cancellationToken)
            ?? throw new AuthException.UserNotFoundException();

        return await GenerateTokensAsync(user, cancellationToken);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var encryptedToken = _encryptionService.Encrypt(refreshToken);
            var authData = await _authRepository.GetByRefreshTokenAsync(encryptedToken, cancellationToken);
            
            if (authData == null) return false;

            ((AuthData)authData).EncryptedRefreshToken = null;
            ((AuthData)authData).RefreshTokenExpiresAt = null;
            ((AuthData)authData).UpdatedAt = DateTime.UtcNow;
            await _authRepository.UpdateAsync(authData, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> VerifyRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var encryptedToken = _encryptionService.Encrypt(refreshToken);
            var authData = await _authRepository.GetByRefreshTokenAsync(encryptedToken, cancellationToken);
            return authData != null && authData.RefreshTokenExpiresAt > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    private async Task<AuthResult> GenerateTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.FullName, user.Role, user.PhoneNumber);
        var rawRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        var authData = await _authRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (authData != null)
        {
            ((AuthData)authData).EncryptedRefreshToken = _encryptionService.Encrypt(rawRefreshToken);
            ((AuthData)authData).RefreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiration();
            ((AuthData)authData).UpdatedAt = DateTime.UtcNow;
            await _authRepository.UpdateAsync(authData, cancellationToken);
        }

        return new AuthResult(
            AccessToken: accessToken,
            RefreshToken: rawRefreshToken,
            AccessTokenExpiresAt: _jwtService.GetAccessTokenExpiration(),
            RefreshTokenExpiresAt: _jwtService.GetRefreshTokenExpiration(),
            UserId: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role
        );
    }
}
