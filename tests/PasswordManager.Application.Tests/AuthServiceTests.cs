using FluentValidation;
using PasswordManager.Application.Common.Exceptions;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Application.Features.Auth;
using PasswordManager.Application.Features.Auth.Dtos;
using PasswordManager.Application.Features.Auth.Validators;
using PasswordManager.Domain.Entities;
using Xunit;

namespace PasswordManager.Application.Tests;

public class AuthServiceTests
{
    private class FakeUserRepository : IUserRepository
    {
        public readonly List<User> Users = new();

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public void Update(User user)
        {
            var index = Users.FindIndex(u => u.Id == user.Id);
            if (index >= 0) Users[index] = user;
        }
    }

    private class FakeVaultSettingsRepository : IVaultSettingsRepository
    {
        public readonly List<VaultSettings> Settings = new();

        public Task<VaultSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Settings.FirstOrDefault(s => s.UserId == userId));

        public Task AddAsync(VaultSettings settings, CancellationToken cancellationToken = default)
        {
            Settings.Add(settings);
            return Task.CompletedTask;
        }

        public void Update(VaultSettings settings)
        {
            var index = Settings.FindIndex(s => s.Id == settings.Id);
            if (index >= 0) Settings[index] = settings;
        }
    }

    private class FakeSessionRepository : ISessionRepository
    {
        public readonly List<AuthSession> Sessions = new();

        public Task<AuthSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(Sessions.FirstOrDefault(s => s.TokenHash == tokenHash));

        public Task<IReadOnlyList<AuthSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AuthSession>>(Sessions.Where(s => s.UserId == userId && s.IsActive).ToList());

        public Task AddAsync(AuthSession session, CancellationToken cancellationToken = default)
        {
            Sessions.Add(session);
            return Task.CompletedTask;
        }

        public void Update(AuthSession session)
        {
            var index = Sessions.FindIndex(s => s.Id == session.Id);
            if (index >= 0) Sessions[index] = session;
        }

        public Task RevokeSessionAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            var session = Sessions.FirstOrDefault(s => s.TokenHash == tokenHash);
            session?.Revoke();
            return Task.CompletedTask;
        }

        public Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            foreach (var session in Sessions.Where(s => s.UserId == userId))
            {
                session.Revoke();
            }
            return Task.CompletedTask;
        }

        public Task PurgeExpiredSessionsAsync(DateTime threshold, CancellationToken cancellationToken = default)
        {
            Sessions.RemoveAll(s => s.ExpiresAt < threshold);
            return Task.CompletedTask;
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hashed_{password}";
        public bool VerifyPassword(string password, string passwordHash) => passwordHash == $"hashed_{password}";
    }

    private class FakeSessionTokenService : ISessionTokenService
    {
        public string GenerateToken() => "fake_random_session_token_12345";
        public string HashToken(string token) => $"hash_{token}";
    }

    private class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    }

    private readonly FakeUserRepository _userRepo = new();
    private readonly FakeVaultSettingsRepository _vaultSettingsRepo = new();
    private readonly FakeSessionRepository _sessionRepo = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeSessionTokenService _tokenService = new();
    private readonly FakeDateTimeProvider _dateTime = new();
    private readonly IValidator<RegisterRequest> _registerValidator = new RegisterRequestValidator();
    private readonly IValidator<LoginRequest> _loginValidator = new LoginRequestValidator();

    private AuthService CreateService()
    {
        return new AuthService(
            _userRepo,
            _vaultSettingsRepo,
            _sessionRepo,
            _uow,
            _hasher,
            _tokenService,
            _dateTime,
            _registerValidator,
            _loginValidator
        );
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserAndSession()
    {
        // Arrange
        var service = CreateService();
        var request = new RegisterRequest("alice@example.com", "P@ssword123!");

        // Act
        var (response, rawToken) = await service.RegisterAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.NotNull(response);
        Assert.Equal("alice@example.com", response.Email);
        Assert.Equal("fake_random_session_token_12345", rawToken);

        Assert.Single(_userRepo.Users);
        Assert.Single(_sessionRepo.Sessions);
        Assert.Equal(1, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task RegisterAsync_WithVaultSettings_ShouldStoreInitialVaultSettings()
    {
        // Arrange
        var service = CreateService();
        var vaultSettings = new InitialVaultSettingsDto(
            EncryptedVek: "enc_vek_base64",
            VekNonce: "nonce_base64",
            KdfAlgorithm: "Argon2id",
            KdfIterations: 3,
            KdfMemoryKb: 65536,
            KdfParallelism: 4,
            KdfSalt: "salt_base64"
        );
        var request = new RegisterRequest("bob@example.com", "Secure#P@ss123", vaultSettings);

        // Act
        var (response, _) = await service.RegisterAsync(request, "127.0.0.1", "TestAgent");

        // Assert
        Assert.Single(_vaultSettingsRepo.Settings);
        var storedSettings = _vaultSettingsRepo.Settings[0];
        Assert.Equal(response.UserId, storedSettings.UserId);
        Assert.Equal("enc_vek_base64", storedSettings.EncryptedVek);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowDuplicateEmailException()
    {
        // Arrange
        var service = CreateService();
        _userRepo.Users.Add(User.Create("charlie@example.com", "hash"));

        var request = new RegisterRequest("charlie@example.com", "P@ssword123!");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateEmailException>(() =>
            service.RegisterAsync(request, null, null));
    }

    [Fact]
    public async Task RegisterAsync_WithWeakPassword_ShouldThrowValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new RegisterRequest("weak@example.com", "weak");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.RegisterAsync(request, null, null));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldAuthenticateAndIssueSession()
    {
        // Arrange
        var service = CreateService();
        var user = User.Create("user@example.com", _hasher.HashPassword("P@ssword123!"));
        _userRepo.Users.Add(user);

        var request = new LoginRequest("user@example.com", "P@ssword123!");

        // Act
        var (response, token) = await service.LoginAsync(request, "127.0.0.1", "Browser");

        // Assert
        Assert.Equal(user.Id, response.UserId);
        Assert.Equal("fake_random_session_token_12345", token);
        Assert.Single(_sessionRepo.Sessions);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var service = CreateService();
        var user = User.Create("user@example.com", _hasher.HashPassword("P@ssword123!"));
        _userRepo.Users.Add(user);

        var request = new LoginRequest("user@example.com", "WrongPassword!");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            service.LoginAsync(request, null, null));
    }

    [Fact]
    public async Task LoginAsync_WithDeactivatedUser_ShouldThrowAccountDeactivatedException()
    {
        // Arrange
        var service = CreateService();
        var user = User.Create("user@example.com", _hasher.HashPassword("P@ssword123!"));
        user.Deactivate();
        _userRepo.Users.Add(user);

        var request = new LoginRequest("user@example.com", "P@ssword123!");

        // Act & Assert
        await Assert.ThrowsAsync<AccountDeactivatedException>(() =>
            service.LoginAsync(request, null, null));
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeSession()
    {
        // Arrange
        var service = CreateService();
        var rawToken = "raw_token_xyz";
        var tokenHash = _tokenService.HashToken(rawToken);
        var session = AuthSession.Create(Guid.NewGuid(), tokenHash, _dateTime.UtcNow.AddDays(7), createdAt: _dateTime.UtcNow);
        _sessionRepo.Sessions.Add(session);

        // Act
        await service.LogoutAsync(rawToken);

        // Assert
        Assert.False(session.IsActive);
        Assert.NotNull(session.RevokedAt);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnUserProfile()
    {
        // Arrange
        var service = CreateService();
        var user = User.Create("profile@example.com", "hash");
        _userRepo.Users.Add(user);

        // Act
        var profile = await service.GetCurrentUserAsync(user.Id);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(user.Id, profile.Id);
        Assert.Equal(user.Email, profile.Email);
        Assert.False(profile.HasVaultSettings);
    }
}
