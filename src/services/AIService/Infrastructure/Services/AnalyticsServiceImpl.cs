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

        // Get employer's jobs from JobService
        var jobIds = await FetchEmployerJobIdsAsync(employerId);

        var matchScores = await _db.MatchScores
            .Where(m => jobIds.Contains(m.JobId))
            .ToListAsync();

        // Get application counts from ApplicationService
        var applicationCounts = await FetchApplicationCountsAsync(employerId);

        var dashboard = new EmployerDashboardResponse
        {
            TotalActiveJobs = jobIds.Count,
            TotalApplications = applicationCounts.Total,
            AverageMatchScore = matchScores.Count > 0
                ? Math.Round(matchScores.Average(m => m.OverallScore), 1)
                : 0,
            PipelineBreakdown = applicationCounts.ByStatus,
            TopSkillsInDemand = await GetTopSkillsInDemandAsync(jobIds)
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

        // Common skill gaps from SkillGapReports
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

        var jobIds = await FetchEmployerJobIdsAsync(employerId);

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
            MostRequestedSkills = await GetTopSkillsInDemandAsync(jobIds)
        };

        await _cache.SetAsync(cacheKey, trends, TimeSpan.FromMinutes(15));
        return trends;
    }

    private async Task<List<Guid>> FetchEmployerJobIdsAsync(string employerId)
    {
        var jobServiceUrl = _configuration["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        try
        {
            var response = await _httpClient.GetAsync($"{jobServiceUrl}/api/jobs?employerId={employerId}&pageSize=100");
            if (!response.IsSuccessStatusCode) return new();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            if (doc.RootElement.TryGetProperty("items", out var items))
            {
                return items.EnumerateArray()
                    .Select(j => j.GetProperty("jobId").GetGuid())
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch employer jobs for {EmployerId}", employerId);
        }

        return new();
    }

    private async Task<(int Total, Dictionary<string, int> ByStatus)> FetchApplicationCountsAsync(string employerId)
    {
        var appServiceUrl = _configuration["APPLICATION_SERVICE_URL"] ?? "http://application-service:8080";
        try
        {
            var jobIds = await FetchEmployerJobIdsAsync(employerId);
            var total = 0;
            var byStatus = new Dictionary<string, int>();

            foreach (var jobId in jobIds.Take(20))
            {
                var response = await _httpClient.GetAsync($"{appServiceUrl}/api/applications/job/{jobId}?pageSize=1");
                if (!response.IsSuccessStatusCode) continue;

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("totalCount", out var count))
                    total += count.GetInt32();
            }

            return (total, byStatus);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch application counts for {EmployerId}", employerId);
            return (0, new Dictionary<string, int>());
        }
    }

    private async Task<List<SkillDemand>> GetTopSkillsInDemandAsync(List<Guid> jobIds)
    {
        var skills = await _db.CandidateSkills
            .Where(s => s.Source == "resume_parse")
            .GroupBy(s => s.SkillName)
            .Select(g => new SkillDemand { Skill = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .Take(10)
            .ToListAsync();

        return skills;
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
