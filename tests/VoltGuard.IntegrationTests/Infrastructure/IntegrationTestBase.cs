using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using VoltGuard.Application.DTOs.Auth;

namespace VoltGuard.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<VoltGuardApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected IntegrationTestBase(VoltGuardApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    protected VoltGuardApiFactory Factory { get; }
    protected HttpClient Client { get; }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    protected async Task<AuthResponseDto> LoginAsync(
        string email = "admin@voltguard.local",
        string password = "Admin@12345")
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions))!;
    }

    protected async Task AuthenticateAsync(
        string email = "admin@voltguard.local",
        string password = "Admin@12345")
    {
        var auth = await LoginAsync(email, password);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            auth.TokenType,
            auth.AccessToken);
    }

    protected static async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
    {
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }
}
