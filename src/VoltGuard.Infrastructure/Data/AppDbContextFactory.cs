using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VoltGuard.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var possibleApiPaths = new[]
        {
            Path.GetFullPath(Path.Combine(currentDirectory, "src", "VoltGuard.Api")),
            Path.GetFullPath(Path.Combine(currentDirectory, "..", "VoltGuard.Api")),
            Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", "src", "VoltGuard.Api"))
        };

        var apiPath = possibleApiPaths.FirstOrDefault(Directory.Exists);

        if (apiPath is null)
        {
            throw new InvalidOperationException("Could not find VoltGuard.Api folder for appsettings.json.");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is missing.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
