using PasswordManager.Domain.Common;
using PasswordManager.Domain.Exceptions;

namespace PasswordManager.Domain.Entities;

public class VaultSettings : BaseEntity
{
    public Guid UserId { get; private set; }
    public string EncryptedVek { get; private set; } = string.Empty;
    public string VekNonce { get; private set; } = string.Empty;
    public string KdfAlgorithm { get; private set; } = "Argon2id";
    public int KdfIterations { get; private set; }
    public int KdfMemoryKb { get; private set; }
    public int KdfParallelism { get; private set; }
    public string KdfSalt { get; private set; } = string.Empty;
    public int SchemaVersion { get; private set; } = 1;

    public virtual User? User { get; private set; }

    private VaultSettings()
    {
        // Required for EF Core
    }

    public static VaultSettings Create(
        Guid userId,
        string encryptedVek,
        string vekNonce,
        string kdfAlgorithm,
        int kdfIterations,
        int kdfMemoryKb,
        int kdfParallelism,
        string kdfSalt,
        int schemaVersion = 1)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(encryptedVek))
        {
            throw new DomainException("Encrypted VEK cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(vekNonce))
        {
            throw new DomainException("VEK nonce cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(kdfAlgorithm))
        {
            throw new DomainException("KDF algorithm cannot be empty.");
        }

        if (kdfIterations < 1)
        {
            throw new DomainException("KDF iterations must be at least 1.");
        }

        if (kdfMemoryKb < 1024)
        {
            throw new DomainException("KDF memory must be at least 1024 KB.");
        }

        if (kdfParallelism < 1)
        {
            throw new DomainException("KDF parallelism must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(kdfSalt))
        {
            throw new DomainException("KDF salt cannot be empty.");
        }

        if (schemaVersion < 1)
        {
            throw new DomainException("Schema version must be positive.");
        }

        var now = DateTime.UtcNow;
        return new VaultSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EncryptedVek = encryptedVek,
            VekNonce = vekNonce,
            KdfAlgorithm = kdfAlgorithm,
            KdfIterations = kdfIterations,
            KdfMemoryKb = kdfMemoryKb,
            KdfParallelism = kdfParallelism,
            KdfSalt = kdfSalt,
            SchemaVersion = schemaVersion,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        string encryptedVek,
        string vekNonce,
        string kdfAlgorithm,
        int kdfIterations,
        int kdfMemoryKb,
        int kdfParallelism,
        string kdfSalt,
        int schemaVersion)
    {
        if (string.IsNullOrWhiteSpace(encryptedVek))
        {
            throw new DomainException("Encrypted VEK cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(vekNonce))
        {
            throw new DomainException("VEK nonce cannot be empty.");
        }

        if (kdfIterations < 1 || kdfMemoryKb < 1024 || kdfParallelism < 1)
        {
            throw new DomainException("Invalid KDF parameters.");
        }

        EncryptedVek = encryptedVek;
        VekNonce = vekNonce;
        KdfAlgorithm = kdfAlgorithm;
        KdfIterations = kdfIterations;
        KdfMemoryKb = kdfMemoryKb;
        KdfParallelism = kdfParallelism;
        KdfSalt = kdfSalt;
        SchemaVersion = schemaVersion;
        UpdatedAt = DateTime.UtcNow;
    }
}
