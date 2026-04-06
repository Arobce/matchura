using ApplicationService.Domain.Enums;

namespace ApplicationService.Application.DTOs;

public class CreateApplicationRequest
{
    public Guid JobId { get; set; }
    public string? CoverLetter { get; set; }
    public string? ResumeUrl { get; set; }
}

public class UpdateApplicationStatusRequest
{
    public ApplicationStatus Status { get; set; }
}

public class UpdateEmployerNotesRequest
{
    public string Notes { get; set; } = string.Empty;
}

public class ApplicationResponse
{
    public Guid ApplicationId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public string? CoverLetter { get; set; }
    public string? ResumeUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? EmployerNotes { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ApplicationListResponse
{
    public List<ApplicationResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
