using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Domain.Entities;
using Xunit;

namespace PasswordManager.Application.Tests;

public class InterfaceContractTests
{
    private class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public void DateTimeProvider_ShouldReturnConfiguredTime()
    {
        // Arrange
        var expectedTime = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var provider = new FakeDateTimeProvider { UtcNow = expectedTime };

        // Act & Assert
        Assert.Equal(expectedTime, provider.UtcNow);
    }
}
