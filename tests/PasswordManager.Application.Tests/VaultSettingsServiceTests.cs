using FluentValidation;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Application.Features.VaultSettings;
using PasswordManager.Application.Features.VaultSettings.Dtos;
using PasswordManager.Application.Features.VaultSettings.Validators;
using PasswordManager.Domain.Entities;
using Xunit;

namespace PasswordManager.Application.Tests;

public class VaultSettingsServiceTests
{
    private class FakeVaultSettingsRepository : IVaultSettingsRepository
    {
        public readonly List<VaultSettings> Settings = new();

        public Task<VaultSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Settings.FirstOrDefault(s => s.UserId == userId));

        public Task AddAsync(VaultSettings settings, CancellationToken cancellationToken = default)
        {
            Settings.Add(settings);
            return Task.CompletedTask;
        }

        public void Update(VaultSettings settings)
        {
            var index = Settings.FindIndex(s => s.Id == settings.Id);
            if (index >= 0) Settings[index] = settings;
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

    private readonly FakeVaultSettingsRepository _vaultSettingsRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly IValidator<UpdateVaultSettingsRequest> _validator = new UpdateVaultSettingsRequestValidator();

    private VaultSettingsService CreateService()
    {
        return new VaultSettingsService(
            _vaultSettingsRepository,
            _unitOfWork,
            _validator
        );
    }

    [Fact]
    public async Task GetSettingsAsync_WhenNoneExist_ReturnsNull()
    {
        var service = CreateService();
        var result = await service.GetSettingsAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenSettingsExist_ReturnsDto()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var settings = VaultSettings.Create(
            userId,
            "encVekBase64==",
            "nonceBase64==",
            "Argon2id",
            3,
            65536,
            4,
            "saltBase64==",
            1
        );
        _vaultSettingsRepository.Settings.Add(settings);

        var result = await service.GetSettingsAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("encVekBase64==", result.EncryptedVek);
        Assert.Equal("nonceBase64==", result.VekNonce);
        Assert.Equal("Argon2id", result.KdfAlgorithm);
        Assert.Equal(3, result.KdfIterations);
        Assert.Equal(65536, result.KdfMemoryKb);
        Assert.Equal(4, result.KdfParallelism);
        Assert.Equal("saltBase64==", result.KdfSalt);
        Assert.Equal(1, result.SchemaVersion);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenNoneExist_CreatesAndReturnsDto()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var request = new UpdateVaultSettingsRequest(
            "newEncVek==",
            "newNonce==",
            "Argon2id",
            3,
            65536,
            4,
            "newSalt==",
            1
        );

        var result = await service.UpdateSettingsAsync(userId, request);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("newEncVek==", result.EncryptedVek);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Single(_vaultSettingsRepository.Settings);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenExists_UpdatesAndReturnsDto()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var settings = VaultSettings.Create(
            userId,
            "oldEncVek==",
            "oldNonce==",
            "Argon2id",
            3,
            65536,
            4,
            "oldSalt==",
            1
        );
        _vaultSettingsRepository.Settings.Add(settings);

        var request = new UpdateVaultSettingsRequest(
            "updatedEncVek==",
            "updatedNonce==",
            "Argon2id",
            4,
            131072,
            8,
            "updatedSalt==",
            2
        );

        var result = await service.UpdateSettingsAsync(userId, request);

        Assert.NotNull(result);
        Assert.Equal("updatedEncVek==", result.EncryptedVek);
        Assert.Equal("updatedNonce==", result.VekNonce);
        Assert.Equal(4, result.KdfIterations);
        Assert.Equal(131072, result.KdfMemoryKb);
        Assert.Equal(8, result.KdfParallelism);
        Assert.Equal(2, result.SchemaVersion);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Theory]
    [InlineData("", "nonce==", "Argon2id", 3, 65536, 4, "salt==")]
    [InlineData("vek==", "", "Argon2id", 3, 65536, 4, "salt==")]
    [InlineData("vek==", "nonce==", "PBKDF2", 3, 65536, 4, "salt==")]
    [InlineData("vek==", "nonce==", "Argon2id", 0, 65536, 4, "salt==")]
    [InlineData("vek==", "nonce==", "Argon2id", 3, 512, 4, "salt==")]
    [InlineData("vek==", "nonce==", "Argon2id", 3, 65536, 0, "salt==")]
    [InlineData("vek==", "nonce==", "Argon2id", 3, 65536, 4, "")]
    public async Task UpdateSettingsAsync_WithInvalidInput_ThrowsValidationException(
        string vek, string nonce, string algo, int iterations, int memory, int parallelism, string salt)
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var request = new UpdateVaultSettingsRequest(vek, nonce, algo, iterations, memory, parallelism, salt, 1);

        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateSettingsAsync(userId, request));
    }
}
