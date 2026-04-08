using System.Text.Json.Serialization;

namespace AIService.Application.DTOs;

// ── Parsed resume data (from Claude) ──

public class ParsedResumeData
{
    public PersonalInfo? PersonalInfo { get; set; }
    public string? Summary { get; set; }
    public List<ExperienceEntry> Experience { get; set; } = new();
    public List<EducationEntry> Education { get; set; } = new();
    public List<SkillEntry> Skills { get; set; } = new();
    public List<CertificationEntry> Certifications { get; set; } = new();
    public List<ProjectEntry> Projects { get; set; } = new();
}

public class PersonalInfo
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
}

public class ExperienceEntry
{
    public string Company { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new();
}

public class EducationEntry
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string? GraduationDate { get; set; }
    public decimal? Gpa { get; set; }
}

public class SkillEntry
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ProficiencyLevel { get; set; } = "Intermediate";
    public int? YearsUsed { get; set; }
}

public class CertificationEntry
{
    public string Name { get; set; } = string.Empty;
    public string? Issuer { get; set; }
    public string? Date { get; set; }
}

public class ProjectEntry
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Technologies { get; set; } = new();
}

// ── API responses ──

public class ResumeResponse
{
    public Guid ResumeId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public ParsedResumeData? ParsedData { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ParsedAt { get; set; }
}

public class DocumentUploadResponse
{
    public string FileUrl { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
}

public class ResumeStatusResponse
{
    public Guid ResumeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class ResumeUploadResponse
{
    public Guid ResumeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
