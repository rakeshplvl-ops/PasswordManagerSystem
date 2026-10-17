using FluentValidation;
using PasswordManager.Application.Common.Exceptions;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Application.Features.Auth.Dtos;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IVaultSettingsRepository _vaultSettingsRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionTokenService _sessionTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    private static readonly TimeSpan SessionDuration = TimeSpan.FromDays(7);

    public AuthService(
        IUserRepository userRepository,
        IVaultSettingsRepository vaultSettingsRepository,
        ISessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ISessionTokenService sessionTokenService,
        IDateTimeProvider dateTimeProvider,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userRepository = userRepository;
        _vaultSettingsRepository = vaultSettingsRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _sessionTokenService = sessionTokenService;
        _dateTimeProvider = dateTimeProvider;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<(AuthResponse Response, string RawSessionToken)> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new DuplicateEmailException(normalizedEmail);
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = User.Create(normalizedEmail, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);

        if (request.VaultSettings != null)
        {
            var vs = request.VaultSettings;
            var settings = VaultSettings.Create(
                user.Id,
                vs.EncryptedVek,
                vs.VekNonce,
                vs.KdfAlgorithm,
                vs.KdfIterations,
                vs.KdfMemoryKb,
                vs.KdfParallelism,
                vs.KdfSalt,
                vs.SchemaVersion
            );
            await _vaultSettingsRepository.AddAsync(settings, cancellationToken);
        }

        var rawToken = _sessionTokenService.GenerateToken();
        var tokenHash = _sessionTokenService.HashToken(rawToken);
        var now = _dateTimeProvider.UtcNow;
        var expiresAt = now.Add(SessionDuration);

        var session = AuthSession.Create(user.Id, tokenHash, expiresAt, ipAddress, userAgent, now);
        await _sessionRepository.AddAsync(session, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (new AuthResponse(user.Id, user.Email), rawToken);
    }

    public async Task<(AuthResponse Response, string RawSessionToken)> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user == null)
        {
            throw new InvalidCredentialsException();
        }

        var passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
        {
            throw new AccountDeactivatedException();
        }

        user.RecordLogin(_dateTimeProvider.UtcNow);
        _userRepository.Update(user);

        var rawToken = _sessionTokenService.GenerateToken();
        var tokenHash = _sessionTokenService.HashToken(rawToken);
        var now = _dateTimeProvider.UtcNow;
        var expiresAt = now.Add(SessionDuration);

        var session = AuthSession.Create(user.Id, tokenHash, expiresAt, ipAddress, userAgent, now);
        await _sessionRepository.AddAsync(session, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (new AuthResponse(user.Id, user.Email), rawToken);
    }

    public async Task LogoutAsync(
        string rawSessionToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawSessionToken))
        {
            return;
        }

        var tokenHash = _sessionTokenService.HashToken(rawSessionToken);
        await _sessionRepository.RevokeSessionAsync(tokenHash, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }

        var settings = await _vaultSettingsRepository.GetByUserIdAsync(userId, cancellationToken);

        return new CurrentUserResponse(
            user.Id,
            user.Email,
            user.CreatedAt,
            user.LastLoginAt,
            HasVaultSettings: settings != null
        );
    }
}
