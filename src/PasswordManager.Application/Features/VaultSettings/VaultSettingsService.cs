using FluentValidation;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Application.Features.VaultSettings.Dtos;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Features.VaultSettings;

public class VaultSettingsService : IVaultSettingsService
{
    private readonly IVaultSettingsRepository _vaultSettingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateVaultSettingsRequest> _validator;

    public VaultSettingsService(
        IVaultSettingsRepository vaultSettingsRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateVaultSettingsRequest> validator)
    {
        _vaultSettingsRepository = vaultSettingsRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<VaultSettingsDto?> GetSettingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = await _vaultSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (settings == null)
        {
            return null;
        }

        return MapToDto(settings);
    }

    public async Task<VaultSettingsDto> UpdateSettingsAsync(
        Guid userId,
        UpdateVaultSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var settings = await _vaultSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (settings == null)
        {
            settings = Domain.Entities.VaultSettings.Create(
                userId,
                request.EncryptedVek,
                request.VekNonce,
                request.KdfAlgorithm,
                request.KdfIterations,
                request.KdfMemoryKb,
                request.KdfParallelism,
                request.KdfSalt,
                request.SchemaVersion
            );
            await _vaultSettingsRepository.AddAsync(settings, cancellationToken);
        }
        else
        {
            settings.Update(
                request.EncryptedVek,
                request.VekNonce,
                request.KdfAlgorithm,
                request.KdfIterations,
                request.KdfMemoryKb,
                request.KdfParallelism,
                request.KdfSalt,
                request.SchemaVersion
            );
            _vaultSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(settings);
    }

    private static VaultSettingsDto MapToDto(Domain.Entities.VaultSettings settings)
    {
        return new VaultSettingsDto(
            settings.UserId,
            settings.EncryptedVek,
            settings.VekNonce,
            settings.KdfAlgorithm,
            settings.KdfIterations,
            settings.KdfMemoryKb,
            settings.KdfParallelism,
            settings.KdfSalt,
            settings.SchemaVersion,
            settings.UpdatedAt
        );
    }
}
