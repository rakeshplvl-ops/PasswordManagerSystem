using FluentValidation;
using PasswordManager.Application.Common.Exceptions;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Application.Features.VaultEntries;
using PasswordManager.Application.Features.VaultEntries.Dtos;
using PasswordManager.Application.Features.VaultEntries.Validators;
using PasswordManager.Domain.Entities;
using Xunit;

namespace PasswordManager.Application.Tests;

public class VaultEntryServiceTests
{
    private class FakeVaultEntryRepository : IVaultEntryRepository
    {
        public readonly List<VaultEntry> Entries = new();

        public Task<VaultEntry?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Entries.FirstOrDefault(e => e.Id == id && e.UserId == userId));

        public Task<IReadOnlyList<VaultEntry>> GetByUserIdAsync(
            Guid userId,
            Guid? folderId = null,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var query = Entries.Where(e => e.UserId == userId);

            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            if (folderId.HasValue)
            {
                query = query.Where(e => e.FolderId == folderId.Value);
            }

            return Task.FromResult<IReadOnlyList<VaultEntry>>(query.OrderByDescending(e => e.UpdatedAt).ToList());
        }

        public Task AddAsync(VaultEntry entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public void Update(VaultEntry entry)
        {
            var index = Entries.FindIndex(e => e.Id == entry.Id);
            if (index >= 0) Entries[index] = entry;
        }

        public void Delete(VaultEntry entry)
        {
            entry.SoftDelete();
            Update(entry);
        }

        public void HardDelete(VaultEntry entry)
        {
            Entries.RemoveAll(e => e.Id == entry.Id);
        }
    }

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

    private readonly FakeVaultEntryRepository _entryRepo = new();
    private readonly FakeFolderRepository _folderRepo = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly IValidator<CreateVaultEntryRequest> _createValidator = new CreateVaultEntryRequestValidator();
    private readonly IValidator<UpdateVaultEntryRequest> _updateValidator = new UpdateVaultEntryRequestValidator();

    private VaultEntryService CreateService()
    {
        return new VaultEntryService(
            _entryRepo,
            _folderRepo,
            _unitOfWork,
            _createValidator,
            _updateValidator
        );
    }

    [Fact]
    public async Task CreateEntryAsync_WithValidData_CreatesAndSaves()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var request = new CreateVaultEntryRequest("ciphertextBase64==", "nonceBase64==", null, 1);

        var result = await service.CreateEntryAsync(userId, request);

        Assert.NotNull(result);
        Assert.Equal("ciphertextBase64==", result.Ciphertext);
        Assert.Equal("nonceBase64==", result.Nonce);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Single(_entryRepo.Entries);
    }

    [Fact]
    public async Task CreateEntryAsync_WithNonExistentFolder_ThrowsNotFoundException()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var request = new CreateVaultEntryRequest("ciphertextBase64==", "nonceBase64==", Guid.NewGuid(), 1);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateEntryAsync(userId, request));
    }

    [Fact]
    public async Task CreateEntryAsync_WithForeignFolder_ThrowsNotFoundException()
    {
        var service = CreateService();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var foreignFolder = Folder.Create(userB, "EncryptedFolder==", "nonce==");
        _folderRepo.Folders.Add(foreignFolder);

        var request = new CreateVaultEntryRequest("ciphertextBase64==", "nonceBase64==", foreignFolder.Id, 1);

        // User A cannot attach an entry to User B's folder!
        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateEntryAsync(userA, request));
    }

    [Fact]
    public async Task GetEntriesAsync_ExcludesSoftDeletedByDefault()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var active = VaultEntry.Create(userId, "activeCipher==", "nonce==");
        var deleted = VaultEntry.Create(userId, "deletedCipher==", "nonce==");
        deleted.SoftDelete();

        _entryRepo.Entries.Add(active);
        _entryRepo.Entries.Add(deleted);

        var result = await service.GetEntriesAsync(userId, includeDeleted: false);

        Assert.Single(result);
        Assert.Equal(active.Id, result[0].Id);
    }

    [Fact]
    public async Task GetEntriesAsync_IncludesSoftDeletedWhenRequested()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var active = VaultEntry.Create(userId, "activeCipher==", "nonce==");
        var deleted = VaultEntry.Create(userId, "deletedCipher==", "nonce==");
        deleted.SoftDelete();

        _entryRepo.Entries.Add(active);
        _entryRepo.Entries.Add(deleted);

        var result = await service.GetEntriesAsync(userId, includeDeleted: true);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SoftDeleteEntryAsync_And_RestoreEntryAsync_WorkCorrectly()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var entry = VaultEntry.Create(userId, "cipher==", "nonce==");
        _entryRepo.Entries.Add(entry);

        var deleteSuccess = await service.SoftDeleteEntryAsync(entry.Id, userId);
        Assert.True(deleteSuccess);
        Assert.NotNull(_entryRepo.Entries[0].DeletedAt);

        var restoreSuccess = await service.RestoreEntryAsync(entry.Id, userId);
        Assert.True(restoreSuccess);
        Assert.Null(_entryRepo.Entries[0].DeletedAt);
    }

    [Fact]
    public async Task PurgeEntryAsync_HardDeletesEntry()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var entry = VaultEntry.Create(userId, "cipher==", "nonce==");
        _entryRepo.Entries.Add(entry);

        var purgeSuccess = await service.PurgeEntryAsync(entry.Id, userId);
        Assert.True(purgeSuccess);
        Assert.Empty(_entryRepo.Entries);
    }

    [Fact]
    public async Task TenantIsolation_UserCannotAccessOrMutateAnotherUsersEntry()
    {
        var service = CreateService();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var entryB = VaultEntry.Create(userB, "userBCipher==", "nonce==");
        _entryRepo.Entries.Add(entryB);

        // User A attempts to read User B's entry
        var fetched = await service.GetEntryByIdAsync(entryB.Id, userA);
        Assert.Null(fetched);

        // User A attempts to update User B's entry
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateEntryAsync(entryB.Id, userA, new UpdateVaultEntryRequest("hacked==", "nonce==")));

        // User A attempts to soft delete User B's entry
        var deleted = await service.SoftDeleteEntryAsync(entryB.Id, userA);
        Assert.False(deleted);

        // User A attempts to purge User B's entry
        var purged = await service.PurgeEntryAsync(entryB.Id, userA);
        Assert.False(purged);

        // User B's entry remains unmodified
        Assert.Single(_entryRepo.Entries);
        Assert.Equal("userBCipher==", _entryRepo.Entries[0].Ciphertext);
    }
}
