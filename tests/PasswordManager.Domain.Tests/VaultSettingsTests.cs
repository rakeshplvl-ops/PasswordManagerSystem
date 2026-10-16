using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Exceptions;
using Xunit;

namespace PasswordManager.Domain.Tests;

public class VaultSettingsTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateVaultSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var settings = VaultSettings.Create(
            userId: userId,
            encryptedVek: "base64_encrypted_vek",
            vekNonce: "base64_nonce",
            kdfAlgorithm: "Argon2id",
            kdfIterations: 3,
            kdfMemoryKb: 65536,
            kdfParallelism: 4,
            kdfSalt: "base64_salt",
            schemaVersion: 1
        );

        // Assert
        Assert.NotEqual(Guid.Empty, settings.Id);
        Assert.Equal(userId, settings.UserId);
        Assert.Equal("base64_encrypted_vek", settings.EncryptedVek);
        Assert.Equal("base64_nonce", settings.VekNonce);
        Assert.Equal("Argon2id", settings.KdfAlgorithm);
        Assert.Equal(3, settings.KdfIterations);
        Assert.Equal(65536, settings.KdfMemoryKb);
        Assert.Equal(4, settings.KdfParallelism);
        Assert.Equal("base64_salt", settings.KdfSalt);
        Assert.Equal(1, settings.SchemaVersion);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => VaultSettings.Create(
            userId: Guid.Empty,
            encryptedVek: "vek",
            vekNonce: "nonce",
            kdfAlgorithm: "Argon2id",
            kdfIterations: 3,
            kdfMemoryKb: 65536,
            kdfParallelism: 4,
            kdfSalt: "salt"
        ));
    }

    [Theory]
    [InlineData(0, 65536, 4)]
    [InlineData(3, 512, 4)]
    [InlineData(3, 65536, 0)]
    public void Create_WithInvalidKdfParameters_ShouldThrowDomainException(int iterations, int memoryKb, int parallelism)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => VaultSettings.Create(
            userId: Guid.NewGuid(),
            encryptedVek: "vek",
            vekNonce: "nonce",
            kdfAlgorithm: "Argon2id",
            kdfIterations: iterations,
            kdfMemoryKb: memoryKb,
            kdfParallelism: parallelism,
            kdfSalt: "salt"
        ));
    }
}
