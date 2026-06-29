namespace VoltGuard.Domain.Entities;

public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SiteId { get; set; }
    public Site Site { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? AssetTag { get; set; }
    public string? SerialNumber { get; set; }
    public string? AssetType { get; set; }

    public string? Manufacturer { get; set; }
    public string? Model { get; set; }

    public string? LocationDescription { get; set; }

    public decimal? RatedVoltage { get; set; }
    public decimal? RatedCurrent { get; set; }

    public DateTime? InstalledAtUtc { get; set; }
    public DateTime? LastTestedAtUtc { get; set; }
    public DateTime? NextTestDueAtUtc { get; set; }

    // Calculated from the latest TestResult. User should not manually set this.
    public string RiskLevel { get; set; } = "Unknown";

    public string? LatestRiskLevel { get; set; }

    public string? CurrentRiskLevel { get; set; }

    public string? AssetRiskLevel { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
