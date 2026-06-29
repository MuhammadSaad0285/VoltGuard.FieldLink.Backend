using Microsoft.EntityFrameworkCore;
using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.TestResults;
using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;
using VoltGuard.Infrastructure.Data;

namespace VoltGuard.Infrastructure.Services;

public class TestResultService : ITestResultService
{
    private static readonly string[] AllowedStatuses = TestStatusConstants.All;
    private static readonly string[] AllowedRiskLevels = RiskLevelConstants.All;

    private const int RecentFailureWindowDays = 90;

    private readonly AppDbContext _context;
    private readonly ITestEvaluationService _testEvaluationService;
    private readonly IAssetRiskService _assetRiskService;

    public TestResultService(
        AppDbContext context,
        ITestEvaluationService testEvaluationService,
        IAssetRiskService assetRiskService)
    {
        _context = context;
        _testEvaluationService = testEvaluationService;
        _assetRiskService = assetRiskService;
    }

    public async Task<PagedResult<TestResultDto>> GetAllAsync(
        Guid? customerId,
        Guid? siteId,
        Guid? assetId,
        string? status,
        string? riskLevel,
        string? search,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _context.TestResults
            .AsNoTracking()
            .Include(x => x.Asset)
            .ThenInclude(x => x.Site)
            .ThenInclude(x => x.Customer)
            .Include(x => x.Measurements)
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(x => x.Asset.Site.CustomerId == customerId.Value);
        }

        if (siteId.HasValue)
        {
            query = query.Where(x => x.Asset.SiteId == siteId.Value);
        }

