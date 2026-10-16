using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Common.Interfaces;

public interface IVaultSettingsRepository
{
    Task<VaultSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(VaultSettings settings, CancellationToken cancellationToken = default);
    void Update(VaultSettings settings);
}
