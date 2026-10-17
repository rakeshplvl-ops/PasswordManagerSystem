using PasswordManager.Application.Features.VaultEntries.Dtos;

namespace PasswordManager.Application.Features.VaultEntries;

public interface IVaultEntryService
{
    Task<IReadOnlyList<VaultEntryDto>> GetEntriesAsync(
        Guid userId,
        Guid? folderId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    Task<VaultEntryDto?> GetEntryByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<VaultEntryDto> CreateEntryAsync(
        Guid userId,
        CreateVaultEntryRequest request,
        CancellationToken cancellationToken = default);

    Task<VaultEntryDto> UpdateEntryAsync(
        Guid id,
        Guid userId,
        UpdateVaultEntryRequest request,
        CancellationToken cancellationToken = default);

    Task<VaultEntryDto> MoveEntryAsync(
        Guid id,
        Guid userId,
        MoveVaultEntryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteEntryAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> RestoreEntryAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> PurgeEntryAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);
}
