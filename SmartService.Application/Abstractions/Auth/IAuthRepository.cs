namespace SmartService.Application.Abstractions.Auth;

/// <summary>
/// Repository for authentication data operations.
/// This abstraction allows Application layer to work with auth data
/// without depending on Infrastructure implementation.
/// </summary>
public interface IAuthRepository
{
    Task<IAuthData?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IAuthData?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IAuthData?> GetByRefreshTokenAsync(string encryptedRefreshToken, CancellationToken cancellationToken = default);
    Task AddAsync(IAuthData authData, CancellationToken cancellationToken = default);
    Task UpdateAsync(IAuthData authData, CancellationToken cancellationToken = default);
}