        if (assetId.HasValue)
        {
            query = query.Where(x => x.AssetId == assetId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeChoice(status, TestStatusConstants.Pass, AllowedStatuses, "status");
            query = query.Where(x => x.OverallStatus == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(riskLevel))
        {
            var normalizedRiskLevel = NormalizeChoice(riskLevel, RiskLevelConstants.Unknown, AllowedRiskLevels, "risk level");
            query = query.Where(x => x.RiskLevel == normalizedRiskLevel);
        }

        if (fromDateUtc.HasValue)
        {
            query = query.Where(x => x.TestDateUtc >= fromDateUtc.Value);
        }

        if (toDateUtc.HasValue)
        {
            query = query.Where(x => x.TestDateUtc <= toDateUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchText = search.Trim();
            var pattern = $"%{searchText}%";

            query = query.Where(x =>
                (x.TestReference != null && EF.Functions.Like(x.TestReference, pattern)) ||
                EF.Functions.Like(x.TestType, pattern) ||
                EF.Functions.Like(x.EngineerName, pattern) ||
                EF.Functions.Like(x.OverallStatus, pattern) ||
                EF.Functions.Like(x.RiskLevel, pattern) ||
                (x.InstrumentSerialNumber != null && EF.Functions.Like(x.InstrumentSerialNumber, pattern)) ||
                (x.InstrumentModel != null && EF.Functions.Like(x.InstrumentModel, pattern)) ||
                EF.Functions.Like(x.Asset.Name, pattern) ||
                (x.Asset.AssetTag != null && EF.Functions.Like(x.Asset.AssetTag, pattern)) ||
                EF.Functions.Like(x.Asset.Site.Name, pattern) ||
                EF.Functions.Like(x.Asset.Site.Customer.Name, pattern));
        }

        var totalCount = await query.CountAsync();

        var results = await query
            .OrderByDescending(x => x.TestDateUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TestResultDto
            {
                Id = x.Id,
                AssetId = x.AssetId,
                AssetName = x.Asset.Name,
                AssetTag = x.Asset.AssetTag,
                SiteId = x.Asset.SiteId,
                SiteName = x.Asset.Site.Name,
                CustomerId = x.Asset.Site.CustomerId,
                CustomerName = x.Asset.Site.Customer.Name,
                TestReference = x.TestReference,
                TestType = x.TestType,
                TestDateUtc = x.TestDateUtc,
                OverallStatus = x.OverallStatus,
                RiskLevel = x.RiskLevel,
                EngineerName = x.EngineerName,
                InstrumentSerialNumber = x.InstrumentSerialNumber,
                InstrumentModel = x.InstrumentModel,
                NextTestDueAtUtc = x.NextTestDueAtUtc,
                Summary = x.Summary,
                Notes = x.Notes,
                MeasurementCount = x.Measurements.Count,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync();

        return new PagedResult<TestResultDto>
        {
            Items = results,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TestResultDetailDto?> GetByIdAsync(Guid id)
    {
        var testResult = await _context.TestResults
            .AsNoTracking()
            .Include(x => x.Asset)
            .ThenInclude(x => x.Site)
            .ThenInclude(x => x.Customer)
            .Include(x => x.Measurements)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        return testResult is null ? null : MapToDetailDto(testResult);
    }

    public async Task<TestResultDetailDto> CreateManualAsync(CreateManualTestResultDto request)
    {
        if (request.AssetId == Guid.Empty)
        {
            throw new InvalidOperationException("AssetId is required.");
        }

        EnsureMeasurementsExist(request.Measurements);

        var asset = await _context.Assets
            .Include(x => x.Site)
            .ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == request.AssetId && x.IsActive);

        if (asset is null)
        {
            throw new InvalidOperationException("Active asset not found.");
        }

        var engineerName = NormalizeRequiredText(request.EngineerName, "Engineer name is required.");
        var testType = NormalizeRequiredText(request.TestType, "Test type is required.");
        var testReference = NormalizeOptionalText(request.TestReference);

        if (!string.IsNullOrWhiteSpace(testReference))
        {
            var duplicateReferenceExists = await _context.TestResults
                .AnyAsync(x =>
                    x.TestReference == testReference &&
                    !x.IsDeleted);

            if (duplicateReferenceExists)
            {
                throw new InvalidOperationException("A test result with this test reference already exists.");
            }
        }

        var testResult = new TestResult
        {
            Id = Guid.NewGuid(),
            AssetId = request.AssetId,
            TestReference = testReference,
            TestType = testType,
            TestDateUtc = request.TestDateUtc,
            OverallStatus = TestStatusConstants.Pass,
            RiskLevel = RiskLevelConstants.Unknown,
            EngineerName = engineerName,
            InstrumentSerialNumber = NormalizeOptionalText(request.InstrumentSerialNumber),
            InstrumentModel = NormalizeOptionalText(request.InstrumentModel),
            NextTestDueAtUtc = request.NextTestDueAtUtc,
            Summary = NormalizeOptionalText(request.Summary),
            Notes = NormalizeOptionalText(request.Notes),
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            Measurements = BuildMeasurements(request.Measurements)
        };

        _testEvaluationService.EvaluateAndApply(testResult);

        var recentResults = await GetRecentTestResultsForAssetAsync(
            request.AssetId,
            request.TestDateUtc,
            excludedTestResultId: null);

        _assetRiskService.ApplyRisk(testResult, asset, recentResults);

        _context.TestResults.Add(testResult);
        await _context.SaveChangesAsync();

        await RefreshAssetSnapshotAsync(request.AssetId);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(testResult.Id))!;
    }

    public async Task<TestResultDetailDto?> UpdateAsync(Guid id, UpdateTestResultDto request)
    {
        if (request.AssetId == Guid.Empty)
        {
            throw new InvalidOperationException("AssetId is required.");
        }

        EnsureMeasurementsExist(request.Measurements);

        var testResult = await _context.TestResults
            .Include(x => x.Measurements)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (testResult is null)
        {
            return null;
        }

        var originalAssetId = testResult.AssetId;

        var asset = await _context.Assets
            .Include(x => x.Site)
            .ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == request.AssetId && x.IsActive);

        if (asset is null)
        {
            throw new InvalidOperationException("Active asset not found.");
        }

        var engineerName = NormalizeRequiredText(request.EngineerName, "Engineer name is required.");
        var testType = NormalizeRequiredText(request.TestType, "Test type is required.");
        var testReference = NormalizeOptionalText(request.TestReference);

        if (!string.IsNullOrWhiteSpace(testReference))
        {
            var duplicateReferenceExists = await _context.TestResults
                .AnyAsync(x =>
                    x.Id != id &&
                    x.TestReference == testReference &&
                    !x.IsDeleted);

            if (duplicateReferenceExists)
            {
                throw new InvalidOperationException("Another test result with this test reference already exists.");
            }
        }

        testResult.AssetId = request.AssetId;
        testResult.TestReference = testReference;
        testResult.TestType = testType;
        testResult.TestDateUtc = request.TestDateUtc;
        testResult.RiskLevel = RiskLevelConstants.Unknown;
        testResult.EngineerName = engineerName;
        testResult.InstrumentSerialNumber = NormalizeOptionalText(request.InstrumentSerialNumber);
        testResult.InstrumentModel = NormalizeOptionalText(request.InstrumentModel);
        testResult.NextTestDueAtUtc = request.NextTestDueAtUtc;
        testResult.Summary = NormalizeOptionalText(request.Summary);
        testResult.Notes = NormalizeOptionalText(request.Notes);
        testResult.UpdatedAtUtc = DateTime.UtcNow;

        var oldMeasurements = testResult.Measurements.ToList();
        _context.Measurements.RemoveRange(oldMeasurements);
        testResult.Measurements.Clear();

        foreach (var measurement in BuildMeasurements(request.Measurements))
        {
            testResult.Measurements.Add(measurement);
        }

        _testEvaluationService.EvaluateAndApply(testResult);

        var recentResults = await GetRecentTestResultsForAssetAsync(
            request.AssetId,
            request.TestDateUtc,
            excludedTestResultId: id);

        _assetRiskService.ApplyRisk(testResult, asset, recentResults);

        await _context.SaveChangesAsync();

        await RefreshAssetSnapshotAsync(request.AssetId);

        if (originalAssetId != request.AssetId)
        {
            await RefreshAssetSnapshotAsync(originalAssetId);
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var testResult = await _context.TestResults.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (testResult is null)
        {
            return false;
        }

        var assetId = testResult.AssetId;

        testResult.IsDeleted = true;
        testResult.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await RefreshAssetSnapshotAsync(assetId);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PagedResult<TestHistoryItemDto>?> GetTestHistoryForAssetAsync(
        Guid assetId,
        string? status,
        string? riskLevel,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        int pageNumber,
        int pageSize)
    {
        if (assetId == Guid.Empty)
        {
            throw new InvalidOperationException("AssetId is required.");
        }

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var assetExists = await _context.Assets
            .AsNoTracking()
            .AnyAsync(x => x.Id == assetId);

        if (!assetExists)
        {
            return null;
        }

        var query = _context.TestResults
            .AsNoTracking()
            .Include(x => x.Measurements)
            .Where(x => x.AssetId == assetId && !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeChoice(status, TestStatusConstants.Pass, AllowedStatuses, "status");
            query = query.Where(x => x.OverallStatus == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(riskLevel))
        {
            var normalizedRiskLevel = NormalizeChoice(riskLevel, RiskLevelConstants.Unknown, AllowedRiskLevels, "risk level");
            query = query.Where(x => x.RiskLevel == normalizedRiskLevel);
        }

        if (fromDateUtc.HasValue)
        {
            query = query.Where(x => x.TestDateUtc >= fromDateUtc.Value);
        }

        if (toDateUtc.HasValue)
        {
            query = query.Where(x => x.TestDateUtc <= toDateUtc.Value);
        }

        var totalCount = await query.CountAsync();

        var testResults = await query
            .OrderByDescending(x => x.TestDateUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TestHistoryItemDto>
        {
            Items = testResults.Select(MapToHistoryItemDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    private async Task<List<TestResult>> GetRecentTestResultsForAssetAsync(
        Guid assetId,
        DateTime testDateUtc,
        Guid? excludedTestResultId)
    {
        var fromDateUtc = testDateUtc.AddDays(-RecentFailureWindowDays);

        var query = _context.TestResults
            .AsNoTracking()
            .Include(x => x.Measurements)
            .Where(x => x.AssetId == assetId)
            .Where(x => !x.IsDeleted)
            .Where(x => x.TestDateUtc >= fromDateUtc && x.TestDateUtc <= testDateUtc);

        if (excludedTestResultId.HasValue)
        {
            query = query.Where(x => x.Id != excludedTestResultId.Value);
        }

        return await query.ToListAsync();
    }

    private async Task RefreshAssetSnapshotAsync(Guid assetId)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(x => x.Id == assetId);

        if (asset is null)
        {
            return;
        }

        var latestTestResult = await _context.TestResults
            .Where(x => x.AssetId == assetId && !x.IsDeleted)
            .OrderByDescending(x => x.TestDateUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (latestTestResult is null)
        {
            asset.LastTestedAtUtc = null;
            asset.NextTestDueAtUtc = null;
            asset.RiskLevel = RiskLevelConstants.Unknown;
            asset.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        asset.LastTestedAtUtc = latestTestResult.TestDateUtc;
        asset.NextTestDueAtUtc = latestTestResult.NextTestDueAtUtc;
        asset.RiskLevel = latestTestResult.RiskLevel;
        asset.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static List<Measurement> BuildMeasurements(IEnumerable<CreateMeasurementDto>? measurements)
    {
        EnsureMeasurementsExist(measurements);

        var result = new List<Measurement>();
        var fallbackOrder = 1;

        foreach (var item in measurements!)
        {
            var measurementType = NormalizeRequiredText(item.MeasurementType, "Measurement type is required.");
            var unit = NormalizeRequiredText(item.Unit, "Measurement unit is required.");

            result.Add(new Measurement
            {
                Id = Guid.NewGuid(),
                MeasurementType = measurementType,
                Phase = NormalizeOptionalText(item.Phase),
                Value = item.Value,
                Unit = unit,
                MinimumAllowedValue = item.MinimumAllowedValue,
                MaximumAllowedValue = item.MaximumAllowedValue,
                WarningMinimumValue = item.WarningMinimumValue,
                WarningMaximumValue = item.WarningMaximumValue,
                Status = TestStatusConstants.Pass,
                Notes = NormalizeOptionalText(item.Notes),
                DisplayOrder = item.DisplayOrder > 0 ? item.DisplayOrder : fallbackOrder
            });

            fallbackOrder++;
        }

        return result;
    }


    private static TestHistoryItemDto MapToHistoryItemDto(TestResult testResult)
    {
        var measurements = testResult.Measurements
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.MeasurementType)
            .ToList();

        var passCount = measurements.Count(x =>
            string.Equals(x.Status, "Pass", StringComparison.OrdinalIgnoreCase));

        var warningCount = measurements.Count(x =>
            string.Equals(x.Status, "Warning", StringComparison.OrdinalIgnoreCase));

        var failCount = measurements.Count(x =>
            string.Equals(x.Status, "Fail", StringComparison.OrdinalIgnoreCase));

        var criticalFailCount = measurements.Count(x =>
            x.IsCritical &&
            string.Equals(x.Status, "Fail", StringComparison.OrdinalIgnoreCase));

        return new TestHistoryItemDto
        {
            Id = testResult.Id,
            TestReference = testResult.TestReference,
            TestType = testResult.TestType,
            TestDateUtc = testResult.TestDateUtc,
            EngineerName = testResult.EngineerName,
            OverallStatus = testResult.OverallStatus,
            RiskLevel = testResult.RiskLevel,
            Summary = testResult.Summary,
            Notes = testResult.Notes,
            CreatedAtUtc = testResult.CreatedAtUtc,
            MeasurementsSummary = new MeasurementsSummaryDto
            {
                TotalCount = measurements.Count,
                PassCount = passCount,
                WarningCount = warningCount,
                FailCount = failCount,
                CriticalFailCount = criticalFailCount,
                Measurements = measurements
                    .Select(x => new TestHistoryMeasurementDto
                    {
                        Id = x.Id,
                        MeasurementType = x.MeasurementType,
                        Phase = x.Phase,
                        Value = x.Value,
                        Unit = x.Unit,
                        Status = x.Status,
                        IsCritical = x.IsCritical
                    })
                    .ToList()
            }
        };
    }
    private static TestResultDetailDto MapToDetailDto(TestResult testResult)
    {
        return new TestResultDetailDto
        {
            Id = testResult.Id,
            AssetId = testResult.AssetId,
            AssetName = testResult.Asset.Name,
            AssetTag = testResult.Asset.AssetTag,
            SiteId = testResult.Asset.SiteId,
            SiteName = testResult.Asset.Site.Name,
            CustomerId = testResult.Asset.Site.CustomerId,
            CustomerName = testResult.Asset.Site.Customer.Name,
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
            MeasurementCount = testResult.Measurements.Count,
            CreatedAtUtc = testResult.CreatedAtUtc,
            UpdatedAtUtc = testResult.UpdatedAtUtc,
            Measurements = testResult.Measurements
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.MeasurementType)
                .Select(x => new MeasurementDto
                {
                    Id = x.Id,
                    TestResultId = x.TestResultId,
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
    }

    private static void EnsureMeasurementsExist(IEnumerable<CreateMeasurementDto>? measurements)
    {
        if (measurements is null || !measurements.Any())
        {
            throw new InvalidOperationException("At least one measurement is required for a test result.");
        }
    }

    private static string NormalizeChoice(string? value, string defaultValue, string[] allowedValues, string fieldName)
    {
        var text = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();

        var match = allowedValues.FirstOrDefault(x =>
            string.Equals(x, text, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            throw new InvalidOperationException(
                $"Invalid {fieldName}. Allowed values: {string.Join(", ", allowedValues)}.");
        }

        return match;
    }

    private static string NormalizeRequiredText(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}



