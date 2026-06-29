namespace VoltGuard.Domain.Entities;

public class TestResult
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public string? TestReference { get; set; }

    public string TestType { get; set; } = "Manual";

    public DateTime TestDateUtc { get; set; } = DateTime.UtcNow;

    public string OverallStatus { get; set; } = "Unknown";
    public string RiskLevel { get; set; } = "Unknown";

    public string EngineerName { get; set; } = string.Empty;

    public string? InstrumentSerialNumber { get; set; }
    public string? InstrumentModel { get; set; }

    public DateTime? NextTestDueAtUtc { get; set; }

    public string? Summary { get; set; }
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}
