using PasswordManager.Domain.Common;
using PasswordManager.Domain.Exceptions;

namespace PasswordManager.Domain.Entities;

public class AuthSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public bool IsActive => !RevokedAt.HasValue && ExpiresAt > DateTime.UtcNow;

    public virtual User? User { get; private set; }

    private AuthSession()
    {
        // Required for EF Core
    }

    public static AuthSession Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new DomainException("Session token hash cannot be empty.");
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new DomainException("Session expiry must be in the future.");
        }

        var now = DateTime.UtcNow;
        return new AuthSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Revoke()
    {
        if (!RevokedAt.HasValue)
        {
            RevokedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
