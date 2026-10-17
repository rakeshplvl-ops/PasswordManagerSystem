using PasswordManager.Application.Features.Auth.Dtos;

namespace PasswordManager.Application.Features.Auth;

public interface IAuthService
{
    Task<(AuthResponse Response, string RawSessionToken)> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

    Task<(AuthResponse Response, string RawSessionToken)> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string rawSessionToken,
        CancellationToken cancellationToken = default);

    Task<CurrentUserResponse?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
