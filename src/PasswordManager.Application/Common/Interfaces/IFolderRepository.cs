using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Common.Interfaces;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Folder>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Folder folder, CancellationToken cancellationToken = default);
    void Update(Folder folder);
    void Delete(Folder folder);
}
