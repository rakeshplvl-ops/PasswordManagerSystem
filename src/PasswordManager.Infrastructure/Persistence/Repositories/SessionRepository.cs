using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuthSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.AuthSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.AuthSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > now)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AuthSession session, CancellationToken cancellationToken = default)
    {
        await _context.AuthSessions.AddAsync(session, cancellationToken);
    }

    public void Update(AuthSession session)
    {
        _context.AuthSessions.Update(session);
    }

    public async Task RevokeSessionAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var session = await _context.AuthSessions
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);

        if (session != null)
        {
            session.Revoke();
            _context.AuthSessions.Update(session);
        }
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeSessions = await _context.AuthSessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.Revoke();
            _context.AuthSessions.Update(session);
        }
    }

    public async Task PurgeExpiredSessionsAsync(DateTime threshold, CancellationToken cancellationToken = default)
    {
        var expired = await _context.AuthSessions
            .Where(s => s.ExpiresAt < threshold || (s.RevokedAt != null && s.RevokedAt < threshold))
            .ToListAsync(cancellationToken);

        _context.AuthSessions.RemoveRange(expired);
    }
}
