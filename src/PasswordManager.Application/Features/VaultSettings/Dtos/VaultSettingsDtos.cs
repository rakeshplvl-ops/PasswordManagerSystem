namespace PasswordManager.Application.Features.VaultSettings.Dtos;

public record VaultSettingsDto(
    Guid UserId,
    string EncryptedVek,
    string VekNonce,
    string KdfAlgorithm,
    int KdfIterations,
    int KdfMemoryKb,
    int KdfParallelism,
    string KdfSalt,
    int SchemaVersion,
    DateTime UpdatedAt
);

public record UpdateVaultSettingsRequest(
    string EncryptedVek,
    string VekNonce,
    string KdfAlgorithm,
    int KdfIterations,
    int KdfMemoryKb,
    int KdfParallelism,
    string KdfSalt,
    int SchemaVersion = 1
);
