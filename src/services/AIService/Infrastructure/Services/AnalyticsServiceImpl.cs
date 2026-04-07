using System.Text.Json;
using AIService.Application.DTOs;
using AIService.Application.Interfaces;
using AIService.Domain.Entities;
using AIService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIService.Infrastructure.Services;

public class AnalyticsServiceImpl : IAnalyticsService
{
    private readonly AIDbContext _db;
    private readonly ICacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AnalyticsServiceImpl> _logger;

    public AnalyticsServiceImpl(
        AIDbContext db,
        ICacheService cache,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AnalyticsServiceImpl> logger)
    {
        _db = db;
        _cache = cache;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<EmployerDashboardResponse> GetDashboardAsync(string employerId)
    {
        var cacheKey = $"dashboard:{employerId}";
        var cached = await _cache.GetAsync<EmployerDashboardResponse>(cacheKey);
        if (cached != null) return cached;

        // Get employer's jobs from JobService (filtered by employerId)
        var (jobIds, jobCount) = await FetchEmployerJobsAsync(employerId);

        var matchScores = await _db.MatchScores
            .Where(m => jobIds.Contains(m.JobId))
            .ToListAsync();

        // Get application pipeline from ApplicationService (internal, no auth)
        var (totalApps, pipelineBreakdown) = await FetchApplicationPipelineAsync(jobIds);

        // Get skill demand from job skills via JobService
        var topSkills = await FetchJobSkillDemandAsync(employerId);

        var dashboard = new EmployerDashboardResponse
        {
            TotalActiveJobs = jobCount,
            TotalApplications = totalApps,
            AverageMatchScore = matchScores.Count > 0
                ? Math.Round(matchScores.Average(m => m.OverallScore), 1)
                : 0,
            PipelineBreakdown = pipelineBreakdown,
            TopSkillsInDemand = topSkills
        };

        await _cache.SetAsync(cacheKey, dashboard, TimeSpan.FromMinutes(5));
        return dashboard;
    }

    public async Task<JobAnalyticsResponse> GetJobAnalyticsAsync(string employerId, Guid jobId)
    {
        var cacheKey = $"job-analytics:{employerId}:{jobId}";
        var cached = await _cache.GetAsync<JobAnalyticsResponse>(cacheKey);
        if (cached != null) return cached;

        // Verify ownership
        var jobJson = await FetchJobDataAsync(jobId);
        using var doc = JsonDocument.Parse(jobJson);
        var jobEmployerId = doc.RootElement.GetProperty("employerId").GetString();
        if (jobEmployerId != employerId)
            throw new UnauthorizedAccessException("You can only view analytics for your own jobs");

        var postedAt = doc.RootElement.TryGetProperty("createdAt", out var createdEl)
            ? createdEl.GetDateTime()
            : DateTime.UtcNow;

        var matchScores = await _db.MatchScores
            .Where(m => m.JobId == jobId)
            .OrderByDescending(m => m.OverallScore)
            .ToListAsync();

        var scoreDistribution = new Dictionary<string, int>
        {
            ["90-100"] = matchScores.Count(m => m.OverallScore >= 90),
            ["75-89"] = matchScores.Count(m => m.OverallScore >= 75 && m.OverallScore < 90),
            ["60-74"] = matchScores.Count(m => m.OverallScore >= 60 && m.OverallScore < 75),
            ["40-59"] = matchScores.Count(m => m.OverallScore >= 40 && m.OverallScore < 60),
            ["0-39"] = matchScores.Count(m => m.OverallScore < 40)
        };

        var skillGapReports = await _db.SkillGapReports
            .Where(r => r.JobId == jobId && r.MissingSkills != null)
            .ToListAsync();

        var commonGaps = skillGapReports
            .SelectMany(r =>
            {
                try
                {
                    return JsonSerializer.Deserialize<List<MissingSkillEntry>>(r.MissingSkills!,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new();
                }
                catch { return new List<MissingSkillEntry>(); }
            })
            .GroupBy(s => s.SkillName)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new SkillDemand { Skill = g.Key, Count = g.Count() })
            .ToList();

        var analytics = new JobAnalyticsResponse
        {
            JobId = jobId,
            TotalApplicants = matchScores.Count,
            AverageMatchScore = matchScores.Count > 0
                ? Math.Round(matchScores.Average(m => m.OverallScore), 1)
                : 0,
            ScoreDistribution = scoreDistribution,
            PipelineBreakdown = new Dictionary<string, int>(),
            TopCandidates = matchScores.Take(5).Select(MapMatchToResponse).ToList(),
            CommonSkillGaps = commonGaps,
            DaysSincePosting = (int)(DateTime.UtcNow - postedAt).TotalDays
        };

        await _cache.SetAsync(cacheKey, analytics, TimeSpan.FromMinutes(15));
        return analytics;
    }

    public async Task<TrendDataResponse> GetTrendsAsync(string employerId)
    {
        var cacheKey = $"trends:{employerId}";
        var cached = await _cache.GetAsync<TrendDataResponse>(cacheKey);
        if (cached != null) return cached;

        var (jobIds, _) = await FetchEmployerJobsAsync(employerId);

        var fourWeeksAgo = DateTime.UtcNow.AddDays(-28);
        var matchScores = await _db.MatchScores
            .Where(m => jobIds.Contains(m.JobId) && m.GeneratedAt >= fourWeeksAgo)
            .ToListAsync();

        var weeklyGroups = matchScores
            .GroupBy(m =>
            {
                var diff = (DateTime.UtcNow - m.GeneratedAt).Days;
                return $"Week {diff / 7 + 1}";
            })
            .OrderBy(g => g.Key);

        var trends = new TrendDataResponse
        {
            ApplicationsPerWeek = weeklyGroups
                .Select(g => new WeeklyTrend { Week = g.Key, Value = g.Count() })
                .ToList(),
            AverageScorePerWeek = weeklyGroups
                .Select(g => new WeeklyTrend { Week = g.Key, Value = Math.Round(g.Average(m => m.OverallScore), 1) })
                .ToList(),
            MostRequestedSkills = await FetchJobSkillDemandAsync(employerId)
        };

        await _cache.SetAsync(cacheKey, trends, TimeSpan.FromMinutes(15));
        return trends;
    }

    private async Task<(List<Guid> JobIds, int Count)> FetchEmployerJobsAsync(string employerId)
    {
        var jobServiceUrl = _configuration["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        try
        {
            var response = await _httpClient.GetAsync($"{jobServiceUrl}/api/jobs?employerId={employerId}&pageSize=100");
            if (!response.IsSuccessStatusCode) return (new(), 0);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var totalCount = doc.RootElement.TryGetProperty("totalCount", out var tc) ? tc.GetInt32() : 0;

            if (doc.RootElement.TryGetProperty("items", out var items))
            {
                var ids = items.EnumerateArray()
                    .Select(j => j.GetProperty("jobId").GetGuid())
                    .ToList();
                return (ids, totalCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch employer jobs for {EmployerId}", employerId);
        }

        return (new(), 0);
    }

    private async Task<(int Total, Dictionary<string, int> ByStatus)> FetchApplicationPipelineAsync(List<Guid> jobIds)
    {
        var appServiceUrl = _configuration["APPLICATION_SERVICE_URL"] ?? "http://application-service:8080";
        var total = 0;
        var byStatus = new Dictionary<string, int>();

        try
        {
            foreach (var jobId in jobIds.Take(20))
            {
                var response = await _httpClient.GetAsync($"{appServiceUrl}/internal/applications/job/{jobId}");
                if (!response.IsSuccessStatusCode) continue;

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var app in doc.RootElement.EnumerateArray())
                    {
                        total++;
                        var status = app.TryGetProperty("status", out var s) ? s.GetString() ?? "Unknown" : "Unknown";
                        byStatus[status] = byStatus.GetValueOrDefault(status) + 1;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch application pipeline");
        }

        return (total, byStatus);
    }

    private async Task<List<SkillDemand>> FetchJobSkillDemandAsync(string employerId)
    {
        var jobServiceUrl = _configuration["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        var skillCounts = new Dictionary<string, int>();

        try
        {
            var response = await _httpClient.GetAsync($"{jobServiceUrl}/api/jobs?employerId={employerId}&pageSize=100");
            if (!response.IsSuccessStatusCode) return new();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            if (doc.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var job in items.EnumerateArray())
                {
                    if (job.TryGetProperty("skills", out var skills))
                    {
                        foreach (var skill in skills.EnumerateArray())
                        {
                            var name = skill.TryGetProperty("skillName", out var n) ? n.GetString() ?? ""
                                     : skill.TryGetProperty("name", out var n2) ? n2.GetString() ?? "" : "";
                            if (!string.IsNullOrEmpty(name))
                                skillCounts[name] = skillCounts.GetValueOrDefault(name) + 1;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch job skills for {EmployerId}", employerId);
        }

        return skillCounts
            .OrderByDescending(k => k.Value)
            .Take(10)
            .Select(k => new SkillDemand { Skill = k.Key, Count = k.Value })
            .ToList();
    }

    private static MatchScoreResponse MapMatchToResponse(MatchScore m) => new()
    {
        MatchScoreId = m.MatchScoreId,
        CandidateId = m.CandidateId,
        JobId = m.JobId,
        OverallScore = m.OverallScore,
        SkillScore = m.SkillScore,
        ExperienceScore = m.ExperienceScore,
        EducationScore = m.EducationScore,
        Explanation = m.Explanation,
        Strengths = string.IsNullOrEmpty(m.Strengths) ? new()
            : JsonSerializer.Deserialize<List<string>>(m.Strengths) ?? new(),
        Gaps = string.IsNullOrEmpty(m.Gaps) ? new()
            : JsonSerializer.Deserialize<List<string>>(m.Gaps) ?? new(),
        GeneratedAt = m.GeneratedAt
    };

    private async Task<string> FetchJobDataAsync(Guid jobId)
    {
        var jobServiceUrl = _configuration["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        var response = await _httpClient.GetAsync($"{jobServiceUrl}/api/jobs/{jobId}");

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Job {jobId} not found or job service unavailable");

        return await response.Content.ReadAsStringAsync();
    }
}
