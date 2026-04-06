namespace ProfileService.Application.DTOs;

public class CreateCandidateProfileRequest
{
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? ProfessionalSummary { get; set; }
    public int YearsOfExperience { get; set; }
    public string? HighestEducation { get; set; }
    public string? LinkedinUrl { get; set; }
}

public class UpdateCandidateProfileRequest
{
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? ProfessionalSummary { get; set; }
    public int YearsOfExperience { get; set; }
    public string? HighestEducation { get; set; }
    public string? LinkedinUrl { get; set; }
}

public class CandidateProfileResponse
{
    public Guid CandidateId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? ProfessionalSummary { get; set; }
    public int YearsOfExperience { get; set; }
    public string? HighestEducation { get; set; }
    public string? LinkedinUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CandidateProfilePublicResponse
{
    public Guid CandidateId { get; set; }
    public string? Location { get; set; }
    public string? ProfessionalSummary { get; set; }
    public int YearsOfExperience { get; set; }
    public string? HighestEducation { get; set; }
}
