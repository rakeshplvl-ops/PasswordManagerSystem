using System.Net;
using System.Net.Http.Json;
using PasswordManager.Application.Features.Auth.Dtos;
using PasswordManager.Application.Features.VaultEntries.Dtos;
using Xunit;

namespace PasswordManager.Api.IntegrationTests;

public class VaultEntriesEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VaultEntriesEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient Client, string CookieHeader)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var regResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(email, "P@ssw0rdEnterprise!"));
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);

        var cookieHeader = regResponse.Headers.GetValues("Set-Cookie").First(c => c.Contains("pm_session="));
        var rawCookie = cookieHeader.Split(';')[0];
        return (client, rawCookie);
    }

    [Fact]
    public async Task GetEntries_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/vault/entries");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEntry_And_Lifecycle_Succeeds()
    {
        var (client, cookie) = await CreateAuthenticatedClientAsync();

        // 1. Create entry
        var createPayload = new CreateVaultEntryRequest("ciphertext123==", "nonce123==", null, 1);
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/vault/entries")
        {
            Content = JsonContent.Create(createPayload)
        };
        postRequest.Headers.Add("Cookie", cookie);

        var postResponse = await client.SendAsync(postRequest);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var created = await postResponse.Content.ReadFromJsonAsync<VaultEntryDto>();
        Assert.NotNull(created);
        Assert.Equal("ciphertext123==", created.Ciphertext);

        // 2. GET by ID
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/vault/entries/{created.Id}");
        getRequest.Headers.Add("Cookie", cookie);

        var getResponse = await client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // 3. Update entry
        var updatePayload = new UpdateVaultEntryRequest("updatedCipher==", "updatedNonce==", null, 1);
        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/vault/entries/{created.Id}")
        {
            Content = JsonContent.Create(updatePayload)
        };
        putRequest.Headers.Add("Cookie", cookie);

        var putResponse = await client.SendAsync(putRequest);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<VaultEntryDto>();
        Assert.NotNull(updated);
        Assert.Equal("updatedCipher==", updated.Ciphertext);

        // 4. Soft Delete
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/vault/entries/{created.Id}");
        deleteRequest.Headers.Add("Cookie", cookie);

        var deleteResponse = await client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 5. Active list should be empty
        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/vault/entries");
        listRequest.Headers.Add("Cookie", cookie);

        var listResponse = await client.SendAsync(listRequest);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<VaultEntryDto>>();
        Assert.NotNull(list);
        Assert.Empty(list);

        // 6. Include deleted list should have 1 item
        var deletedListRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/vault/entries?includeDeleted=true");
        deletedListRequest.Headers.Add("Cookie", cookie);

        var deletedListResponse = await client.SendAsync(deletedListRequest);
        var deletedList = await deletedListResponse.Content.ReadFromJsonAsync<List<VaultEntryDto>>();
        Assert.NotNull(deletedList);
        Assert.Single(deletedList);

        // 7. Restore
        var restoreRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/vault/entries/{created.Id}/restore");
        restoreRequest.Headers.Add("Cookie", cookie);

        var restoreResponse = await client.SendAsync(restoreRequest);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        // 8. Purge (hard delete)
        var purgeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/vault/entries/{created.Id}/purge");
        purgeRequest.Headers.Add("Cookie", cookie);

        var purgeResponse = await client.SendAsync(purgeRequest);
        Assert.Equal(HttpStatusCode.NoContent, purgeResponse.StatusCode);
    }

    [Fact]
    public async Task TenantIsolation_UserCannotAccessAnotherUsersEntry()
    {
        var (clientA, cookieA) = await CreateAuthenticatedClientAsync();
        var (clientB, cookieB) = await CreateAuthenticatedClientAsync();

        // User A creates entry
        var createPayload = new CreateVaultEntryRequest("userASecret==", "nonceA==", null, 1);
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/vault/entries")
        {
            Content = JsonContent.Create(createPayload)
        };
        postRequest.Headers.Add("Cookie", cookieA);

        var postResponse = await clientA.SendAsync(postRequest);
        var entryA = await postResponse.Content.ReadFromJsonAsync<VaultEntryDto>();
        Assert.NotNull(entryA);

        // User B attempts to access User A's entry
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/vault/entries/{entryA.Id}");
        getRequest.Headers.Add("Cookie", cookieB);

        var getResponse = await clientB.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
