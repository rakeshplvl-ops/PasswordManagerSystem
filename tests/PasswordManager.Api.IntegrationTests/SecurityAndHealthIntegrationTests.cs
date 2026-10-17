using System.Net;
using System.Net.Http.Json;
using PasswordManager.Application.Features.Auth.Dtos;
using PasswordManager.Application.Features.VaultEntries.Dtos;
using Xunit;

namespace PasswordManager.Api.IntegrationTests;

public class SecurityAndHealthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SecurityAndHealthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_Returns200_AndHealthyStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task ApiHealthEndpoint_Returns200_AndHealthyStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task StateChangingRequest_WithSessionCookie_MissingCsrfHeader_Returns403Forbidden()
    {
        var client = _factory.CreateClient();
        var email = $"csrf_test_{Guid.NewGuid():N}@example.com";

        // Register user to obtain session cookie
        var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "P@ssw0rdEnterprise!"));
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        var cookieHeader = regResponse.Headers.GetValues("Set-Cookie").First(c => c.Contains("pm_session="));
        var rawCookie = cookieHeader.Split(';')[0];

        // Craft a POST request using a fresh HttpClient that deliberately strips all headers
        using var rawClient = _factory.CreateDefaultClient();
        rawClient.DefaultRequestHeaders.Clear();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/vault/entries")
        {
            Content = JsonContent.Create(new CreateVaultEntryRequest("test==", "nonce==", null, 1))
        };
        request.Headers.Add("Cookie", rawCookie);
        // Notice: No X-Requested-With or X-CSRF-Protection header added!

        var response = await rawClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("CSRF validation failed", body);
    }

    [Fact]
    public async Task StateChangingRequest_WithSessionCookie_WithCsrfHeader_Succeeds()
    {
        var client = _factory.CreateClient();
        var email = $"csrf_valid_{Guid.NewGuid():N}@example.com";

        var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "P@ssw0rdEnterprise!"));
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        var cookieHeader = regResponse.Headers.GetValues("Set-Cookie").First(c => c.Contains("pm_session="));
        var rawCookie = cookieHeader.Split(';')[0];

        using var rawClient = _factory.CreateDefaultClient();
        rawClient.DefaultRequestHeaders.Clear();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/vault/entries")
        {
            Content = JsonContent.Create(new CreateVaultEntryRequest("test==", "nonce==", null, 1))
        };
        request.Headers.Add("Cookie", rawCookie);
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");

        var response = await rawClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
