using System.ComponentModel.DataAnnotations;

namespace VoltGuard.Application.DTOs.Assets;

public class CreateAssetDto
{
    [Required]
    public Guid SiteId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? AssetTag { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    [MaxLength(50)]
    public string? AssetType { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(200)]
    public string? LocationDescription { get; set; }

    public decimal? RatedVoltage { get; set; }

    public decimal? RatedCurrent { get; set; }

    public DateTime? InstalledAtUtc { get; set; }

    public DateTime? LastTestedAtUtc { get; set; }

    public DateTime? NextTestDueAtUtc { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
