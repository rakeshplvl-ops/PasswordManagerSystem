using FluentValidation;
using PasswordManager.Application.Common.Exceptions;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Application.Features.Folders;
using PasswordManager.Application.Features.Folders.Dtos;
using PasswordManager.Application.Features.Folders.Validators;
using PasswordManager.Domain.Entities;
using Xunit;

namespace PasswordManager.Application.Tests;

public class FolderServiceTests
{
    private class FakeFolderRepository : IFolderRepository
    {
        public readonly List<Folder> Folders = new();

        public Task<Folder?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Folders.FirstOrDefault(f => f.Id == id && f.UserId == userId));

        public Task<IReadOnlyList<Folder>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Folder>>(Folders.Where(f => f.UserId == userId).ToList());

        public Task AddAsync(Folder folder, CancellationToken cancellationToken = default)
        {
            Folders.Add(folder);
            return Task.CompletedTask;
        }

        public void Update(Folder folder)
        {
            var index = Folders.FindIndex(f => f.Id == folder.Id);
            if (index >= 0) Folders[index] = folder;
        }

        public void Delete(Folder folder)
        {
            Folders.RemoveAll(f => f.Id == folder.Id);
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }
    }

    private readonly FakeFolderRepository _folderRepo = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly IValidator<CreateFolderRequest> _createValidator = new CreateFolderRequestValidator();
    private readonly IValidator<UpdateFolderRequest> _updateValidator = new UpdateFolderRequestValidator();

    private FolderService CreateService()
    {
        return new FolderService(
            _folderRepo,
            _unitOfWork,
            _createValidator,
            _updateValidator
        );
    }

    [Fact]
    public async Task CreateFolderAsync_WithValidData_CreatesAndSaves()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var request = new CreateFolderRequest("encryptedName==", "nonce==");

        var result = await service.CreateFolderAsync(userId, request);

        Assert.NotNull(result);
        Assert.Equal("encryptedName==", result.EncryptedName);
        Assert.Equal("nonce==", result.Nonce);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Single(_folderRepo.Folders);
    }

    [Fact]
    public async Task UpdateFolderAsync_UpdatesEncryptedName()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var folder = Folder.Create(userId, "oldEncrypted==", "oldNonce==");
        _folderRepo.Folders.Add(folder);

        var request = new UpdateFolderRequest("newEncrypted==", "newNonce==");
        var result = await service.UpdateFolderAsync(folder.Id, userId, request);

        Assert.NotNull(result);
        Assert.Equal("newEncrypted==", result.EncryptedName);
        Assert.Equal("newNonce==", result.Nonce);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task DeleteFolderAsync_RemovesFolder()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var folder = Folder.Create(userId, "name==", "nonce==");
        _folderRepo.Folders.Add(folder);

        var success = await service.DeleteFolderAsync(folder.Id, userId);

        Assert.True(success);
        Assert.Empty(_folderRepo.Folders);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task TenantIsolation_UserCannotAccessOrMutateAnotherUsersFolder()
    {
        var service = CreateService();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var folderB = Folder.Create(userB, "userBFolder==", "nonce==");
        _folderRepo.Folders.Add(folderB);

        // User A cannot get User B's folder
        var fetched = await service.GetFolderByIdAsync(folderB.Id, userA);
        Assert.Null(fetched);

        // User A cannot update User B's folder
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateFolderAsync(folderB.Id, userA, new UpdateFolderRequest("hacked==", "nonce==")));

        // User A cannot delete User B's folder
        var deleted = await service.DeleteFolderAsync(folderB.Id, userA);
        Assert.False(deleted);

        Assert.Single(_folderRepo.Folders);
        Assert.Equal("userBFolder==", _folderRepo.Folders[0].EncryptedName);
    }
}
