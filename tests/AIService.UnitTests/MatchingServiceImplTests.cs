using System.Net;
using System.Text.Json;
using AIService.Agents;
using AIService.Agents.Core;
using AIService.Application.DTOs;
using AIService.Domain.Entities;
using AIService.Domain.Enums;
using AIService.Infrastructure.Data;
using AIService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.TestUtilities;
using Shared.TestUtilities.Fakes;

namespace AIService.UnitTests;

public class MatchingServiceImplTests : IDisposable
{
    private readonly AIDbContext _db;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly FakeCacheService _cache;
    private readonly Mock<JobMatcherAgent> _agentMock;
    private readonly MockHttpMessageHandler _httpHandler;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly MatchingServiceImpl _sut;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public MatchingServiceImplTests()
    {
        (_db, _connection) = DbContextFactory.Create<AIDbContext>();
        RegisterGuidGeneration(_db);
        _cache = new FakeCacheService();

        // Create a mock for JobMatcherAgent (method is virtual)
        var claudeClient = new ClaudeApiClient(
            new HttpClient(),
            NullLogger<ClaudeApiClient>.Instance);
        _agentMock = new Mock<JobMatcherAgent>(claudeClient) { CallBase = false };

        _httpHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_httpHandler);

        var configData = new Dictionary<string, string?>
        {
            ["JOB_SERVICE_URL"] = "http://test-job-service:8080"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _sut = new MatchingServiceImpl(
            _db,
            _agentMock.Object,
            _cache,
            _httpClient,
            _configuration,
            NullLogger<MatchingServiceImpl>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
        _httpClient.Dispose();
    }

    // -- Helpers --

    private static void RegisterGuidGeneration(AIDbContext db)
    {
        db.SavingChanges += (sender, _) =>
        {
            if (sender is not AIDbContext ctx) return;
            foreach (var entry in ctx.ChangeTracker.Entries<MatchScore>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.MatchScoreId == Guid.Empty)
                    entry.Entity.MatchScoreId = Guid.NewGuid();
            }
            foreach (var entry in ctx.ChangeTracker.Entries<Resume>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.ResumeId == Guid.Empty)
                    entry.Entity.ResumeId = Guid.NewGuid();
            }
            foreach (var entry in ctx.ChangeTracker.Entries<CandidateSkill>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.CandidateSkillId == Guid.Empty)
                    entry.Entity.CandidateSkillId = Guid.NewGuid();
            }
            foreach (var entry in ctx.ChangeTracker.Entries<SkillGapReport>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.ReportId == Guid.Empty)
                    entry.Entity.ReportId = Guid.NewGuid();
            }
        };
    }

    private static MatchResult CreateMatchResult(decimal overall = 85m) => new()
    {
        OverallScore = overall,
        SkillScore = 90m,
        ExperienceScore = 80m,
        EducationScore = 75m,
        Explanation = "Strong match overall",
        Strengths = new List<string> { "C# expertise", "Cloud experience" },
        Gaps = new List<string> { "No Kubernetes experience" }
    };

    private async Task SeedParsedResume(string candidateId)
    {
        _db.Resumes.Add(new Resume
        {
            ResumeId = Guid.NewGuid(),
            CandidateId = candidateId,
            OriginalFileName = "resume.pdf",
            FileUrl = "resumes/test/resume.pdf",
            ContentType = "application/pdf",
            ParsedData = JsonSerializer.Serialize(new { summary = "Software engineer" }, JsonOpts),
            ParseStatus = ParseStatus.Completed,
            ParsedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    private void EnqueueJobResponse(Guid jobId, string employerId = "employer-1")
    {
        var jobJson = JsonSerializer.Serialize(new
        {
            jobId,
            employerId,
            title = "Software Engineer",
            description = "Build things"
        }, JsonOpts);
        _httpHandler.EnqueueResponse(HttpStatusCode.OK, jobJson);
    }

    // -- Tests --

    [Fact]
    public async Task ComputeMatchAsync_CacheHit_ReturnsCachedWithoutCallingAgent()
    {
        // Arrange
        var candidateId = "candidate-1";
        var jobId = Guid.NewGuid();
        var request = new ComputeMatchRequest { JobId = jobId };

        var cachedResponse = new MatchScoreResponse
        {
            MatchScoreId = Guid.NewGuid(),
            CandidateId = candidateId,
            JobId = jobId,
            OverallScore = 92m,
            Explanation = "Cached result"
        };
        await _cache.SetAsync($"match:{candidateId}:{jobId}", cachedResponse, TimeSpan.FromHours(1));

        // Act
        var result = await _sut.ComputeMatchAsync(candidateId, request);

        // Assert
        result.OverallScore.Should().Be(92m);
        result.Explanation.Should().Be("Cached result");
        _agentMock.Verify(
            a => a.ComputeMatchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ComputeMatchAsync_CacheMiss_CallsAgentSavesToDbAndCaches()
    {
        // Arrange
        var candidateId = "candidate-2";
        var jobId = Guid.NewGuid();
        var request = new ComputeMatchRequest { JobId = jobId };

        await SeedParsedResume(candidateId);
        EnqueueJobResponse(jobId);

        var matchResult = CreateMatchResult();
        _agentMock
            .Setup(a => a.ComputeMatchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchResult);

        // Act
        var result = await _sut.ComputeMatchAsync(candidateId, request);

        // Assert
        result.OverallScore.Should().Be(85m);
        result.SkillScore.Should().Be(90m);
        result.ExperienceScore.Should().Be(80m);
        result.EducationScore.Should().Be(75m);
        result.Explanation.Should().Be("Strong match overall");
        result.Strengths.Should().Contain("C# expertise");
        result.Gaps.Should().Contain("No Kubernetes experience");
        result.CandidateId.Should().Be(candidateId);
        result.JobId.Should().Be(jobId);

        // Verify saved to DB
        var dbRecord = _db.MatchScores.Single(m => m.CandidateId == candidateId && m.JobId == jobId);
        dbRecord.OverallScore.Should().Be(85m);

        // Verify cached
        var cached = await _cache.GetAsync<MatchScoreResponse>($"match:{candidateId}:{jobId}");
        cached.Should().NotBeNull();
        cached!.OverallScore.Should().Be(85m);
    }

    [Fact]
    public async Task ComputeMatchAsync_NoParsedResume_ThrowsInvalidOperationException()
    {
        // Arrange - no resume seeded
        var candidateId = "candidate-no-resume";
        var jobId = Guid.NewGuid();
        var request = new ComputeMatchRequest { JobId = jobId };

        EnqueueJobResponse(jobId);

        // Act & Assert
        var act = () => _sut.ComputeMatchAsync(candidateId, request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*parsed resume*");
    }

    [Fact]
    public async Task ComputeMatchAsync_ExistingMatchScore_UpsertsInsteadOfDuplicating()
    {
        // Arrange
        var candidateId = "candidate-3";
        var jobId = Guid.NewGuid();
        var request = new ComputeMatchRequest { JobId = jobId };

        // Seed an existing match score
        _db.MatchScores.Add(new MatchScore
        {
            CandidateId = candidateId,
            JobId = jobId,
            OverallScore = 60m,
            SkillScore = 50m,
            ExperienceScore = 55m,
            EducationScore = 65m,
            Explanation = "Old result",
            Strengths = "[]",
            Gaps = "[]"
        });
        await _db.SaveChangesAsync();

        await SeedParsedResume(candidateId);
        EnqueueJobResponse(jobId);

        var updatedResult = CreateMatchResult(95m);
        _agentMock
            .Setup(a => a.ComputeMatchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedResult);

        // Act
        var result = await _sut.ComputeMatchAsync(candidateId, request);

        // Assert - score updated, not duplicated
        result.OverallScore.Should().Be(95m);
        _db.MatchScores.Count(m => m.CandidateId == candidateId && m.JobId == jobId).Should().Be(1);

        var dbRecord = _db.MatchScores.Single(m => m.CandidateId == candidateId && m.JobId == jobId);
        dbRecord.OverallScore.Should().Be(95m);
        dbRecord.Explanation.Should().Be("Strong match overall");
    }

    [Fact]
    public async Task GetMatchesForJobAsync_ReturnsPaginatedResultsSortedByScoreDesc()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var employerId = "employer-1";

        // Seed match scores with varying scores
        var scores = new[] { 60m, 95m, 75m, 88m, 40m };
        foreach (var (score, idx) in scores.Select((s, i) => (s, i)))
        {
            _db.MatchScores.Add(new MatchScore
            {
                CandidateId = $"candidate-{idx}",
                JobId = jobId,
                OverallScore = score,
                SkillScore = score,
                ExperienceScore = score,
                EducationScore = score,
                Explanation = $"Score {score}"
            });
        }
        await _db.SaveChangesAsync();

        // Mock the job fetch to return employerId
        EnqueueJobResponse(jobId, employerId);

        // Act
        var result = await _sut.GetMatchesForJobAsync(employerId, jobId, page: 1, pageSize: 3);

        // Assert
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.Items.Should().HaveCount(3);

        // Should be sorted by score descending
        result.Items[0].OverallScore.Should().Be(95m);
        result.Items[1].OverallScore.Should().Be(88m);
        result.Items[2].OverallScore.Should().Be(75m);
    }

    [Fact]
    public async Task GetRecommendedJobsAsync_ReturnsCandidateMatchesOrderedByScore()
    {
        // Arrange
        var candidateId = "candidate-recs";

        var jobScores = new[] { 50m, 92m, 78m, 65m };
        foreach (var (score, idx) in jobScores.Select((s, i) => (s, i)))
        {
            _db.MatchScores.Add(new MatchScore
            {
                CandidateId = candidateId,
                JobId = Guid.NewGuid(),
                OverallScore = score,
                SkillScore = score,
                ExperienceScore = score,
                EducationScore = score,
                Explanation = $"Job match {score}"
            });
        }
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.GetRecommendedJobsAsync(candidateId, page: 1, pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(4);
        result.Items.Should().HaveCount(4);
        result.Items[0].OverallScore.Should().Be(92m);
        result.Items[1].OverallScore.Should().Be(78m);
        result.Items[2].OverallScore.Should().Be(65m);
        result.Items[3].OverallScore.Should().Be(50m);

        result.Items.Should().OnlyContain(i => i.CandidateId == candidateId);
    }
}
