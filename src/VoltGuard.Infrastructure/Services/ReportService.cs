using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using VoltGuard.Application.DTOs.Reports;
using VoltGuard.Application.Interfaces;
using VoltGuard.Infrastructure.Data;
using VoltGuard.Infrastructure.Reports;

namespace VoltGuard.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReportFileDto?> GenerateTestResultReportAsync(Guid testResultId)
    {
        if (testResultId == Guid.Empty)
        {
            throw new InvalidOperationException("TestResultId is required.");
        }

        var testResult = await _context.TestResults
            .AsNoTracking()
            .Include(x => x.Asset)
            .ThenInclude(x => x.Site)
            .ThenInclude(x => x.Customer)
            .Include(x => x.Measurements)
            .FirstOrDefaultAsync(x => x.Id == testResultId && !x.IsDeleted);

        if (testResult is null)
        {
            return null;
        }

        var report = new TestResultReportDto
        {
            GeneratedAtUtc = DateTime.UtcNow,

            Customer = new ReportCustomerDto
            {
                Id = testResult.Asset.Site.Customer.Id,
                Name = testResult.Asset.Site.Customer.Name,
                ContactPerson = testResult.Asset.Site.Customer.ContactPerson,
                ContactEmail = testResult.Asset.Site.Customer.ContactEmail,
                ContactPhone = testResult.Asset.Site.Customer.ContactPhone,
                AddressLine1 = testResult.Asset.Site.Customer.AddressLine1,
                AddressLine2 = testResult.Asset.Site.Customer.AddressLine2,
                City = testResult.Asset.Site.Customer.City,
                Postcode = testResult.Asset.Site.Customer.Postcode,
                Country = testResult.Asset.Site.Customer.Country,
                Notes = testResult.Asset.Site.Customer.Notes
            },

            Site = new ReportSiteDto
            {
                Id = testResult.Asset.Site.Id,
                Name = testResult.Asset.Site.Name,
                SiteCode = testResult.Asset.Site.SiteCode,
                SiteType = testResult.Asset.Site.SiteType,
                ContactPerson = testResult.Asset.Site.ContactPerson,
                ContactEmail = testResult.Asset.Site.ContactEmail,
                ContactPhone = testResult.Asset.Site.ContactPhone,
                AddressLine1 = testResult.Asset.Site.AddressLine1,
                AddressLine2 = testResult.Asset.Site.AddressLine2,
                City = testResult.Asset.Site.City,
                Postcode = testResult.Asset.Site.Postcode,
                Country = testResult.Asset.Site.Country,
                Notes = testResult.Asset.Site.Notes
            },

            Asset = new ReportAssetDto
            {
                Id = testResult.Asset.Id,
                Name = testResult.Asset.Name,
                AssetTag = testResult.Asset.AssetTag,
                SerialNumber = testResult.Asset.SerialNumber,
                AssetType = testResult.Asset.AssetType,
                Manufacturer = testResult.Asset.Manufacturer,
                Model = testResult.Asset.Model,
                LocationDescription = testResult.Asset.LocationDescription,
                RatedVoltage = testResult.Asset.RatedVoltage,
                RatedCurrent = testResult.Asset.RatedCurrent,
                InstalledAtUtc = testResult.Asset.InstalledAtUtc,
                LastTestedAtUtc = testResult.Asset.LastTestedAtUtc,
                NextTestDueAtUtc = testResult.Asset.NextTestDueAtUtc,
                RiskLevel = testResult.Asset.RiskLevel,
                Notes = testResult.Asset.Notes
            },

            TestResultId = testResult.Id,
            TestReference = testResult.TestReference,
            TestType = testResult.TestType,
            TestDateUtc = testResult.TestDateUtc,
            OverallStatus = testResult.OverallStatus,
            RiskLevel = testResult.RiskLevel,
            EngineerName = testResult.EngineerName,
            InstrumentSerialNumber = testResult.InstrumentSerialNumber,
            InstrumentModel = testResult.InstrumentModel,
            NextTestDueAtUtc = testResult.NextTestDueAtUtc,
            Summary = testResult.Summary,
            Notes = testResult.Notes,
            CreatedAtUtc = testResult.CreatedAtUtc,
            UpdatedAtUtc = testResult.UpdatedAtUtc,

            Measurements = testResult.Measurements
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.MeasurementType)
                .Select(x => new ReportMeasurementDto
                {
                    Id = x.Id,
                    MeasurementType = x.MeasurementType,
                    Phase = x.Phase,
                    Value = x.Value,
                    Unit = x.Unit,
                    MinimumAllowedValue = x.MinimumAllowedValue,
                    MaximumAllowedValue = x.MaximumAllowedValue,
                    WarningMinimumValue = x.WarningMinimumValue,
                    WarningMaximumValue = x.WarningMaximumValue,
                    IsCritical = x.IsCritical,
                    Status = x.Status,
                    Notes = x.Notes,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList()
        };

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = new TestResultPdfDocument(report).GeneratePdf();

        return new ReportFileDto
        {
            FileName = BuildFileName(report),
            ContentType = "application/pdf",
            Content = pdfBytes
        };
    }

    private static string BuildFileName(TestResultReportDto report)
    {
        var reportNumber = BuildReportNumber(report);
        var safeReportNumber = Regex.Replace(reportNumber, @"[^a-zA-Z0-9_\-]+", "-");
        var datePart = report.GeneratedAtUtc.ToString("yyyyMMdd-HHmmss");

        return $"VoltGuard-TestReport-{safeReportNumber}-{datePart}.pdf";
    }

    private static string BuildReportNumber(TestResultReportDto report)
    {
        if (!string.IsNullOrWhiteSpace(report.TestReference))
        {
            return report.TestReference.Trim();
        }

        return $"VG-TR-{report.TestDateUtc:yyyyMMdd}-{report.TestResultId.ToString("N")[..8].ToUpperInvariant()}";
    }
}

