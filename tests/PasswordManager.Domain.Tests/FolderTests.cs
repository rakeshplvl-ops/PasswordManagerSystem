using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Exceptions;
using Xunit;

namespace PasswordManager.Domain.Tests;

public class FolderTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateFolder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var encName = "base64_enc_name";
        var nonce = "base64_nonce";

        // Act
        var folder = Folder.Create(userId, encName, nonce);

        // Assert
        Assert.NotEqual(Guid.Empty, folder.Id);
        Assert.Equal(userId, folder.UserId);
        Assert.Equal(encName, folder.EncryptedName);
        Assert.Equal(nonce, folder.Nonce);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowDomainException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => Folder.Create(Guid.NewGuid(), invalidName, "nonce"));
    }

    [Fact]
    public void Update_ShouldUpdateProperties()
    {
        // Arrange
        var folder = Folder.Create(Guid.NewGuid(), "old_name", "old_nonce");

        // Act
        folder.Update("new_name", "new_nonce");

        // Assert
        Assert.Equal("new_name", folder.EncryptedName);
        Assert.Equal("new_nonce", folder.Nonce);
    }
}
