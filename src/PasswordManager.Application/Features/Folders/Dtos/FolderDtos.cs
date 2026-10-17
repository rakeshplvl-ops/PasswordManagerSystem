namespace PasswordManager.Application.Features.Folders.Dtos;

public record FolderDto(
    Guid Id,
    string EncryptedName,
    string Nonce,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateFolderRequest(
    string EncryptedName,
    string Nonce
);

public record UpdateFolderRequest(
    string EncryptedName,
    string Nonce
);
