using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Infrastructure.Data;

public static class DevDataSeeder
{
    private const string DemoCustomerName = "VoltGuard Demo Customer";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var seedDemoValue = configuration["DemoData:SeedDemoData"];

        if (!string.Equals(seedDemoValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var testEvaluationService = serviceProvider.GetRequiredService<ITestEvaluationService>();
        var assetRiskService = serviceProvider.GetRequiredService<IAssetRiskService>();

        // Idempotent safety check.
        // If demo customer already exists, do not insert duplicate demo data.
        var demoAlreadyExists = await context.Customers
            .AnyAsync(x => x.Name == DemoCustomerName);

        if (demoAlreadyExists)
        {
            return;
        }

        await SeedEngineerUserAsync(userManager);

        var now = DateTime.UtcNow;

        var customer = new Customer
        {
            Name = DemoCustomerName,
            ContactPerson = "John Smith",
            ContactEmail = "john.smith@voltguard-demo.local",
            ContactPhone = "07123456789",
            AddressLine1 = "Unit 5",
            AddressLine2 = "Industrial Estate",
            City = "Leicester",
            Postcode = "LE1 1AA",
            Country = "United Kingdom",
            Notes = "Demo customer used for testing and presentation.",
            IsActive = true,
            CreatedAtUtc = now
        };

        var mainSite = new Site
        {
            Customer = customer,
            Name = "Leicester Main Site",
            SiteCode = "LEI-001",
            SiteType = "Industrial",
            ContactPerson = "John Smith",
            ContactEmail = "john.smith@voltguard-demo.local",
            ContactPhone = "07123456789",
            AddressLine1 = "Unit 5",
            AddressLine2 = "Industrial Estate",
            City = "Leicester",
            Postcode = "LE1 1AA",
            Country = "United Kingdom",
            Notes = "Main electrical testing location.",
            IsActive = true,
            CreatedAtUtc = now
        };

        var warehouseSite = new Site
        {
            Customer = customer,
            Name = "Leicester Warehouse Site",
            SiteCode = "LEI-002",
            SiteType = "Warehouse",
            ContactPerson = "Sarah Ahmed",
            ContactEmail = "sarah.ahmed@voltguard-demo.local",
            ContactPhone = "07123456780",
            AddressLine1 = "Warehouse 2",
            AddressLine2 = "Distribution Park",
            City = "Leicester",
            Postcode = "LE4 5AB",
            Country = "United Kingdom",
            Notes = "Secondary demo testing location.",
            IsActive = true,
            CreatedAtUtc = now
        };

        var motorAsset = new Asset
        {
            Site = mainSite,
            Name = "Main Production Motor",
            AssetTag = "MTR-001",
            SerialNumber = "MTR-DEMO-1001",
            AssetType = "Motor",
            Manufacturer = "ABB",
            Model = "M3BP",
            LocationDescription = "Production Line A",
            RatedVoltage = 415,
            RatedCurrent = 32,
            InstalledAtUtc = now.AddYears(-3),
            NextTestDueAtUtc = now.AddMonths(12),
            RiskLevel = RiskLevelConstants.Unknown,
            Notes = "Demo asset expected to pass.",
            IsActive = true,
            CreatedAtUtc = now
        };

        var transformerAsset = new Asset
        {
            Site = mainSite,
            Name = "Site Distribution Transformer",
            AssetTag = "TRF-001",
            SerialNumber = "TRF-DEMO-2001",
            AssetType = "Transformer",
            Manufacturer = "Schneider Electric",
            Model = "Trihal",
            LocationDescription = "LV Switch Room",
            RatedVoltage = 11000,
            RatedCurrent = 250,
            InstalledAtUtc = now.AddYears(-5),
            NextTestDueAtUtc = now.AddMonths(6),
            RiskLevel = RiskLevelConstants.Unknown,
            Notes = "Demo asset expected to show warning.",
            IsActive = true,
            CreatedAtUtc = now
        };

        var evChargerAsset = new Asset
        {
            Site = warehouseSite,
            Name = "Warehouse EV Charger",
            AssetTag = "EV-001",
            SerialNumber = "EV-DEMO-3001",
            AssetType = "EV Charger",
            Manufacturer = "Rolec",
            Model = "QUBEV",
            LocationDescription = "Warehouse car park",
            RatedVoltage = 230,
            RatedCurrent = 32,
            InstalledAtUtc = now.AddYears(-1),
            NextTestDueAtUtc = now.AddMonths(3),
            RiskLevel = RiskLevelConstants.Unknown,
            Notes = "Demo asset expected to fail with high risk.",
            IsActive = true,
            CreatedAtUtc = now
        };

        var cableAsset = new Asset
        {
            Site = warehouseSite,
            Name = "Incoming Supply Cable",
            AssetTag = "CBL-001",
            SerialNumber = "CBL-DEMO-4001",
            AssetType = "Cable",
            Manufacturer = "Prysmian",
            Model = "XLPE/SWA",
            LocationDescription = "Incoming supply route",
            RatedVoltage = 415,
            RatedCurrent = 125,
            InstalledAtUtc = now.AddYears(-6),
            NextTestDueAtUtc = now.AddMonths(1),
            RiskLevel = RiskLevelConstants.Unknown,
            Notes = "Demo asset expected to fail safety-critical insulation resistance.",
            IsActive = true,
            CreatedAtUtc = now
        };

        context.Customers.Add(customer);
        context.Sites.AddRange(mainSite, warehouseSite);
        context.Assets.AddRange(motorAsset, transformerAsset, evChargerAsset, cableAsset);

        await context.SaveChangesAsync();

        await AddTestResultAsync(
            context,
            testEvaluationService,
            assetRiskService,
            motorAsset,
            "VG-DEMO-PASS-001",
            now.AddDays(-12),
            now.AddMonths(12),
            "All measurements are within acceptable range.",
            new List<Measurement>
            {
                new Measurement
                {
                    MeasurementType = "Insulation Resistance",
                    Phase = "L1-L2-L3",
                    Value = 100,
                    Unit = "MOhm",
                    MinimumAllowedValue = 1,
                    WarningMinimumValue = 2,
                    Notes = "Healthy insulation reading.",
                    DisplayOrder = 1
                },
                new Measurement
                {
                    MeasurementType = "Earth Continuity",
                    Phase = "PE",
                    Value = 0.08m,
                    Unit = "Ohm",
                    MaximumAllowedValue = 0.5m,
                    WarningMaximumValue = 0.3m,
                    Notes = "Earth continuity passed.",
                    DisplayOrder = 2
                },
                new Measurement
                {
                    MeasurementType = "Leakage Current",
                    Phase = "L-N",
                    Value = 0.7m,
                    Unit = "mA",
                    MaximumAllowedValue = 3.5m,
                    WarningMaximumValue = 2.5m,
                    Notes = "Leakage current within safe range.",
                    DisplayOrder = 3
                }
            });

        await AddTestResultAsync(
            context,
            testEvaluationService,
            assetRiskService,
            transformerAsset,
            "VG-DEMO-WARN-001",
            now.AddDays(-8),
            now.AddMonths(6),
            "One reading is close to the warning threshold.",
            new List<Measurement>
            {
                new Measurement
                {
                    MeasurementType = "Insulation Resistance",
                    Phase = "Primary-Secondary",
                    Value = 1.5m,
                    Unit = "MOhm",
                    MinimumAllowedValue = 1,
                    WarningMinimumValue = 2,
                    Notes = "Above fail limit but below preferred warning limit.",
                    DisplayOrder = 1
                },
                new Measurement
                {
                    MeasurementType = "Voltage",
                    Phase = "L1-L2",
                    Value = 414,
                    Unit = "V",
                    MinimumAllowedValue = 380,
                    MaximumAllowedValue = 440,
                    WarningMinimumValue = 400,
                    WarningMaximumValue = 430,
                    Notes = "Voltage is normal.",
                    DisplayOrder = 2
                }
            });

        await AddTestResultAsync(
            context,
            testEvaluationService,
            assetRiskService,
            evChargerAsset,
            "VG-DEMO-FAIL-001",
            now.AddDays(-5),
            now.AddMonths(3),
            "Voltage measurement failed but is not safety-critical.",
            new List<Measurement>
            {
                new Measurement
                {
                    MeasurementType = "Voltage",
                    Phase = "L-N",
                    Value = 260,
                    Unit = "V",
                    MinimumAllowedValue = 210,
                    MaximumAllowedValue = 250,
                    WarningMinimumValue = 220,
                    WarningMaximumValue = 245,
                    Notes = "Voltage is above maximum allowed value.",
                    DisplayOrder = 1
                },
                new Measurement
                {
                    MeasurementType = "Earth Continuity",
                    Phase = "PE",
                    Value = 0.12m,
                    Unit = "Ohm",
                    MaximumAllowedValue = 0.5m,
                    WarningMaximumValue = 0.3m,
                    Notes = "Earth continuity is acceptable.",
                    DisplayOrder = 2
                }
            });

        await AddTestResultAsync(
            context,
            testEvaluationService,
            assetRiskService,
            cableAsset,
            "VG-DEMO-CRITICAL-001",
            now.AddDays(-2),
            now.AddMonths(1),
            "Safety-critical insulation resistance failure.",
            new List<Measurement>
            {
                new Measurement
                {
                    MeasurementType = "Insulation Resistance",
                    Phase = "L1-E",
                    Value = 0.4m,
                    Unit = "MOhm",
                    MinimumAllowedValue = 1,
                    WarningMinimumValue = 2,
                    Notes = "Below minimum allowed value. This should become Fail and Critical.",
                    DisplayOrder = 1
                },
                new Measurement
                {
                    MeasurementType = "Earth Continuity",
                    Phase = "PE",
                    Value = 0.09m,
                    Unit = "Ohm",
                    MaximumAllowedValue = 0.5m,
                    WarningMaximumValue = 0.3m,
                    Notes = "Earth continuity is acceptable.",
                    DisplayOrder = 2
                }
            });

        context.Jobs.AddRange(
            new Job
            {
                AssetId = cableAsset.Id,
                Title = "Urgent insulation failure follow-up",
                Description = "Investigate safety-critical insulation resistance failure and confirm isolation controls.",
                JobType = JobTypeConstants.Repair,
                Priority = JobPriorityConstants.Critical,
                Status = JobStatusConstants.InProgress,
                AssignedTo = "Demo Field Engineer",
                ScheduledAtUtc = now.AddDays(-1),
                DueAtUtc = now.AddDays(1),
                StartedAtUtc = now.AddHours(-6),
                Notes = "Created from demo critical test scenario.",
                CreatedAtUtc = now
            },
            new Job
            {
                AssetId = evChargerAsset.Id,
                Title = "EV charger voltage retest",
                Description = "Run a focused retest after voltage fault investigation.",
                JobType = JobTypeConstants.Retest,
                Priority = JobPriorityConstants.High,
                Status = JobStatusConstants.Scheduled,
                AssignedTo = "Demo Field Engineer",
                ScheduledAtUtc = now.AddDays(1),
                DueAtUtc = now.AddDays(3),
                Notes = "Created from demo failed test result.",
                CreatedAtUtc = now
            },
            new Job
            {
                AssetId = transformerAsset.Id,
                Title = "Transformer warning review",
                Description = "Review warning-range insulation reading and decide whether maintenance is needed.",
                JobType = JobTypeConstants.FollowUp,
                Priority = JobPriorityConstants.Medium,
                Status = JobStatusConstants.Scheduled,
                AssignedTo = "Demo Field Engineer",
                ScheduledAtUtc = now.AddDays(2),
                DueAtUtc = now.AddDays(7),
                CreatedAtUtc = now
            });

        await context.SaveChangesAsync();
    }

    private static async Task SeedEngineerUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string engineerEmail = "engineer@voltguard.local";
        const string engineerPassword = "Engineer@12345";

        var engineerUser = await userManager.FindByEmailAsync(engineerEmail);

        if (engineerUser is not null)
        {
            return;
        }

        engineerUser = new ApplicationUser
        {
            UserName = engineerEmail,
            Email = engineerEmail,
            EmailConfirmed = true,
            FullName = "Demo Field Engineer",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(engineerUser, engineerPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Failed to seed engineer user: {errors}");
        }

        await userManager.AddToRoleAsync(engineerUser, RoleConstants.Engineer);
    }

    private static async Task AddTestResultAsync(
        AppDbContext context,
        ITestEvaluationService testEvaluationService,
        IAssetRiskService assetRiskService,
        Asset asset,
        string testReference,
        DateTime testDateUtc,
        DateTime nextTestDueUtc,
        string summary,
        List<Measurement> measurements)
    {
        var testResult = new TestResult
        {
            AssetId = asset.Id,
            Asset = asset,
            TestReference = testReference,
            TestType = "Manual",
            TestDateUtc = testDateUtc,
            EngineerName = "Demo Field Engineer",
            InstrumentSerialNumber = "MG-DEMO-0001",
            InstrumentModel = "Megger Demo Tester",
            NextTestDueAtUtc = nextTestDueUtc,
            Summary = summary,
            Notes = "Seeded demo test result for Swagger/frontend/presentation.",
            OverallStatus = TestStatusConstants.Pass,
            RiskLevel = RiskLevelConstants.Unknown,
            CreatedAtUtc = DateTime.UtcNow,
            Measurements = measurements
        };

        testEvaluationService.EvaluateAndApply(testResult);

        var recentTestsForAsset = await context.TestResults
            .Include(x => x.Measurements)
            .Where(x => x.AssetId == asset.Id && !x.IsDeleted)
            .ToListAsync();

        assetRiskService.ApplyRisk(testResult, asset, recentTestsForAsset);

        asset.LastTestedAtUtc = testDateUtc;
        asset.NextTestDueAtUtc = nextTestDueUtc;
        asset.UpdatedAtUtc = DateTime.UtcNow;

        context.TestResults.Add(testResult);

        await context.SaveChangesAsync();
    }
}
