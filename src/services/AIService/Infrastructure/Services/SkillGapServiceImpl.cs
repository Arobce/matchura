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

public class SkillGapServiceImpl : ISkillGapService
{
    private readonly AIDbContext _db;
    private readonly SkillGapAnalyzerAgent _agent;
    private readonly ICacheService _cache;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SkillGapServiceImpl> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SkillGapServiceImpl(
        AIDbContext db,
        SkillGapAnalyzerAgent agent,
        ICacheService cache,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SkillGapServiceImpl> logger)
    {
        _db = db;
        _agent = agent;
        _cache = cache;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SkillGapReportResponse> AnalyzeAsync(string candidateId, AnalyzeSkillGapRequest request)
    {
        var cacheKey = $"skillgap:{candidateId}:{request.JobId}";
        var cached = await _cache.GetAsync<SkillGapReportResponse>(cacheKey);
        if (cached != null) return cached;

        // Build candidate data
        var candidateData = await BuildCandidateDataAsync(candidateId);

        // Fetch job data
        var jobData = await FetchJobDataAsync(request.JobId);

        // Run AI agent
        var result = await _agent.AnalyzeAsync(candidateData, jobData);

        // Upsert report
        var existing = await _db.SkillGapReports
            .FirstOrDefaultAsync(r => r.CandidateId == candidateId && r.JobId == request.JobId);

        if (existing != null)
        {
            existing.Summary = result.Summary;
            existing.OverallReadiness = result.OverallReadiness;
            existing.EstimatedTimeToReady = result.EstimatedTimeToReady;
            existing.MissingSkills = JsonSerializer.Serialize(result.MissingSkills, JsonOpts);
            existing.RecommendedActions = JsonSerializer.Serialize(result.RecommendedActions, JsonOpts);
            existing.StrengthAreas = JsonSerializer.Serialize(result.Strengths, JsonOpts);
            existing.GeneratedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new SkillGapReport
            {
                CandidateId = candidateId,
                JobId = request.JobId,
                Summary = result.Summary,
                OverallReadiness = result.OverallReadiness,
                EstimatedTimeToReady = result.EstimatedTimeToReady,
                MissingSkills = JsonSerializer.Serialize(result.MissingSkills, JsonOpts),
                RecommendedActions = JsonSerializer.Serialize(result.RecommendedActions, JsonOpts),
                StrengthAreas = JsonSerializer.Serialize(result.Strengths, JsonOpts)
            };
            _db.SkillGapReports.Add(existing);
        }

        await _db.SaveChangesAsync();

        var response = MapToResponse(existing);
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(48));

        _logger.LogInformation("Skill gap analyzed: candidate {CandidateId} × job {JobId}, readiness={Readiness}%",
            candidateId, request.JobId, result.OverallReadiness);

        return response;
    }

    public async Task<SkillGapReportResponse?> GetReportAsync(string candidateId, Guid jobId)
    {
        var report = await _db.SkillGapReports
            .FirstOrDefaultAsync(r => r.CandidateId == candidateId && r.JobId == jobId);

        return report == null ? null : MapToResponse(report);
    }

    public async Task<List<SkillGapReportResponse>> GetReportsForCandidateAsync(string candidateId)
    {
        var reports = await _db.SkillGapReports
            .Where(r => r.CandidateId == candidateId)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync();

        return reports.Select(MapToResponse).ToList();
    }

    private async Task<string> BuildCandidateDataAsync(string candidateId)
    {
        var skills = await _db.CandidateSkills
            .Where(s => s.CandidateId == candidateId)
            .ToListAsync();

        var latestResume = await _db.Resumes
            .Where(r => r.CandidateId == candidateId && r.ParsedData != null)
            .OrderByDescending(r => r.ParsedAt)
            .FirstOrDefaultAsync();

        return JsonSerializer.Serialize(new
        {
            parsedResume = latestResume?.ParsedData,
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

    private static SkillGapReportResponse MapToResponse(SkillGapReport r) => new()
    {
        ReportId = r.ReportId,
        CandidateId = r.CandidateId,
        JobId = r.JobId,
        Summary = r.Summary,
        OverallReadiness = r.OverallReadiness,
        EstimatedTimeToReady = r.EstimatedTimeToReady,
        MissingSkills = string.IsNullOrEmpty(r.MissingSkills) ? new()
            : JsonSerializer.Deserialize<List<MissingSkillEntry>>(r.MissingSkills, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new(),
        RecommendedActions = string.IsNullOrEmpty(r.RecommendedActions) ? new()
            : JsonSerializer.Deserialize<List<RecommendedAction>>(r.RecommendedActions, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new(),
        Strengths = string.IsNullOrEmpty(r.StrengthAreas) ? new()
            : JsonSerializer.Deserialize<List<string>>(r.StrengthAreas) ?? new(),
        GeneratedAt = r.GeneratedAt
    };
}
