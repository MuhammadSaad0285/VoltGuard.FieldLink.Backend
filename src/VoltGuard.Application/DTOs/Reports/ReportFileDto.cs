namespace VoltGuard.Application.DTOs.Reports;

public class ReportFileDto
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/pdf";

    public byte[] Content { get; set; } = Array.Empty<byte>();
}
