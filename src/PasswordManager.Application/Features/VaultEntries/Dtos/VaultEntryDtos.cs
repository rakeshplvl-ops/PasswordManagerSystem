namespace PasswordManager.Application.Features.VaultEntries.Dtos;

public record VaultEntryDto(
    Guid Id,
    Guid? FolderId,
    string Ciphertext,
    string Nonce,
    int SchemaVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
);

public record CreateVaultEntryRequest(
    string Ciphertext,
    string Nonce,
    Guid? FolderId = null,
    int SchemaVersion = 1
);

public record UpdateVaultEntryRequest(
    string Ciphertext,
    string Nonce,
    Guid? FolderId = null,
    int SchemaVersion = 1
);

public record MoveVaultEntryRequest(
    Guid? FolderId
);
