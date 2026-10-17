namespace PasswordManager.Application.Common.Interfaces;

public interface ISessionTokenService
{
    string GenerateToken();
    string HashToken(string token);
}
