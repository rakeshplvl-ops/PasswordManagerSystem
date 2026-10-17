using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using PasswordManager.Application.Common.Interfaces;

namespace PasswordManager.Infrastructure.Security;

public class Argon2idPasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 3;
    private const int MemorySizeKb = 65536; // 64 MB
    private const int Parallelism = 4;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            MemorySize = MemorySizeKb,
            Iterations = Iterations
        };

        var hash = argon2.GetBytes(HashSize);

        var saltBase64 = Convert.ToBase64String(salt);
        var hashBase64 = Convert.ToBase64String(hash);

        // Standard modular crypt format
        return $"$argon2id$v=19$m={MemorySizeKb},t={Iterations},p={Parallelism}${saltBase64}${hashBase64}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            var parts = passwordHash.Split('$');
            // Expected format: ["", "argon2id", "v=19", "m=...,t=...,p=...", salt, hash]
            if (parts.Length != 6 || parts[1] != "argon2id")
            {
                return false;
            }

            var paramParts = parts[3].Split(',');
            int memory = MemorySizeKb;
            int iterations = Iterations;
            int parallelism = Parallelism;

            foreach (var part in paramParts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2)
                {
                    if (kv[0] == "m") memory = int.Parse(kv[1]);
                    else if (kv[0] == "t") iterations = int.Parse(kv[1]);
                    else if (kv[0] == "p") parallelism = int.Parse(kv[1]);
                }
            }

            var salt = Convert.FromBase64String(parts[4]);
            var expectedHash = Convert.FromBase64String(parts[5]);

            var passwordBytes = Encoding.UTF8.GetBytes(password);
            using var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                DegreeOfParallelism = parallelism,
                MemorySize = memory,
                Iterations = iterations
            };

            var actualHash = argon2.GetBytes(expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }
}
