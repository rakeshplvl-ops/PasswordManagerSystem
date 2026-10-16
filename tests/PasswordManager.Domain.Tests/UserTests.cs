using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Exceptions;
using Xunit;

namespace PasswordManager.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidEmailAndPassword_ShouldCreateUser()
    {
        // Arrange
        var email = "user@example.com";
        var passwordHash = "argon2id_hash_12345";

        // Act
        var user = User.Create(email, passwordHash);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.True(user.IsActive);
        Assert.Null(user.LastLoginAt);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("missing-at-domain.com")]
    public void Create_WithInvalidEmail_ShouldThrowDomainException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => User.Create(invalidEmail, "hash123"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPasswordHash_ShouldThrowDomainException(string invalidHash)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => User.Create("valid@example.com", invalidHash));
    }

    [Fact]
    public void UpdatePasswordHash_WithValidHash_ShouldUpdateHashAndTimestamp()
    {
        // Arrange
        var user = User.Create("user@example.com", "old_hash");
        var newHash = "new_argon2id_hash";

        // Act
        user.UpdatePasswordHash(newHash);

        // Assert
        Assert.Equal(newHash, user.PasswordHash);
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var user = User.Create("user@example.com", "hash");
        var loginTime = DateTime.UtcNow;

        // Act
        user.RecordLogin(loginTime);

        // Assert
        Assert.Equal(loginTime, user.LastLoginAt);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create("user@example.com", "hash");

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }
}
