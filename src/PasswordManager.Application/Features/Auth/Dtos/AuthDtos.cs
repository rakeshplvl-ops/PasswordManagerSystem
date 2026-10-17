namespace PasswordManager.Application.Features.Auth.Dtos;

public record InitialVaultSettingsDto(
    string EncryptedVek,
    string VekNonce,
    string KdfAlgorithm,
    int KdfIterations,
    int KdfMemoryKb,
    int KdfParallelism,
    string KdfSalt,
    int SchemaVersion = 1
);

public record RegisterRequest(
    string Email,
    string Password,
    InitialVaultSettingsDto? VaultSettings = null
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    Guid UserId,
    string Email
);

public record CurrentUserResponse(
    Guid Id,
    string Email,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool HasVaultSettings
);
