namespace AIService.Domain.Entities;

public class MatchScore
{
    public Guid MatchScoreId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public decimal OverallScore { get; set; }
    public decimal SkillScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal EducationScore { get; set; }
    public string? Explanation { get; set; }
    public string? Strengths { get; set; }
    public string? Gaps { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
