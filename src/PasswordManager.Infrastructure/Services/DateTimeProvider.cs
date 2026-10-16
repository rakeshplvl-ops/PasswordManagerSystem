using PasswordManager.Application.Common.Interfaces;

namespace PasswordManager.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
