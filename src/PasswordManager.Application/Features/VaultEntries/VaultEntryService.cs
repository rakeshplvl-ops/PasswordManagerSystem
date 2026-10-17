using FluentValidation;
using PasswordManager.Application.Common.Exceptions;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Application.Features.VaultEntries.Dtos;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Features.VaultEntries;

public class VaultEntryService : IVaultEntryService
{
    private readonly IVaultEntryRepository _vaultEntryRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateVaultEntryRequest> _createValidator;
    private readonly IValidator<UpdateVaultEntryRequest> _updateValidator;

    public VaultEntryService(
        IVaultEntryRepository vaultEntryRepository,
        IFolderRepository folderRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateVaultEntryRequest> createValidator,
        IValidator<UpdateVaultEntryRequest> updateValidator)
    {
        _vaultEntryRepository = vaultEntryRepository;
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<VaultEntryDto>> GetEntriesAsync(
        Guid userId,
        Guid? folderId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var entries = await _vaultEntryRepository.GetByUserIdAsync(userId, folderId, includeDeleted, cancellationToken);
        return entries.Select(MapToDto).ToList();
    }

    public async Task<VaultEntryDto?> GetEntryByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _vaultEntryRepository.GetByIdAsync(id, userId, cancellationToken);
        return entry == null ? null : MapToDto(entry);
    }

    public async Task<VaultEntryDto> CreateEntryAsync(
        Guid userId,
        CreateVaultEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (request.FolderId.HasValue)
        {
            var folder = await _folderRepository.GetByIdAsync(request.FolderId.Value, userId, cancellationToken);
            if (folder == null)
            {
                throw new NotFoundException($"Folder with ID '{request.FolderId.Value}' was not found.");
            }
        }

        var entry = VaultEntry.Create(
            userId,
            request.Ciphertext,
            request.Nonce,
            request.FolderId,
            request.SchemaVersion
        );

        await _vaultEntryRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(entry);
    }

    public async Task<VaultEntryDto> UpdateEntryAsync(
        Guid id,
        Guid userId,
        UpdateVaultEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entry = await _vaultEntryRepository.GetByIdAsync(id, userId, cancellationToken);
        if (entry == null)
        {
            throw new NotFoundException($"Vault entry with ID '{id}' was not found.");
        }

        if (request.FolderId.HasValue)
        {
            var folder = await _folderRepository.GetByIdAsync(request.FolderId.Value, userId, cancellationToken);
            if (folder == null)
            {
                throw new NotFoundException($"Folder with ID '{request.FolderId.Value}' was not found.");
            }
        }

        entry.Update(request.Ciphertext, request.Nonce, request.SchemaVersion, request.FolderId);
        _vaultEntryRepository.Update(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(entry);
    }

    public async Task<VaultEntryDto> MoveEntryAsync(
        Guid id,
        Guid userId,
        MoveVaultEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var entry = await _vaultEntryRepository.GetByIdAsync(id, userId, cancellationToken);
        if (entry == null)
        {
            throw new NotFoundException($"Vault entry with ID '{id}' was not found.");
        }

        if (request.FolderId.HasValue)
        {
            var folder = await _folderRepository.GetByIdAsync(request.FolderId.Value, userId, cancellationToken);
            if (folder == null)
            {
                throw new NotFoundException($"Folder with ID '{request.FolderId.Value}' was not found.");
            }
        }

        entry.MoveToFolder(request.FolderId);
        _vaultEntryRepository.Update(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(entry);
    }

    public async Task<bool> SoftDeleteEntryAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _vaultEntryRepository.GetByIdAsync(id, userId, cancellationToken);
        if (entry == null)
        {
            return false;
        }

        _vaultEntryRepository.Delete(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreEntryAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _vaultEntryRepository.GetByIdAsync(id, userId, cancellationToken);
        if (entry == null)
        {
            return false;
        }

        entry.Restore();
        _vaultEntryRepository.Update(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PurgeEntryAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _vaultEntryRepository.GetByIdAsync(id, userId, cancellationToken);
        if (entry == null)
        {
            return false;
        }

        _vaultEntryRepository.HardDelete(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static VaultEntryDto MapToDto(VaultEntry entry)
    {
        return new VaultEntryDto(
            entry.Id,
            entry.FolderId,
            entry.Ciphertext,
            entry.Nonce,
            entry.SchemaVersion,
            entry.CreatedAt,
            entry.UpdatedAt,
            entry.DeletedAt
        );
    }
}
