using PasswordManager.Domain.Common;
using PasswordManager.Domain.Exceptions;

namespace PasswordManager.Domain.Entities;

public class Folder : BaseEntity
{
    public Guid UserId { get; private set; }
    public string EncryptedName { get; private set; } = string.Empty;
    public string Nonce { get; private set; } = string.Empty;

    public virtual User? User { get; private set; }
    public virtual ICollection<VaultEntry> VaultEntries { get; private set; } = new List<VaultEntry>();

    private Folder()
    {
        // Required for EF Core
    }

    public static Folder Create(Guid userId, string encryptedName, string nonce)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(encryptedName))
        {
            throw new DomainException("Encrypted folder name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(nonce))
        {
            throw new DomainException("Nonce cannot be empty.");
        }

        var now = DateTime.UtcNow;
        return new Folder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EncryptedName = encryptedName,
            Nonce = nonce,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string encryptedName, string nonce)
    {
        if (string.IsNullOrWhiteSpace(encryptedName))
        {
            throw new DomainException("Encrypted folder name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(nonce))
        {
            throw new DomainException("Nonce cannot be empty.");
        }

        EncryptedName = encryptedName;
        Nonce = nonce;
        UpdatedAt = DateTime.UtcNow;
    }
}
