using PasswordManager.Domain.Common;
using PasswordManager.Domain.Exceptions;

namespace PasswordManager.Domain.Entities;

public class VaultEntry : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid? FolderId { get; private set; }
    public string Ciphertext { get; private set; } = string.Empty;
    public string Nonce { get; private set; } = string.Empty;
    public int SchemaVersion { get; private set; } = 1;
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    public virtual User? User { get; private set; }
    public virtual Folder? Folder { get; private set; }

    private VaultEntry()
    {
        // Required for EF Core
    }

    public static VaultEntry Create(
        Guid userId,
        string ciphertext,
        string nonce,
        Guid? folderId = null,
        int schemaVersion = 1)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(ciphertext))
        {
            throw new DomainException("Ciphertext cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(nonce))
        {
            throw new DomainException("Nonce cannot be empty.");
        }

        if (schemaVersion < 1)
        {
            throw new DomainException("Schema version must be positive.");
        }

        var now = DateTime.UtcNow;
        return new VaultEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FolderId = folderId,
            Ciphertext = ciphertext,
            Nonce = nonce,
            SchemaVersion = schemaVersion,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string ciphertext, string nonce, int schemaVersion = 1, Guid? folderId = null)
    {
        if (string.IsNullOrWhiteSpace(ciphertext))
        {
            throw new DomainException("Ciphertext cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(nonce))
        {
            throw new DomainException("Nonce cannot be empty.");
        }

        if (schemaVersion < 1)
        {
            throw new DomainException("Schema version must be positive.");
        }

        Ciphertext = ciphertext;
        Nonce = nonce;
        SchemaVersion = schemaVersion;
        FolderId = folderId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToFolder(Guid? folderId)
    {
        FolderId = folderId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (!DeletedAt.HasValue)
        {
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Restore()
    {
        if (DeletedAt.HasValue)
        {
            DeletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
