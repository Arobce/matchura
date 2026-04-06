namespace AIService.Application.DTOs;

// ── From Claude ──

public class MatchResult
{
    public decimal OverallScore { get; set; }
    public decimal SkillScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal EducationScore { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Gaps { get; set; } = new();
}

// ── API requests/responses ──

public class ComputeMatchRequest
{
    public Guid JobId { get; set; }
}

public class MatchScoreResponse
{
    public Guid MatchScoreId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public decimal OverallScore { get; set; }
    public decimal SkillScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal EducationScore { get; set; }
    public string? Explanation { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Gaps { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class MatchListResponse
{
    public List<MatchScoreResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
