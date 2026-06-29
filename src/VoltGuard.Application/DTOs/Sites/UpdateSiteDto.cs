using System.ComponentModel.DataAnnotations;

namespace VoltGuard.Application.DTOs.Sites;

public class UpdateSiteDto
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SiteCode { get; set; }

    [MaxLength(50)]
    public string? SiteType { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? AddressLine1 { get; set; }

    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(30)]
    public string? Postcode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
