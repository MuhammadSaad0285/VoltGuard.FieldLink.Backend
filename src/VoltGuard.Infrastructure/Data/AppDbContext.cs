using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<ThresholdRule> ThresholdRules => Set<ThresholdRule>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<AiFeedback> AiFeedbacks => Set<AiFeedback>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
