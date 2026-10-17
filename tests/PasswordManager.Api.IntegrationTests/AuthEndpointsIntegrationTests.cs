using System.Net;
using System.Net.Http.Json;
using PasswordManager.Application.Features.Auth.Dtos;
using Xunit;

namespace PasswordManager.Api.IntegrationTests;

public class AuthEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithValidData_Returns201AndSetsSessionCookie()
    {
        // Arrange
        var client = _factory.CreateClient();
        var uniqueEmail = $"reg_{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest(uniqueEmail, "P@ssw0rdEnterprise!");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotEqual(Guid.Empty, authResponse.UserId);
        Assert.Equal(uniqueEmail, authResponse.Email);

        Assert.True(response.Headers.Contains("Set-Cookie"));
        var cookies = response.Headers.GetValues("Set-Cookie");
        Assert.Contains(cookies, c => c.Contains("pm_session="));
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndSetsSessionCookie()
    {
        // Arrange
        var client = _factory.CreateClient();
        var uniqueEmail = $"login_{Guid.NewGuid():N}@example.com";
        var password = "Secure#Login123!";
        var registerRequest = new RegisterRequest(uniqueEmail, password);

        var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        // Act
        var loginRequest = new LoginRequest(uniqueEmail, password);
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.Equal(uniqueEmail, authResponse.Email);

        Assert.True(loginResponse.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task GetCurrentUser_WithoutSession_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndToEnd_Register_GetCurrentUser_Logout_VerifyUnauthorized()
    {
        // Arrange
        // Using CreateClientWithOptions to preserve cookies across requests
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var email = $"e2e_{Guid.NewGuid():N}@example.com";
        var password = "Enterprise#Password2026!";
        var regRequest = new RegisterRequest(email, password);

        // 1. Register
        var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", regRequest);
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        // 2. Call /me with preserved cookie
        var meResponse = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var userProfile = await meResponse.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.NotNull(userProfile);
        Assert.Equal(email, userProfile.Email);

        // 3. Logout
        var logoutResponse = await client.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        // 4. Call /me again after logout -> should be 401 Unauthorized
        var meAfterLogout = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409Conflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"dup_{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest(email, "P@ssw0rd12345!");

        // First registration
        var first = await client.PostAsJsonAsync("/api/v1/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // Second registration with same email
        var second = await client.PostAsJsonAsync("/api/v1/auth/register", request);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = $"badpass_{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "Valid#Password123!"));

        // Act
        var badLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "WrongPassword123!"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, badLogin.StatusCode);
    }
}
