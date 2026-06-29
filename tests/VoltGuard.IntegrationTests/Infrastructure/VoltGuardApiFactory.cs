using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;
using VoltGuard.Infrastructure.Data;

namespace VoltGuard.IntegrationTests.Infrastructure;

public sealed class VoltGuardApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"VoltGuardIntegrationTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:InitializeOnStartup"] = "false",
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=UnusedByIntegrationTests;Trusted_Connection=True",
                ["Jwt:Issuer"] = "VoltGuard.FieldLink",
                ["Jwt:Audience"] = "VoltGuard.FieldLink.Client",
                ["Jwt:Key"] = "CHANGE_THIS_DEV_SECRET_KEY_1234567890_32_BYTES_MINIMUM",
                ["Jwt:ExpiryMinutes"] = "120"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in RoleConstants.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        await CreateUserAsync(
            scope.ServiceProvider,
            "admin@voltguard.local",
            "Admin User",
            "Admin@12345",
            RoleConstants.Admin);

        await CreateUserAsync(
            scope.ServiceProvider,
            "engineer@voltguard.local",
            "Engineer User",
            "Engineer@12345",
            RoleConstants.Engineer);
    }

    private static async Task CreateUserAsync(
        IServiceProvider serviceProvider,
        string email,
        string fullName,
        string password,
        string role)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Failed to create integration test user {email}: {errors}");
        }

        await userManager.AddToRoleAsync(user, role);
    }
}
