using System.Net;
using System.Net.Http.Json;
using VoltGuard.Application.DTOs.Auth;
using VoltGuard.IntegrationTests.Infrastructure;

namespace VoltGuard.IntegrationTests.Auth;

public class AuthApiTests : IntegrationTestBase
{
    public AuthApiTests(VoltGuardApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsBearerTokenAndRoles()
    {
        var auth = await LoginAsync();

        Assert.Equal("Bearer", auth.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.Equal("admin@voltguard.local", auth.Email);
        Assert.Contains("Admin", auth.Roles);
        Assert.True(auth.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@voltguard.local",
            password = "Wrong@12345"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUser()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync("/api/auth/me");

        response.EnsureSuccessStatusCode();
        var user = await ReadAsAsync<CurrentUserDto>(response);
        Assert.Equal("admin@voltguard.local", user.Email);
        Assert.Contains("Admin", user.Roles);
    }

    [Fact]
    public async Task AdminOnly_WithEngineerToken_ReturnsForbidden()
    {
        await AuthenticateAsync("engineer@voltguard.local", "Engineer@12345");

        var response = await Client.GetAsync("/api/auth/admin-only");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
