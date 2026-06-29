using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();
        await EnsureAdminUserManagementColumnsAsync(context);

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in RoleConstants.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = configuration["SeedAdmin:Email"] ?? "admin@voltguard.local";
        var adminPassword = configuration["SeedAdmin:Password"] ?? "Admin@12345";
        var adminFullName = configuration["SeedAdmin:FullName"] ?? "VoltGuard Admin";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminFullName,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);
        }

        await DevDataSeeder.SeedAsync(serviceProvider);
    }

    private static async Task EnsureAdminUserManagementColumnsAsync(AppDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('AspNetUsers', 'LastLoginAtUtc') IS NULL
            BEGIN
                ALTER TABLE [AspNetUsers] ADD [LastLoginAtUtc] datetime2 NULL;
            END
            """);

        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('AspNetUsers', 'UpdatedAtUtc') IS NULL
            BEGIN
                ALTER TABLE [AspNetUsers] ADD [UpdatedAtUtc] datetime2 NULL;
            END
            """);
    }
}
