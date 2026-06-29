using Microsoft.EntityFrameworkCore;
using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.Jobs;
using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;
using VoltGuard.Infrastructure.Data;

namespace VoltGuard.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly AppDbContext _context;

    public JobService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<JobDto>> GetAllAsync(
        Guid? customerId,
        Guid? siteId,
        Guid? assetId,
        string? status,
        string? priority,
        string? jobType,
        string? search,
        bool overdueOnly,
        int pageNumber,
        int pageSize,
        IReadOnlyCollection<string>? assignedToScope = null)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var todayUtc = DateTime.UtcNow.Date;

        var query = ApplyAssignedToScope(BaseQuery().AsNoTracking(), assignedToScope);

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
            var normalizedStatus = NormalizeChoice(status, JobStatusConstants.Scheduled, JobStatusConstants.All, "status");
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            var normalizedPriority = NormalizeChoice(priority, JobPriorityConstants.Medium, JobPriorityConstants.All, "priority");
            query = query.Where(x => x.Priority == normalizedPriority);
        }

        if (!string.IsNullOrWhiteSpace(jobType))
        {
            var normalizedJobType = NormalizeChoice(jobType, JobTypeConstants.Inspection, JobTypeConstants.All, "jobType");
            query = query.Where(x => x.JobType == normalizedJobType);
        }

        if (overdueOnly)
        {
            query = query.Where(x =>
                x.DueAtUtc.HasValue &&
                x.DueAtUtc.Value.Date < todayUtc &&
                JobStatusConstants.Open.Contains(x.Status));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.Like(x.Title, pattern) ||
                (x.Description != null && EF.Functions.Like(x.Description, pattern)) ||
                (x.AssignedTo != null && EF.Functions.Like(x.AssignedTo, pattern)) ||
                (x.Asset.AssetTag != null && EF.Functions.Like(x.Asset.AssetTag, pattern)) ||
                EF.Functions.Like(x.Asset.Name, pattern) ||
                EF.Functions.Like(x.Asset.Site.Name, pattern) ||
                EF.Functions.Like(x.Asset.Site.Customer.Name, pattern) ||
                (x.TestResult != null &&
                    x.TestResult.TestReference != null &&
                    EF.Functions.Like(x.TestResult.TestReference, pattern)));
        }

        var totalCount = await query.CountAsync();

        var jobs = await query
            .OrderBy(x => x.Status == JobStatusConstants.InProgress ? 0 : x.Status == JobStatusConstants.Scheduled ? 1 : 2)
            .ThenBy(x => x.DueAtUtc ?? x.ScheduledAtUtc)
            .ThenBy(x => x.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var assignees = await GetAssigneesByAssignedToAsync(jobs.Select(x => x.AssignedTo));

        return new PagedResult<JobDto>
        {
            Items = jobs.Select(x => MapToDto(x, todayUtc, assignees)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<JobDto?> GetByIdAsync(Guid id, IReadOnlyCollection<string>? assignedToScope = null)
    {
        var job = await ApplyAssignedToScope(BaseQuery().AsNoTracking(), assignedToScope)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return null;
        }

        var assignees = await GetAssigneesByAssignedToAsync([job.AssignedTo]);

        return MapToDto(job, DateTime.UtcNow.Date, assignees);
    }

    public async Task<JobDto> CreateAsync(CreateJobDto request)
    {
        await ValidateAssetAndTestResultAsync(request.AssetId, request.TestResultId);

        var nowUtc = DateTime.UtcNow;

        var job = new Job
        {
            Id = Guid.NewGuid(),
            AssetId = request.AssetId,
            TestResultId = request.TestResultId,
            Title = NormalizeRequiredText(request.Title, "Job title is required."),
            Description = NormalizeOptionalText(request.Description),
            JobType = NormalizeChoice(request.JobType, JobTypeConstants.Inspection, JobTypeConstants.All, "jobType"),
            Priority = NormalizeChoice(request.Priority, JobPriorityConstants.Medium, JobPriorityConstants.All, "priority"),
            Status = JobStatusConstants.Scheduled,
            AssignedTo = NormalizeOptionalText(request.AssignedTo),
            ScheduledAtUtc = request.ScheduledAtUtc ?? nowUtc,
            DueAtUtc = request.DueAtUtc,
            Notes = NormalizeOptionalText(request.Notes),
            CreatedAtUtc = nowUtc
        };

        ValidateJobDates(job.ScheduledAtUtc, job.DueAtUtc);

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(job.Id))!;
    }

    public async Task<JobDto?> UpdateAsync(Guid id, UpdateJobDto request)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return null;
        }

        EnsureOpenForModification(job);
        await ValidateAssetAndTestResultAsync(request.AssetId, request.TestResultId);
        ValidateJobDates(request.ScheduledAtUtc, request.DueAtUtc);

        job.AssetId = request.AssetId;
        job.TestResultId = request.TestResultId;
        job.Title = NormalizeRequiredText(request.Title, "Job title is required.");
        job.Description = NormalizeOptionalText(request.Description);
        job.JobType = NormalizeChoice(request.JobType, JobTypeConstants.Inspection, JobTypeConstants.All, "jobType");
        job.Priority = NormalizeChoice(request.Priority, JobPriorityConstants.Medium, JobPriorityConstants.All, "priority");
        job.AssignedTo = NormalizeOptionalText(request.AssignedTo);
        job.ScheduledAtUtc = request.ScheduledAtUtc;
        job.DueAtUtc = request.DueAtUtc;
        job.Notes = NormalizeOptionalText(request.Notes);
        job.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(job.Id);
    }

    public async Task<JobDto?> StartAsync(Guid id)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return null;
        }

        if (job.Status != JobStatusConstants.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled jobs can be started.");
        }

        var nowUtc = DateTime.UtcNow;
        job.Status = JobStatusConstants.InProgress;
        job.StartedAtUtc ??= nowUtc;
        job.UpdatedAtUtc = nowUtc;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(job.Id);
    }

    public async Task<JobDto?> CompleteAsync(Guid id, CompleteJobDto request)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return null;
        }

        if (job.Status is not JobStatusConstants.Scheduled and not JobStatusConstants.InProgress)
        {
            throw new InvalidOperationException("Only scheduled or in-progress jobs can be completed.");
        }

        var nowUtc = DateTime.UtcNow;
        job.Status = JobStatusConstants.Completed;
        job.StartedAtUtc ??= nowUtc;
        job.CompletedAtUtc = nowUtc;
        job.CompletionNotes = NormalizeOptionalText(request.CompletionNotes);
        job.UpdatedAtUtc = nowUtc;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(job.Id);
    }

    public async Task<JobDto?> CancelAsync(Guid id, CancelJobDto request)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return null;
        }

        if (job.Status == JobStatusConstants.Completed)
        {
            throw new InvalidOperationException("Completed jobs cannot be cancelled.");
        }

        if (job.Status == JobStatusConstants.Cancelled)
        {
            throw new InvalidOperationException("Job is already cancelled.");
        }

        var nowUtc = DateTime.UtcNow;
        job.Status = JobStatusConstants.Cancelled;
        job.CancelledAtUtc = nowUtc;
        job.CancellationReason = NormalizeOptionalText(request.CancellationReason);
        job.UpdatedAtUtc = nowUtc;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(job.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return false;
        }

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        return true;
    }

    private IQueryable<Job> BaseQuery()
    {
        return _context.Jobs
            .Include(x => x.Asset)
            .ThenInclude(x => x.Site)
            .ThenInclude(x => x.Customer)
            .Include(x => x.TestResult);
    }

    private static IQueryable<Job> ApplyAssignedToScope(
        IQueryable<Job> query,
        IReadOnlyCollection<string>? assignedToScope)
    {
        if (assignedToScope is null)
        {
            return query;
        }

        var assignedToValues = assignedToScope
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return assignedToValues.Count == 0
            ? query.Where(_ => false)
            : query.Where(x => x.AssignedTo != null && assignedToValues.Contains(x.AssignedTo));
    }

    private async Task ValidateAssetAndTestResultAsync(Guid assetId, Guid? testResultId)
    {
        if (assetId == Guid.Empty)
        {
            throw new InvalidOperationException("AssetId is required.");
        }

        var assetExists = await _context.Assets
            .AnyAsync(x => x.Id == assetId && x.IsActive);

        if (!assetExists)
        {
            throw new InvalidOperationException("Active asset not found.");
        }

        if (!testResultId.HasValue)
        {
            return;
        }

        var testResultMatchesAsset = await _context.TestResults
            .AnyAsync(x =>
                x.Id == testResultId.Value &&
                x.AssetId == assetId &&
                !x.IsDeleted);

        if (!testResultMatchesAsset)
        {
            throw new InvalidOperationException("Test result was not found for this asset.");
        }
    }

    private static void EnsureOpenForModification(Job job)
    {
        if (!JobStatusConstants.Open.Contains(job.Status))
        {
            throw new InvalidOperationException("Only scheduled or in-progress jobs can be updated.");
        }
    }

    private static void ValidateJobDates(DateTime scheduledAtUtc, DateTime? dueAtUtc)
    {
        if (dueAtUtc.HasValue && dueAtUtc.Value < scheduledAtUtc)
        {
            throw new InvalidOperationException("DueAtUtc cannot be earlier than ScheduledAtUtc.");
        }
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

    private static string NormalizeChoice(string? value, string defaultValue, IReadOnlyCollection<string> allowedValues, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        var normalizedValue = value.Trim();
        var match = allowedValues.FirstOrDefault(x =>
            string.Equals(x, normalizedValue, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            throw new InvalidOperationException(
                $"Invalid {fieldName}. Allowed values: {string.Join(", ", allowedValues)}.");
        }

        return match;
    }

    private async Task<Dictionary<string, ApplicationUser>> GetAssigneesByAssignedToAsync(IEnumerable<string?> assignedToValues)
    {
        var assignedToLookup = assignedToValues
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (assignedToLookup.Count == 0)
        {
            return new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase);
        }

        var assignedToUserIds = assignedToLookup
            .Select(x => Guid.TryParse(x, out var userId) ? userId : (Guid?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        var users = await _context.Users
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                (assignedToUserIds.Contains(x.Id) ||
                    assignedToLookup.Contains(x.FullName) ||
                    assignedToLookup.Contains(x.Email!)))
            .ToListAsync();

        var assignees = new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase);

        foreach (var user in users)
        {
            assignees.TryAdd(user.Id.ToString(), user);

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                assignees.TryAdd(user.FullName.Trim(), user);
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                assignees.TryAdd(user.Email.Trim(), user);
            }
        }

        return assignees;
    }

    private static JobDto MapToDto(
        Job job,
        DateTime todayUtc,
        IReadOnlyDictionary<string, ApplicationUser> assigneesByAssignedTo)
    {
        var assignedTo = NormalizeOptionalText(job.AssignedTo);
        assigneesByAssignedTo.TryGetValue(assignedTo ?? string.Empty, out var assignedUser);

        return new JobDto
        {
            Id = job.Id,
            AssetId = job.AssetId,
            AssetName = job.Asset.Name,
            AssetTag = job.Asset.AssetTag,
            SiteId = job.Asset.SiteId,
            SiteName = job.Asset.Site.Name,
            CustomerId = job.Asset.Site.CustomerId,
            CustomerName = job.Asset.Site.Customer.Name,
            TestResultId = job.TestResultId,
            TestReference = job.TestResult?.TestReference,
            TestOverallStatus = job.TestResult?.OverallStatus,
            Title = job.Title,
            Description = job.Description,
            JobType = job.JobType,
            Priority = job.Priority,
            Status = job.Status,
            AssignedTo = assignedTo,
            AssignedToName = ResolveAssignedToName(assignedTo, assignedUser),
            AssignedToEmail = ResolveAssignedToEmail(assignedTo, assignedUser),
            ScheduledAtUtc = job.ScheduledAtUtc,
            DueAtUtc = job.DueAtUtc,
            DaysOverdue = CalculateDaysOverdue(job, todayUtc),
            StartedAtUtc = job.StartedAtUtc,
            CompletedAtUtc = job.CompletedAtUtc,
            CancelledAtUtc = job.CancelledAtUtc,
            CompletionNotes = job.CompletionNotes,
            CancellationReason = job.CancellationReason,
            Notes = job.Notes,
            CreatedAtUtc = job.CreatedAtUtc,
            UpdatedAtUtc = job.UpdatedAtUtc
        };
    }

    private static int? CalculateDaysOverdue(Job job, DateTime todayUtc)
    {
        if (!job.DueAtUtc.HasValue || !JobStatusConstants.Open.Contains(job.Status))
        {
            return null;
        }

        var days = (todayUtc - job.DueAtUtc.Value.Date).Days;

        return days > 0 ? days : null;
    }

    private static string? ResolveAssignedToName(string? assignedTo, ApplicationUser? assignedUser)
    {
        if (!string.IsNullOrWhiteSpace(assignedUser?.FullName))
        {
            return assignedUser.FullName.Trim();
        }

        return IsEmailLike(assignedTo) || IsGuidLike(assignedTo) ? null : assignedTo;
    }

    private static string? ResolveAssignedToEmail(string? assignedTo, ApplicationUser? assignedUser)
    {
        if (!string.IsNullOrWhiteSpace(assignedUser?.Email))
        {
            return assignedUser.Email.Trim();
        }

        return IsEmailLike(assignedTo) ? assignedTo : null;
    }

    private static bool IsEmailLike(string? value)
    {
        return value?.Contains('@') == true;
    }

    private static bool IsGuidLike(string? value)
    {
        return Guid.TryParse(value, out _);
    }
}
