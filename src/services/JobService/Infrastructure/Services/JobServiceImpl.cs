using JobService.Application.DTOs;
using JobService.Application.Interfaces;
using JobService.Domain.Entities;
using JobService.Domain.Enums;
using JobService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Events;

namespace JobService.Infrastructure.Services;

public class JobServiceImpl : IJobService
{
    private readonly JobDbContext _db;
    private readonly ILogger<JobServiceImpl> _logger;
    private readonly IEventBus _eventBus;

    public JobServiceImpl(JobDbContext db, ILogger<JobServiceImpl> logger, IEventBus eventBus)
    {
        _db = db;
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task<JobResponse> CreateJobAsync(string employerId, CreateJobRequest request)
    {
        var job = new Job
        {
            EmployerId = employerId,
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            EmploymentType = request.EmploymentType,
            ExperienceRequired = request.ExperienceRequired,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            ApplicationDeadline = request.ApplicationDeadline.HasValue
                ? DateTime.SpecifyKind(request.ApplicationDeadline.Value, DateTimeKind.Utc)
                : null,
            JobStatus = JobStatus.Draft
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();

        if (request.Skills.Count > 0)
        {
            var skillIds = request.Skills.Select(s => s.SkillId).ToList();
            var validSkills = await _db.Skills.Where(s => skillIds.Contains(s.SkillId)).Select(s => s.SkillId).ToListAsync();

            foreach (var skillInput in request.Skills.Where(s => validSkills.Contains(s.SkillId)))
            {
                _db.JobSkills.Add(new JobSkill
                {
                    JobId = job.JobId,
                    SkillId = skillInput.SkillId,
                    ImportanceLevel = skillInput.ImportanceLevel
                });
            }
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Job {JobId} created by employer {EmployerId}", job.JobId, employerId);
        return await GetJobByIdAsync(job.JobId);
    }

    public async Task<JobResponse> UpdateJobAsync(string employerId, Guid jobId, UpdateJobRequest request)
    {
        var job = await _db.Jobs.Include(j => j.JobSkills)
            .FirstOrDefaultAsync(j => j.JobId == jobId)
            ?? throw new InvalidOperationException("Job not found");

        if (job.EmployerId != employerId)
            throw new UnauthorizedAccessException("You can only update your own jobs");

        if (job.JobStatus == JobStatus.Closed)
            throw new InvalidOperationException("Cannot update a closed job");

        job.Title = request.Title;
        job.Description = request.Description;
        job.Location = request.Location;
        job.EmploymentType = request.EmploymentType;
        job.ExperienceRequired = request.ExperienceRequired;
        job.SalaryMin = request.SalaryMin;
        job.SalaryMax = request.SalaryMax;
        job.ApplicationDeadline = request.ApplicationDeadline.HasValue
            ? DateTime.SpecifyKind(request.ApplicationDeadline.Value, DateTimeKind.Utc)
            : null;
        job.UpdatedAt = DateTime.UtcNow;

        // Replace skills
        _db.JobSkills.RemoveRange(job.JobSkills);

        if (request.Skills.Count > 0)
        {
            var skillIds = request.Skills.Select(s => s.SkillId).ToList();
            var validSkills = await _db.Skills.Where(s => skillIds.Contains(s.SkillId)).Select(s => s.SkillId).ToListAsync();

            foreach (var skillInput in request.Skills.Where(s => validSkills.Contains(s.SkillId)))
            {
                _db.JobSkills.Add(new JobSkill
                {
                    JobId = job.JobId,
                    SkillId = skillInput.SkillId,
                    ImportanceLevel = skillInput.ImportanceLevel
                });
            }
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Job {JobId} updated", jobId);
        return await GetJobByIdAsync(jobId);
    }

    public async Task<JobResponse> UpdateJobStatusAsync(string employerId, Guid jobId, UpdateJobStatusRequest request)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId)
            ?? throw new InvalidOperationException("Job not found");

        if (job.EmployerId != employerId)
            throw new UnauthorizedAccessException("You can only update your own jobs");

        // Validate transitions
        var valid = (job.JobStatus, request.Status) switch
        {
            (JobStatus.Draft, JobStatus.Active) => true,
            (JobStatus.Active, JobStatus.Closed) => true,
            (JobStatus.Draft, JobStatus.Closed) => true,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException($"Cannot transition from {job.JobStatus} to {request.Status}");

        job.JobStatus = request.Status;
        if (request.Status == JobStatus.Active)
            job.PostedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (request.Status == JobStatus.Active)
        {
            await _eventBus.PublishAsync(new JobPublishedEvent
            {
                JobId = jobId,
                EmployerId = employerId,
                Title = job.Title,
                OccurredAt = DateTime.UtcNow
            });
        }

        _logger.LogInformation("Job {JobId} status changed to {Status}", jobId, request.Status);
        return await GetJobByIdAsync(jobId);
    }

    public async Task DeleteJobAsync(string employerId, Guid jobId)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId)
            ?? throw new InvalidOperationException("Job not found");

        if (job.EmployerId != employerId)
            throw new UnauthorizedAccessException("You can only delete your own jobs");

        job.JobStatus = JobStatus.Closed;
        job.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Job {JobId} soft-deleted (closed)", jobId);
    }

    public async Task<JobResponse> GetJobByIdAsync(Guid jobId)
    {
        var job = await _db.Jobs
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.JobId == jobId)
            ?? throw new InvalidOperationException("Job not found");

        return MapToResponse(job);
    }

