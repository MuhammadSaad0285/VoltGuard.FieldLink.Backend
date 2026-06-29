using System.ComponentModel.DataAnnotations;

namespace VoltGuard.Application.DTOs.Jobs;

public class UpdateJobDto
{
    [Required]
    public Guid AssetId { get; set; }

    public Guid? TestResultId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string JobType { get; set; } = "Inspection";

    [Required]
    [MaxLength(30)]
    public string Priority { get; set; } = "Medium";

    [MaxLength(150)]
    public string? AssignedTo { get; set; }

    public DateTime ScheduledAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DueAtUtc { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
