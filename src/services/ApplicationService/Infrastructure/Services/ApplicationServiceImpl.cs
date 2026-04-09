using System.Net.Http.Headers;
using System.Text.Json;
using ApplicationService.Application.DTOs;
using ApplicationService.Application.Interfaces;
using ApplicationService.Domain.Entities;
using ApplicationService.Domain.Enums;
using ApplicationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedKernel.Events;

namespace ApplicationService.Infrastructure.Services;

public class ApplicationServiceImpl : IApplicationService
{
    private readonly ApplicationDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationServiceImpl> _logger;
    private readonly IEventBus _eventBus;

    public ApplicationServiceImpl(
        ApplicationDbContext db,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ApplicationServiceImpl> logger,
        IEventBus eventBus)
    {
        _db = db;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task<ApplicationResponse> CreateApplicationAsync(string candidateId, string? candidateName, CreateApplicationRequest request)
    {
        // Check for duplicate application
        var existing = await _db.Applications
            .FirstOrDefaultAsync(a => a.CandidateId == candidateId && a.JobId == request.JobId);
        if (existing != null)
            throw new InvalidOperationException("You have already applied to this job");

        var jobTitle = await GetJobTitleAsync(request.JobId);

        var application = new JobApplication
        {
            CandidateId = candidateId,
            CandidateName = candidateName,
            JobId = request.JobId,
            JobTitle = jobTitle,
            CoverLetter = request.CoverLetter,
            CoverLetterUrl = request.CoverLetterUrl,
            ResumeUrl = request.ResumeUrl,
            Status = ApplicationStatus.Submitted
        };

        _db.Applications.Add(application);
        await _db.SaveChangesAsync();

        await _eventBus.PublishAsync(new ApplicationSubmittedEvent
        {
            ApplicationId = application.ApplicationId,
            CandidateId = candidateId,
            JobId = request.JobId,
            OccurredAt = DateTime.UtcNow
        });

        _logger.LogInformation("Application {Id} created by candidate {CandidateId} for job {JobId}",
            application.ApplicationId, candidateId, request.JobId);

        return MapToResponse(application);
    }

    public async Task<ApplicationListResponse> GetMyApplicationsAsync(string candidateId, int page, int pageSize)
    {
        var query = _db.Applications
            .Where(a => a.CandidateId == candidateId)
            .OrderByDescending(a => a.AppliedAt);

        var totalCount = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ApplicationListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<ApplicationResponse> GetApplicationByIdAsync(Guid applicationId, string userId, string role)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId)
            ?? throw new InvalidOperationException("Application not found");

        // Candidates can only see their own applications
        if (role == "Candidate" && application.CandidateId != userId)
            throw new UnauthorizedAccessException("You can only view your own applications");

        // Employers can see applications for their jobs (verified via job ownership)
        if (role == "Employer")
        {
            var isOwner = await VerifyJobOwnershipAsync(application.JobId, userId);
            if (!isOwner)
                throw new UnauthorizedAccessException("You can only view applications for your own jobs");
        }

        return MapToResponse(application);
    }

