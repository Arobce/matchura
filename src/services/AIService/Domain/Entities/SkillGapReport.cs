namespace AIService.Domain.Entities;

public class SkillGapReport
{
    public Guid ReportId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public string? Summary { get; set; }
    public decimal OverallReadiness { get; set; }
    public string? EstimatedTimeToReady { get; set; }
    public string? MissingSkills { get; set; }
    public string? RecommendedActions { get; set; }
    public string? StrengthAreas { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
