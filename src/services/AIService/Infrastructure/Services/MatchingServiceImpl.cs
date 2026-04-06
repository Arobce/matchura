using System.Text.Json;
using AIService.Agents;
using AIService.Application.DTOs;
using AIService.Application.Interfaces;
using AIService.Domain.Entities;
using AIService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIService.Infrastructure.Services;

public class MatchingServiceImpl : IMatchingService
{
    private readonly AIDbContext _db;
    private readonly JobMatcherAgent _agent;
    private readonly ICacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MatchingServiceImpl> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public MatchingServiceImpl(
        AIDbContext db,
        JobMatcherAgent agent,
        ICacheService cache,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MatchingServiceImpl> logger)
    {
        _db = db;
        _agent = agent;
        _cache = cache;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MatchScoreResponse> ComputeMatchAsync(string candidateId, ComputeMatchRequest request)
    {
        var cacheKey = $"match:{candidateId}:{request.JobId}";
        var cached = await _cache.GetAsync<MatchScoreResponse>(cacheKey);
        if (cached != null) return cached;

        // Get candidate data (parsed resume + skills)
        var candidateData = await BuildCandidateDataAsync(candidateId);

        // Get job data from JobService
        var jobData = await FetchJobDataAsync(request.JobId);

        // Run AI agent
        var result = await _agent.ComputeMatchAsync(candidateData, jobData);

        // Upsert match score
        var existing = await _db.MatchScores
            .FirstOrDefaultAsync(m => m.CandidateId == candidateId && m.JobId == request.JobId);

        if (existing != null)
        {
            existing.OverallScore = result.OverallScore;
            existing.SkillScore = result.SkillScore;
            existing.ExperienceScore = result.ExperienceScore;
            existing.EducationScore = result.EducationScore;
            existing.Explanation = result.Explanation;
            existing.Strengths = JsonSerializer.Serialize(result.Strengths, JsonOpts);
            existing.Gaps = JsonSerializer.Serialize(result.Gaps, JsonOpts);
            existing.GeneratedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new MatchScore
            {
                CandidateId = candidateId,
                JobId = request.JobId,
                OverallScore = result.OverallScore,
                SkillScore = result.SkillScore,
                ExperienceScore = result.ExperienceScore,
                EducationScore = result.EducationScore,
                Explanation = result.Explanation,
                Strengths = JsonSerializer.Serialize(result.Strengths, JsonOpts),
                Gaps = JsonSerializer.Serialize(result.Gaps, JsonOpts)
            };
            _db.MatchScores.Add(existing);
        }

        await _db.SaveChangesAsync();

        var response = MapToResponse(existing);
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(24));

        _logger.LogInformation("Match computed: candidate {CandidateId} × job {JobId} = {Score}",
            candidateId, request.JobId, result.OverallScore);

        return response;
    }

    public async Task<MatchListResponse> GetMatchesForJobAsync(string employerId, Guid jobId, int page, int pageSize)
    {
        // Verify employer owns the job
        var jobData = await FetchJobDataAsync(jobId);
        using var doc = JsonDocument.Parse(jobData);
        var jobEmployerId = doc.RootElement.GetProperty("employerId").GetString();
        if (jobEmployerId != employerId)
            throw new UnauthorizedAccessException("You can only view matches for your own jobs");

        var query = _db.MatchScores
            .Where(m => m.JobId == jobId)
            .OrderByDescending(m => m.OverallScore);

        var totalCount = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new MatchListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<MatchListResponse> GetRecommendedJobsAsync(string candidateId, int page, int pageSize)
    {
        var query = _db.MatchScores
            .Where(m => m.CandidateId == candidateId)
            .OrderByDescending(m => m.OverallScore);

        var totalCount = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new MatchListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    private async Task<string> BuildCandidateDataAsync(string candidateId)
    {
        var latestResume = await _db.Resumes
            .Where(r => r.CandidateId == candidateId && r.ParsedData != null)
            .OrderByDescending(r => r.ParsedAt)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No parsed resume found. Please upload and wait for parsing to complete.");

        var skills = await _db.CandidateSkills
            .Where(s => s.CandidateId == candidateId)
            .ToListAsync();

        return JsonSerializer.Serialize(new
        {
            parsedResume = latestResume.ParsedData,
            skills = skills.Select(s => new
            {
                s.SkillName,
                s.SkillCategory,
                proficiency = s.ProficiencyLevel.ToString(),
                s.YearsUsed
            })
        }, JsonOpts);
    }

    private async Task<string> FetchJobDataAsync(Guid jobId)
    {
        var jobServiceUrl = _configuration["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        var response = await _httpClient.GetAsync($"{jobServiceUrl}/api/jobs/{jobId}");

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Job {jobId} not found or job service unavailable");

        return await response.Content.ReadAsStringAsync();
    }

    private static MatchScoreResponse MapToResponse(MatchScore m) => new()
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
}
