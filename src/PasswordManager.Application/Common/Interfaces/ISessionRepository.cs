using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Common.Interfaces;

public interface ISessionRepository
{
    Task<AuthSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(AuthSession session, CancellationToken cancellationToken = default);
    void Update(AuthSession session);
    Task RevokeSessionAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task PurgeExpiredSessionsAsync(DateTime threshold, CancellationToken cancellationToken = default);
}
