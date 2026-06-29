namespace VoltGuard.Application.DTOs.Assets;

public class AssetDto
{
    public Guid Id { get; set; }

    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

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

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
