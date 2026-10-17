using FluentValidation;
using PasswordManager.Application.Common.Exceptions;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Application.Features.Folders.Dtos;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Features.Folders;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateFolderRequest> _createValidator;
    private readonly IValidator<UpdateFolderRequest> _updateValidator;

    public FolderService(
        IFolderRepository folderRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateFolderRequest> createValidator,
        IValidator<UpdateFolderRequest> updateValidator)
    {
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<FolderDto>> GetFoldersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var folders = await _folderRepository.GetByUserIdAsync(userId, cancellationToken);
        return folders.Select(MapToDto).ToList();
    }

    public async Task<FolderDto?> GetFolderByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(id, userId, cancellationToken);
        return folder == null ? null : MapToDto(folder);
    }

    public async Task<FolderDto> CreateFolderAsync(Guid userId, CreateFolderRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var folder = Folder.Create(userId, request.EncryptedName, request.Nonce);
        await _folderRepository.AddAsync(folder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(folder);
    }

    public async Task<FolderDto> UpdateFolderAsync(Guid id, Guid userId, UpdateFolderRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var folder = await _folderRepository.GetByIdAsync(id, userId, cancellationToken);
        if (folder == null)
        {
            throw new NotFoundException($"Folder with ID '{id}' was not found.");
        }

        folder.Update(request.EncryptedName, request.Nonce);
        _folderRepository.Update(folder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(folder);
    }

    public async Task<bool> DeleteFolderAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(id, userId, cancellationToken);
        if (folder == null)
        {
            return false;
        }

        _folderRepository.Delete(folder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static FolderDto MapToDto(Folder folder)
    {
        return new FolderDto(
            folder.Id,
            folder.EncryptedName,
            folder.Nonce,
            folder.CreatedAt,
            folder.UpdatedAt
        );
    }
}
