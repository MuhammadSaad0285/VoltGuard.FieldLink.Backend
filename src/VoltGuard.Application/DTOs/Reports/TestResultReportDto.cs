namespace VoltGuard.Application.DTOs.Reports;

public class TestResultReportDto
{
    public DateTime GeneratedAtUtc { get; set; }

    public ReportCustomerDto Customer { get; set; } = new();
    public ReportSiteDto Site { get; set; } = new();
    public ReportAssetDto Asset { get; set; } = new();

    public Guid TestResultId { get; set; }
    public string? TestReference { get; set; }
    public string TestType { get; set; } = string.Empty;
    public DateTime TestDateUtc { get; set; }

    public string OverallStatus { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;

    public string EngineerName { get; set; } = string.Empty;

    public string? InstrumentSerialNumber { get; set; }
    public string? InstrumentModel { get; set; }

    public DateTime? NextTestDueAtUtc { get; set; }

    public string? Summary { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public IReadOnlyList<ReportMeasurementDto> Measurements { get; set; } =
        Array.Empty<ReportMeasurementDto>();
}

public class ReportCustomerDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }

    public string? Notes { get; set; }
}

public class ReportSiteDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? SiteCode { get; set; }
    public string? SiteType { get; set; }

    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }

    public string? Notes { get; set; }
}

public class ReportAssetDto
{
    public Guid Id { get; set; }

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

    public string RiskLevel { get; set; } = string.Empty;

    public string? Notes { get; set; }
}

public class ReportMeasurementDto
{
    public Guid Id { get; set; }

    public string MeasurementType { get; set; } = string.Empty;
    public string? Phase { get; set; }

    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;

    public decimal? MinimumAllowedValue { get; set; }
    public decimal? MaximumAllowedValue { get; set; }

    public decimal? WarningMinimumValue { get; set; }
    public decimal? WarningMaximumValue { get; set; }

    public bool IsCritical { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public int DisplayOrder { get; set; }
}
