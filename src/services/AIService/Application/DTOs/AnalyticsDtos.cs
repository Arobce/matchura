namespace AIService.Application.DTOs;

public class EmployerDashboardResponse
{
    public int TotalActiveJobs { get; set; }
    public int TotalApplications { get; set; }
    public decimal AverageMatchScore { get; set; }
    public Dictionary<string, int> PipelineBreakdown { get; set; } = new();
    public List<SkillDemand> TopSkillsInDemand { get; set; } = new();
}

public class JobAnalyticsResponse
{
    public Guid JobId { get; set; }
    public int TotalApplicants { get; set; }
    public decimal AverageMatchScore { get; set; }
    public Dictionary<string, int> ScoreDistribution { get; set; } = new();
    public Dictionary<string, int> PipelineBreakdown { get; set; } = new();
    public List<MatchScoreResponse> TopCandidates { get; set; } = new();
    public List<SkillDemand> CommonSkillGaps { get; set; } = new();
    public int DaysSincePosting { get; set; }
}

public class SkillDemand
{
    public string Skill { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TrendDataResponse
{
    public List<WeeklyTrend> ApplicationsPerWeek { get; set; } = new();
    public List<WeeklyTrend> AverageScorePerWeek { get; set; } = new();
    public List<SkillDemand> MostRequestedSkills { get; set; } = new();
}

public class WeeklyTrend
{
    public string Week { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
