using PasswordManager.Application.Features.VaultSettings.Dtos;

namespace PasswordManager.Application.Features.VaultSettings;

public interface IVaultSettingsService
{
    Task<VaultSettingsDto?> GetSettingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<VaultSettingsDto> UpdateSettingsAsync(Guid userId, UpdateVaultSettingsRequest request, CancellationToken cancellationToken = default);
}
