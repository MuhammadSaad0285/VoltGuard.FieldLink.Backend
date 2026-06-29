namespace VoltGuard.Domain.Entities;

public class Measurement
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TestResultId { get; set; }
    public TestResult TestResult { get; set; } = null!;

    public string MeasurementType { get; set; } = string.Empty;
    public string? Phase { get; set; }

    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;

    // Allowed range. Outside this range = Fail.
    public decimal? MinimumAllowedValue { get; set; }
    public decimal? MaximumAllowedValue { get; set; }

    // Preferred range. Inside allowed range but outside this preferred range = Warning.
    public decimal? WarningMinimumValue { get; set; }
    public decimal? WarningMaximumValue { get; set; }

    // Calculated by backend.
    // True only when this is a safety-critical measurement and its Status is Fail.
    public bool IsCritical { get; set; }

    // Calculated by backend. User should not manually set this.
    public string Status { get; set; } = "Pass";

    public string? Notes { get; set; }

    public int DisplayOrder { get; set; }
}
