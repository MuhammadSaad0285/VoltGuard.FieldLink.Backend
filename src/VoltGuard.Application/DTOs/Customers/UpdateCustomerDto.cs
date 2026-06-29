using System.ComponentModel.DataAnnotations;

namespace VoltGuard.Application.DTOs.Customers;

public class UpdateCustomerDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

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
