using ApplicationService.Domain.Enums;

namespace ApplicationService.Domain.Entities;

public class JobApplication
{
    public Guid ApplicationId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public string? CoverLetter { get; set; }
    public string? CoverLetterUrl { get; set; }
    public string? ResumeUrl { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
    public string? EmployerNotes { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
