using PasswordManager.Domain.Common;
using PasswordManager.Domain.Exceptions;

namespace PasswordManager.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    // Navigation properties for EF Core
    public virtual VaultSettings? VaultSettings { get; private set; }
    public virtual ICollection<VaultEntry> VaultEntries { get; private set; } = new List<VaultEntry>();
    public virtual ICollection<Folder> Folders { get; private set; } = new List<Folder>();
    public virtual ICollection<AuthSession> AuthSessions { get; private set; } = new List<AuthSession>();

    private User()
    {
        // Required for EF Core
    }

    public static User Create(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email cannot be empty.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (!normalizedEmail.Contains('@') || normalizedEmail.Length > 256)
        {
            throw new DomainException("Email is invalid or exceeds maximum length of 256 characters.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash cannot be empty.");
        }

        var now = DateTime.UtcNow;
        return new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdatePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new DomainException("New password hash cannot be empty.");
        }

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin(DateTime loginTime)
    {
        LastLoginAt = loginTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