    public async Task<ApplicationResponse> WithdrawApplicationAsync(string candidateId, Guid applicationId)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId)
            ?? throw new InvalidOperationException("Application not found");

        if (application.CandidateId != candidateId)
            throw new UnauthorizedAccessException("You can only withdraw your own applications");

        if (application.Status == ApplicationStatus.Withdrawn)
            throw new InvalidOperationException("Application is already withdrawn");

        if (application.Status == ApplicationStatus.Accepted || application.Status == ApplicationStatus.Rejected)
            throw new InvalidOperationException($"Cannot withdraw a {application.Status} application");

        application.Status = ApplicationStatus.Withdrawn;
        application.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _eventBus.PublishAsync(new ApplicationWithdrawnEvent
        {
            ApplicationId = applicationId,
            CandidateId = candidateId,
            JobId = application.JobId,
            OccurredAt = DateTime.UtcNow
        });

        _logger.LogInformation("Application {Id} withdrawn by candidate", applicationId);
        return MapToResponse(application);
    }

    public async Task<ApplicationListResponse> GetApplicationsForJobAsync(string employerId, Guid jobId, int page, int pageSize)
    {
        var isOwner = await VerifyJobOwnershipAsync(jobId, employerId);
        if (!isOwner)
            throw new UnauthorizedAccessException("You can only view applications for your own jobs");

        var query = _db.Applications
            .Where(a => a.JobId == jobId && a.Status != ApplicationStatus.Withdrawn)
            .OrderByDescending(a => a.AppliedAt);

        var totalCount = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ApplicationListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<ApplicationResponse> UpdateApplicationStatusAsync(string employerId, Guid applicationId, UpdateApplicationStatusRequest request)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId)
            ?? throw new InvalidOperationException("Application not found");

        var isOwner = await VerifyJobOwnershipAsync(application.JobId, employerId);
        if (!isOwner)
            throw new UnauthorizedAccessException("You can only update applications for your own jobs");

        if (application.Status == ApplicationStatus.Withdrawn)
            throw new InvalidOperationException("Cannot update a withdrawn application");

        // Validate status transitions
        var valid = (application.Status, request.Status) switch
        {
            (ApplicationStatus.Submitted, ApplicationStatus.Reviewed) => true,
            (ApplicationStatus.Reviewed, ApplicationStatus.Shortlisted) => true,
            (ApplicationStatus.Reviewed, ApplicationStatus.Rejected) => true,
            (ApplicationStatus.Shortlisted, ApplicationStatus.Accepted) => true,
            (ApplicationStatus.Shortlisted, ApplicationStatus.Rejected) => true,
            (ApplicationStatus.Submitted, ApplicationStatus.Rejected) => true,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException($"Cannot transition from {application.Status} to {request.Status}");

        var oldStatus = application.Status;
        application.Status = request.Status;
        application.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _eventBus.PublishAsync(new ApplicationStatusChangedEvent
        {
            ApplicationId = applicationId,
            CandidateId = application.CandidateId,
            JobId = application.JobId,
            OldStatus = oldStatus.ToString(),
            NewStatus = request.Status.ToString(),
            OccurredAt = DateTime.UtcNow
        });

        _logger.LogInformation("Application {Id} status updated to {Status}", applicationId, request.Status);
        return MapToResponse(application);
    }

    public async Task<ApplicationResponse> UpdateEmployerNotesAsync(string employerId, Guid applicationId, UpdateEmployerNotesRequest request)
    {
        var application = await _db.Applications.FirstOrDefaultAsync(a => a.ApplicationId == applicationId)
            ?? throw new InvalidOperationException("Application not found");

        var isOwner = await VerifyJobOwnershipAsync(application.JobId, employerId);
        if (!isOwner)
            throw new UnauthorizedAccessException("You can only add notes to applications for your own jobs");

        application.EmployerNotes = request.Notes;
        application.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Employer notes updated for application {Id}", applicationId);
        return MapToResponse(application);
    }

    private async Task<string?> GetJobTitleAsync(Guid jobId)
    {
        var jobServiceUrl = _configuration["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        try
        {
            var response = await _httpClient.GetAsync($"{jobServiceUrl}/api/jobs/{jobId}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("title").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch job title for job {JobId}", jobId);
            return null;
        }
    }

    private async Task<bool> VerifyJobOwnershipAsync(Guid jobId, string employerId)
    {
        var jobServiceUrl = _configuration["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        try
        {
            var response = await _httpClient.GetAsync($"{jobServiceUrl}/api/jobs/{jobId}");
            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var jobEmployerId = doc.RootElement.GetProperty("employerId").GetString();
            return jobEmployerId == employerId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to verify job ownership for job {JobId}", jobId);
            return false;
        }
    }

    private static ApplicationResponse MapToResponse(JobApplication a) => new()
    {
        ApplicationId = a.ApplicationId,
        CandidateId = a.CandidateId,
        CandidateName = a.CandidateName,
        JobId = a.JobId,
        JobTitle = a.JobTitle,
        CoverLetter = a.CoverLetter,
        CoverLetterUrl = a.CoverLetterUrl,
        ResumeUrl = a.ResumeUrl,
        Status = a.Status.ToString(),
        EmployerNotes = a.EmployerNotes,
        AppliedAt = a.AppliedAt,
        UpdatedAt = a.UpdatedAt
    };
}
