using System.Security.Cryptography;
using System.Text;
using PasswordManager.Application.Common.Interfaces;

namespace PasswordManager.Infrastructure.Security;

public class SessionTokenService : ISessionTokenService
{
    private const int TokenByteLength = 64;

    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(TokenByteLength);
        // Base64Url encoding for clean cookie / HTTP header transport
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
