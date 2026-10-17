using System.Net;
using System.Net.Http.Json;
using PasswordManager.Application.Features.Auth.Dtos;
using PasswordManager.Application.Features.Folders.Dtos;
using Xunit;

namespace PasswordManager.Api.IntegrationTests;

public class FoldersEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FoldersEndpointsIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task GetFolders_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/vault/folders");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateFolder_And_Lifecycle_Succeeds()
    {
        var (client, cookie) = await CreateAuthenticatedClientAsync();

        // 1. Create folder
        var createPayload = new CreateFolderRequest("encryptedFolderName==", "folderNonce==");
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/vault/folders")
        {
            Content = JsonContent.Create(createPayload)
        };
        postRequest.Headers.Add("Cookie", cookie);

        var postResponse = await client.SendAsync(postRequest);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var folder = await postResponse.Content.ReadFromJsonAsync<FolderDto>();
        Assert.NotNull(folder);
        Assert.Equal("encryptedFolderName==", folder.EncryptedName);

        // 2. GET all folders
        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/vault/folders");
        listRequest.Headers.Add("Cookie", cookie);

        var listResponse = await client.SendAsync(listRequest);
        var folders = await listResponse.Content.ReadFromJsonAsync<List<FolderDto>>();
        Assert.NotNull(folders);
        Assert.Single(folders);

        // 3. Update folder
        var updatePayload = new UpdateFolderRequest("renamedEncrypted==", "newNonce==");
        var putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/vault/folders/{folder.Id}")
        {
            Content = JsonContent.Create(updatePayload)
        };
        putRequest.Headers.Add("Cookie", cookie);

        var putResponse = await client.SendAsync(putRequest);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<FolderDto>();
        Assert.NotNull(updated);
        Assert.Equal("renamedEncrypted==", updated.EncryptedName);

        // 4. Delete folder
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/vault/folders/{folder.Id}");
        deleteRequest.Headers.Add("Cookie", cookie);

        var deleteResponse = await client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 5. Verify deleted
        var verifyRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/vault/folders/{folder.Id}");
        verifyRequest.Headers.Add("Cookie", cookie);

        var verifyResponse = await client.SendAsync(verifyRequest);
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }
}
