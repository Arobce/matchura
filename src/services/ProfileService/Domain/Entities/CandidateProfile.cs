namespace ProfileService.Domain.Entities;

public class CandidateProfile
{
    public Guid CandidateId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? ProfessionalSummary { get; set; }
    public int YearsOfExperience { get; set; }
    public string? HighestEducation { get; set; }
    public string? LinkedinUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
