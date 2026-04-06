namespace AIService.Application.DTOs;

// ── From Claude ──

public class SkillGapResult
{
    public string Summary { get; set; } = string.Empty;
    public decimal OverallReadiness { get; set; }
    public string EstimatedTimeToReady { get; set; } = string.Empty;
    public List<MissingSkillEntry> MissingSkills { get; set; } = new();
    public List<RecommendedAction> RecommendedActions { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
}

public class MissingSkillEntry
{
    public string SkillName { get; set; } = string.Empty;
    public string Importance { get; set; } = string.Empty;
    public string? CurrentLevel { get; set; }
    public string RequiredLevel { get; set; } = string.Empty;
    public int GapSeverity { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class RecommendedAction
{
    public int Priority { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EstimatedTime { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
}

// ── API requests/responses ──

public class AnalyzeSkillGapRequest
{
    public Guid JobId { get; set; }
}

public class SkillGapReportResponse
{
    public Guid ReportId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public string? Summary { get; set; }
    public decimal OverallReadiness { get; set; }
    public string? EstimatedTimeToReady { get; set; }
    public List<MissingSkillEntry> MissingSkills { get; set; } = new();
    public List<RecommendedAction> RecommendedActions { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
