using System.ComponentModel.DataAnnotations;

namespace VoltGuard.Application.DTOs.Jobs;

public class CompleteJobDto
{
    [MaxLength(1000)]
    public string? CompletionNotes { get; set; }
}
