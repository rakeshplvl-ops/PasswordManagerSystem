using PasswordManager.Application.Features.Folders.Dtos;

namespace PasswordManager.Application.Features.Folders;

public interface IFolderService
{
    Task<IReadOnlyList<FolderDto>> GetFoldersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FolderDto?> GetFolderByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<FolderDto> CreateFolderAsync(Guid userId, CreateFolderRequest request, CancellationToken cancellationToken = default);
    Task<FolderDto> UpdateFolderAsync(Guid id, Guid userId, UpdateFolderRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteFolderAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
