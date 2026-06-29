namespace VoltGuard.Application.DTOs.Customers;

public class CustomerDto
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

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public int SitesCount { get; set; }
    public int AssetsCount { get; set; }
}
