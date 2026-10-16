using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Exceptions;
using Xunit;

namespace PasswordManager.Domain.Tests;

public class AuthSessionTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateActiveSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "sha256_session_token_hash";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var session = AuthSession.Create(userId, tokenHash, expiresAt, "127.0.0.1", "Chrome");

        // Assert
        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(tokenHash, session.TokenHash);
        Assert.Equal(expiresAt, session.ExpiresAt);
        Assert.Equal("127.0.0.1", session.IpAddress);
        Assert.Equal("Chrome", session.UserAgent);
        Assert.True(session.IsActive);
        Assert.Null(session.RevokedAt);
    }

    [Fact]
    public void Create_WithPastExpiry_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => AuthSession.Create(
            Guid.NewGuid(),
            "tokenHash",
            DateTime.UtcNow.AddMinutes(-5)
        ));
    }

    [Fact]
    public void Revoke_ShouldDeactivateSession()
    {
        // Arrange
        var session = AuthSession.Create(Guid.NewGuid(), "tokenHash", DateTime.UtcNow.AddHours(2));
        Assert.True(session.IsActive);

        // Act
        session.Revoke();

        // Assert
        Assert.False(session.IsActive);
        Assert.NotNull(session.RevokedAt);
    }
}
