using System.Net;
using System.Net.Http.Json;
using PasswordManager.Application.Features.Auth.Dtos;
using PasswordManager.Application.Features.VaultSettings.Dtos;
using Xunit;

namespace PasswordManager.Api.IntegrationTests;

public class VaultSettingsEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VaultSettingsEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSettings_WhenUnauthenticated_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/vault/settings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_WhenNoSettingsExist_Returns404NotFound()
    {
        var client = _factory.CreateClient();
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "P@ssw0rdEnterprise!"));
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        // Client retains the pm_session cookie automatically via CookieContainer in WebApplicationFactory client if handler preserves it,
        // or we pass the cookie header explicitly. Let's extract pm_session cookie from response:
        var cookieHeader = regResponse.Headers.GetValues("Set-Cookie").First(c => c.Contains("pm_session="));
        var rawCookie = cookieHeader.Split(';')[0];

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/vault/settings");
        request.Headers.Add("Cookie", rawCookie);

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutSettings_AndThenGetSettings_ReturnsUpdatedSettings()
    {
        var client = _factory.CreateClient();
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "P@ssw0rdEnterprise!"));
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        var cookieHeader = regResponse.Headers.GetValues("Set-Cookie").First(c => c.Contains("pm_session="));
        var rawCookie = cookieHeader.Split(';')[0];

        var putPayload = new UpdateVaultSettingsRequest(
            "encVekBase64==",
            "vekNonceBase64==",
            "Argon2id",
            3,
            65536,
            4,
            "saltBase64==",
            1
        );

        var putRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/vault/settings")
        {
            Content = JsonContent.Create(putPayload)
        };
        putRequest.Headers.Add("Cookie", rawCookie);

        var putResponse = await client.SendAsync(putRequest);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var putResult = await putResponse.Content.ReadFromJsonAsync<VaultSettingsDto>();
        Assert.NotNull(putResult);
        Assert.Equal("encVekBase64==", putResult.EncryptedVek);
        Assert.Equal("vekNonceBase64==", putResult.VekNonce);
        Assert.Equal(3, putResult.KdfIterations);

        // Now GET settings
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/vault/settings");
        getRequest.Headers.Add("Cookie", rawCookie);

        var getResponse = await client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getResult = await getResponse.Content.ReadFromJsonAsync<VaultSettingsDto>();
        Assert.NotNull(getResult);
        Assert.Equal("encVekBase64==", getResult.EncryptedVek);
        Assert.Equal("saltBase64==", getResult.KdfSalt);
    }

    [Fact]
    public async Task Register_WithInitialVaultSettings_CanImmediatelyRetrieveSettings()
    {
        var client = _factory.CreateClient();
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var initialSettings = new InitialVaultSettingsDto(
            "initialEncVek==",
            "initialNonce==",
            "Argon2id",
            3,
            65536,
            4,
            "initialSalt==",
            1
        );

        var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "P@ssw0rdEnterprise!", initialSettings));
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        var cookieHeader = regResponse.Headers.GetValues("Set-Cookie").First(c => c.Contains("pm_session="));
        var rawCookie = cookieHeader.Split(';')[0];

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/vault/settings");
        getRequest.Headers.Add("Cookie", rawCookie);

        var getResponse = await client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getResult = await getResponse.Content.ReadFromJsonAsync<VaultSettingsDto>();
        Assert.NotNull(getResult);
        Assert.Equal("initialEncVek==", getResult.EncryptedVek);
        Assert.Equal("initialNonce==", getResult.VekNonce);
        Assert.Equal("initialSalt==", getResult.KdfSalt);
    }
}
