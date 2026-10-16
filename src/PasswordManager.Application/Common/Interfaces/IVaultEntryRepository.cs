using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Common.Interfaces;

public interface IVaultEntryRepository
{
    Task<VaultEntry?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VaultEntry>> GetByUserIdAsync(Guid userId, Guid? folderId = null, CancellationToken cancellationToken = default);
    Task AddAsync(VaultEntry entry, CancellationToken cancellationToken = default);
    void Update(VaultEntry entry);
    void Delete(VaultEntry entry);
}
