using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PasswordManager.Application.Common.Interfaces;

namespace PasswordManager.Api.Authentication;

public class SessionAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "Session";
    public const string CookieName = "pm_session";
}

public class SessionAuthenticationHandler : AuthenticationHandler<SessionAuthenticationOptions>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionTokenService _sessionTokenService;

    public SessionAuthenticationHandler(
        IOptionsMonitor<SessionAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISessionRepository sessionRepository,
        ISessionTokenService sessionTokenService)
        : base(options, logger, encoder)
    {
        _sessionRepository = sessionRepository;
        _sessionTokenService = sessionTokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? rawToken = null;

        // 1. Check HttpOnly cookie
        if (Request.Cookies.TryGetValue(SessionAuthenticationOptions.CookieName, out var cookieToken) &&
            !string.IsNullOrWhiteSpace(cookieToken))
        {
            rawToken = cookieToken;
        }

        // 2. Alternatively check Authorization: Bearer <token>
        if (string.IsNullOrWhiteSpace(rawToken) &&
            Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                rawToken = headerValue.Substring("Bearer ".Length).Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return AuthenticateResult.NoResult();
        }

        var tokenHash = _sessionTokenService.HashToken(rawToken);
        var session = await _sessionRepository.GetByTokenHashAsync(tokenHash, Context.RequestAborted);

        if (session == null || !session.IsActive)
        {
            return AuthenticateResult.Fail("Session has expired or has been revoked.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new(ClaimTypes.Email, session.User?.Email ?? string.Empty),
            new("session_id", session.Id.ToString()),
            new(ClaimTypes.AuthenticationMethod, SessionAuthenticationOptions.SchemeName)
        };

        var identity = new ClaimsIdentity(claims, SessionAuthenticationOptions.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SessionAuthenticationOptions.SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