    public async Task<JobListResponse> GetJobsAsync(JobQueryParams q)
    {
        var query = _db.Jobs
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Where(j => j.JobStatus == JobStatus.Active)
            .AsQueryable();

        // Filter by employer
        if (!string.IsNullOrWhiteSpace(q.EmployerId))
            query = query.Where(j => j.EmployerId == q.EmployerId);

        // Search in title and description
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(search) ||
                j.Description.ToLower().Contains(search));
        }

        // Filter by location
        if (!string.IsNullOrWhiteSpace(q.Location))
            query = query.Where(j => j.Location != null && j.Location.ToLower().Contains(q.Location.ToLower()));

        // Filter by employment type
        if (q.EmploymentType.HasValue)
            query = query.Where(j => j.EmploymentType == q.EmploymentType.Value);

        // Filter by salary range
        if (q.MinSalary.HasValue)
            query = query.Where(j => j.SalaryMax >= q.MinSalary.Value || j.SalaryMax == null);

        if (q.MaxSalary.HasValue)
            query = query.Where(j => j.SalaryMin <= q.MaxSalary.Value || j.SalaryMin == null);

        // Filter by skills
        if (!string.IsNullOrWhiteSpace(q.Skills))
        {
            var skillNames = q.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(j => j.JobSkills.Any(js =>
                skillNames.Contains(js.Skill.SkillName)));
        }

        var totalCount = await query.CountAsync();

        // Sorting
        query = q.SortBy?.ToLower() switch
        {
            "salary" => query.OrderByDescending(j => j.SalaryMax ?? 0),
            "experience" => query.OrderBy(j => j.ExperienceRequired),
            "title" => query.OrderBy(j => j.Title),
            _ => query.OrderByDescending(j => j.PostedAt) // default: newest first
        };

        // Pagination
        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 50);

        var jobs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new JobListResponse
        {
            Items = jobs.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<JobListResponse> GetMyJobsAsync(string employerId, int page, int pageSize)
    {
        var query = _db.Jobs
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Where(j => j.EmployerId == employerId)
            .OrderByDescending(j => j.CreatedAt);

        var totalCount = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var jobs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new JobListResponse
        {
            Items = jobs.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    // ── Skills ──

    public async Task<List<SkillResponse>> GetSkillsAsync(string? category)
    {
        var query = _db.Skills.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(s => s.SkillCategory == category);

        var skills = await query.OrderBy(s => s.SkillName).ToListAsync();
        return skills.Select(s => new SkillResponse
        {
            SkillId = s.SkillId,
            SkillName = s.SkillName,
            SkillCategory = s.SkillCategory
        }).ToList();
    }

    public async Task<SkillResponse> CreateSkillAsync(CreateSkillRequest request)
    {
        var existing = await _db.Skills.FirstOrDefaultAsync(s => s.SkillName == request.SkillName);
        if (existing != null)
            throw new InvalidOperationException("Skill already exists");

        var skill = new Skill
        {
            SkillName = request.SkillName,
            SkillCategory = request.SkillCategory
        };

        _db.Skills.Add(skill);
        await _db.SaveChangesAsync();

        return new SkillResponse
        {
            SkillId = skill.SkillId,
            SkillName = skill.SkillName,
            SkillCategory = skill.SkillCategory
        };
    }

    public async Task<List<string>> GetSkillCategoriesAsync()
    {
        return await _db.Skills
            .Where(s => s.SkillCategory != null)
            .Select(s => s.SkillCategory!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    private static JobResponse MapToResponse(Job job) => new()
    {
        JobId = job.JobId,
        EmployerId = job.EmployerId,
        Title = job.Title,
        Description = job.Description,
        Location = job.Location,
        EmploymentType = job.EmploymentType.ToString(),
        ExperienceRequired = job.ExperienceRequired,
        SalaryMin = job.SalaryMin,
        SalaryMax = job.SalaryMax,
        JobStatus = job.JobStatus.ToString(),
        PostedAt = job.PostedAt,
        ApplicationDeadline = job.ApplicationDeadline,
        CreatedAt = job.CreatedAt,
        UpdatedAt = job.UpdatedAt,
        Skills = job.JobSkills.Select(js => new JobSkillResponse
        {
            SkillId = js.SkillId,
            SkillName = js.Skill.SkillName,
            SkillCategory = js.Skill.SkillCategory,
            ImportanceLevel = js.ImportanceLevel.ToString()
        }).ToList()
    };
}
