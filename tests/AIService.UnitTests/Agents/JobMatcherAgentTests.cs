using System.Net;
using AIService.Agents;
using AIService.Agents.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.TestUtilities.Fakes;

namespace AIService.UnitTests.Agents;

public class JobMatcherAgentTests
{
    private static readonly string StrongMatchFixture = """
        {
          "overallScore": 85,
          "skillScore": 88,
          "experienceScore": 82,
          "educationScore": 80,
          "explanation": "The candidate demonstrates strong alignment with the job requirements. Their extensive experience with .NET and React directly matches the core technology stack. Leadership experience and cloud architecture knowledge further strengthen their candidacy.",
          "strengths": [
            "Expert-level .NET and C# skills matching the primary technology requirement",
            "7+ years of React experience covering the frontend stack needs",
            "Proven team leadership with experience managing 8 engineers",
            "Strong PostgreSQL database skills relevant to the data layer"
          ],
          "gaps": [
            "Limited Kubernetes experience compared to the DevOps requirements",
            "No GraphQL experience mentioned though the role includes API design"
          ]
        }
        """;

    private static readonly string WeakMatchFixture = """
        {
          "overallScore": 35,
          "skillScore": 28,
          "experienceScore": 40,
          "educationScore": 45,
          "explanation": "The candidate shows limited alignment with the position requirements. While they have foundational programming skills, they lack experience with the core technology stack (.NET, React) and have minimal professional experience.",
          "strengths": [
            "Strong academic foundation in computer science fundamentals",
            "Demonstrated initiative through personal projects and internship"
          ],
          "gaps": [
            "No professional experience with C# or .NET framework required for the role",
            "Only 3 months of professional experience versus the 5+ years required",
            "Missing cloud infrastructure experience (AWS, Docker, Kubernetes)",
            "No experience with microservices architecture or distributed systems"
          ]
        }
        """;

    private (JobMatcherAgent Agent, FakeClaudeHandler Handler) CreateAgent()
    {
        var handler = new FakeClaudeHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.anthropic.com/")
        };
        var client = new ClaudeApiClient(httpClient, NullLogger<ClaudeApiClient>.Instance);
        var agent = new JobMatcherAgent(client);
        return (agent, handler);
    }

    [Fact]
    public async Task ComputeMatchAsync_StrongMatch_ReturnsValidScores()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(StrongMatchFixture);

        // Act
        var result = await agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        result.OverallScore.Should().Be(85);
        result.SkillScore.Should().Be(88);
        result.ExperienceScore.Should().Be(82);
        result.EducationScore.Should().Be(80);
    }

    [Fact]
    public async Task ComputeMatchAsync_WeakMatch_ReturnsLowerScores()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(WeakMatchFixture);

        // Act
        var result = await agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        result.OverallScore.Should().Be(35);
        result.SkillScore.Should().Be(28);
        result.ExperienceScore.Should().Be(40);
        result.EducationScore.Should().Be(45);
    }

    [Theory]
    [InlineData(nameof(StrongMatchFixture))]
    [InlineData(nameof(WeakMatchFixture))]
    public async Task ComputeMatchAsync_AllFixtures_ScoresAreInValidRange(string fixtureName)
    {
        // Arrange
        var fixture = fixtureName == nameof(StrongMatchFixture)
            ? StrongMatchFixture : WeakMatchFixture;
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(fixture);

        // Act
        var result = await agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        result.OverallScore.Should().BeInRange(0, 100);
        result.SkillScore.Should().BeInRange(0, 100);
        result.ExperienceScore.Should().BeInRange(0, 100);
        result.EducationScore.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task ComputeMatchAsync_StrongMatch_ExplanationIsSubstantive()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(StrongMatchFixture);

        // Act
        var result = await agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        result.Explanation.Should().NotBeNullOrWhiteSpace();
        result.Explanation.Length.Should().BeGreaterThan(20,
            because: "a match explanation should be a substantive sentence, not a placeholder");
    }

    [Fact]
    public async Task ComputeMatchAsync_StrongMatch_StrengthsAndGapsArePopulated()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(StrongMatchFixture);

        // Act
        var result = await agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        result.Strengths.Should().NotBeEmpty();
        result.Strengths.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Strengths.Should().AllSatisfy(s => s.Should().NotBeNullOrWhiteSpace());

        result.Gaps.Should().NotBeEmpty();
        result.Gaps.Should().AllSatisfy(g => g.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public async Task ComputeMatchAsync_WeakMatch_HasMoreGapsThanStrengths()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(WeakMatchFixture);

        // Act
        var result = await agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        result.Gaps.Count.Should().BeGreaterThan(result.Strengths.Count,
            because: "a weak match should have more gaps than strengths");
    }

    [Fact]
    public async Task ComputeMatchAsync_MalformedJson_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueMalformedJson();
        handler.EnqueueMalformedJson();
        handler.EnqueueMalformedJson();

        // Act
        var act = () => agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        await act.Should().ThrowAsync<System.Text.Json.JsonException>();
    }

    [Fact]
    public async Task ComputeMatchAsync_RateLimited_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");

        // Act
        var act = () => agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*rate limit exceeded*");
    }

    [Fact]
    public async Task ComputeMatchAsync_ServerError_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);

        // Act
        var act = () => agent.ComputeMatchAsync("candidate profile", "job requirements");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
