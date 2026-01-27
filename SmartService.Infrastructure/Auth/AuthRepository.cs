using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Infrastructure.Persistence;

namespace SmartService.Infrastructure.Auth;

/// <summary>
/// Repository implementation for authentication data operations.
/// </summary>
public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IAuthData?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.AuthData
            .FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
    }

    public async Task<IAuthData?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AuthData
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
    }

    public async Task<IAuthData?> GetByRefreshTokenAsync(string encryptedRefreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.AuthData
            .FirstOrDefaultAsync(a => a.EncryptedRefreshToken == encryptedRefreshToken, cancellationToken);
    }

    public async Task AddAsync(IAuthData authData, CancellationToken cancellationToken = default)
    {
        _context.AuthData.Add((AuthData)authData);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(IAuthData authData, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
