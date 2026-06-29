using System.ComponentModel.DataAnnotations;

namespace VoltGuard.Application.DTOs.Jobs;

public class CancelJobDto
{
    [MaxLength(1000)]
    public string? CancellationReason { get; set; }
}
