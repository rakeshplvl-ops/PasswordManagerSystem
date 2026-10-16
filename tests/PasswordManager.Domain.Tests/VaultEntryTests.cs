using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Exceptions;
using Xunit;

namespace PasswordManager.Domain.Tests;

public class VaultEntryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateVaultEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var ciphertext = "base64_ciphertext_payload";
        var nonce = "base64_96bit_nonce";

        // Act
        var entry = VaultEntry.Create(userId, ciphertext, nonce, folderId, schemaVersion: 1);

        // Assert
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(userId, entry.UserId);
        Assert.Equal(folderId, entry.FolderId);
        Assert.Equal(ciphertext, entry.Ciphertext);
        Assert.Equal(nonce, entry.Nonce);
        Assert.Equal(1, entry.SchemaVersion);
        Assert.False(entry.IsDeleted);
        Assert.Null(entry.DeletedAt);
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtAndIsDeletedTrue()
    {
        // Arrange
        var entry = VaultEntry.Create(Guid.NewGuid(), "cipher", "nonce");

        // Act
        entry.SoftDelete();

        // Assert
        Assert.True(entry.IsDeleted);
        Assert.NotNull(entry.DeletedAt);
    }

    [Fact]
    public void Restore_ShouldClearDeletedAt()
    {
        // Arrange
        var entry = VaultEntry.Create(Guid.NewGuid(), "cipher", "nonce");
        entry.SoftDelete();
        Assert.True(entry.IsDeleted);

        // Act
        entry.Restore();

        // Assert
        Assert.False(entry.IsDeleted);
        Assert.Null(entry.DeletedAt);
    }

    [Fact]
    public void MoveToFolder_ShouldUpdateFolderId()
    {
        // Arrange
        var entry = VaultEntry.Create(Guid.NewGuid(), "cipher", "nonce");
        var newFolderId = Guid.NewGuid();

        // Act
        entry.MoveToFolder(newFolderId);

        // Assert
        Assert.Equal(newFolderId, entry.FolderId);
    }
}
