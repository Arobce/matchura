using System.Net;
using AIService.Agents;
using AIService.Agents.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.TestUtilities.Fakes;

namespace AIService.UnitTests.Agents;

public class SkillGapAnalyzerAgentTests
{
    private static readonly string SkillGapFixture = """
        {
          "summary": "The candidate has strong .NET and React skills but lacks cloud-native and DevOps experience required for this senior platform engineering role. With focused upskilling in Kubernetes and CI/CD, readiness can be achieved in 2-4 months.",
          "overallReadiness": 62,
          "estimatedTimeToReady": "2-4 months",
          "missingSkills": [
            {
              "skillName": "Kubernetes",
              "importance": "Critical",
              "currentLevel": "None",
              "requiredLevel": "Advanced",
              "gapSeverity": 9,
              "recommendation": "Complete the Certified Kubernetes Application Developer (CKAD) course and build a multi-service deployment project."
            },
            {
              "skillName": "Terraform",
              "importance": "Important",
              "currentLevel": "Beginner",
              "requiredLevel": "Intermediate",
              "gapSeverity": 6,
              "recommendation": "Work through HashiCorp's Terraform Associate tutorials and provision a small AWS environment."
            },
            {
              "skillName": "GraphQL",
              "importance": "NiceToHave",
              "currentLevel": "None",
              "requiredLevel": "Intermediate",
              "gapSeverity": 4,
              "recommendation": "Build a GraphQL API using Hot Chocolate for .NET to combine with existing REST knowledge."
            }
          ],
          "recommendedActions": [
            {
              "priority": 1,
              "action": "Enroll in the CKAD certification preparation course on KodeKloud",
              "estimatedTime": "4 weeks",
              "resourceType": "Course",
              "rationale": "Kubernetes is a critical gap and the primary blocker for this role."
            },
            {
              "priority": 2,
              "action": "Deploy the existing microservices project to a Kubernetes cluster on AWS EKS",
              "estimatedTime": "2 weeks",
              "resourceType": "Project",
              "rationale": "Hands-on experience bridging current .NET/Docker skills with Kubernetes orchestration."
            },
            {
              "priority": 3,
              "action": "Complete the Terraform Associate certification prep",
              "estimatedTime": "2 weeks",
              "resourceType": "Certification",
              "rationale": "Infrastructure as code is a key requirement and complements Kubernetes knowledge."
            }
          ],
          "strengths": [
            "Expert-level C# and .NET aligning with the core technology stack",
            "Strong React/TypeScript skills covering the frontend requirements",
            "Proven leadership and mentoring experience matching the senior role expectations"
          ]
        }
        """;

    private (SkillGapAnalyzerAgent Agent, FakeClaudeHandler Handler) CreateAgent()
    {
        var handler = new FakeClaudeHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.anthropic.com/")
        };
        var client = new ClaudeApiClient(httpClient, NullLogger<ClaudeApiClient>.Instance);
        var agent = new SkillGapAnalyzerAgent(client);
        return (agent, handler);
    }

    [Fact]
    public async Task AnalyzeAsync_ValidResponse_ReturnsValidReadinessScore()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SkillGapFixture);

        // Act
        var result = await agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        result.OverallReadiness.Should().Be(62);
        result.OverallReadiness.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task AnalyzeAsync_ValidResponse_SummaryIsSubstantive()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SkillGapFixture);

        // Act
        var result = await agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        result.Summary.Should().NotBeNullOrWhiteSpace();
        result.Summary.Length.Should().BeGreaterThan(20,
            because: "the summary should be a meaningful description, not a placeholder");
        result.Summary.Should().Contain("cloud-native");
    }

    [Fact]
    public async Task AnalyzeAsync_ValidResponse_MissingSkillsArePopulated()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SkillGapFixture);

        // Act
        var result = await agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        result.MissingSkills.Should().NotBeEmpty();
        result.MissingSkills.Should().HaveCount(3);

        result.MissingSkills.Should().AllSatisfy(skill =>
        {
            skill.SkillName.Should().NotBeNullOrWhiteSpace();
            skill.Importance.Should().BeOneOf("Critical", "Important", "NiceToHave");
            skill.RequiredLevel.Should().NotBeNullOrWhiteSpace();
            skill.GapSeverity.Should().BeInRange(1, 10);
            skill.Recommendation.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public async Task AnalyzeAsync_ValidResponse_MissingSkillsOrderedBySeverity()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SkillGapFixture);

        // Act
        var result = await agent.AnalyzeAsync("candidate data", "job data");

        // Assert -- the fixture has skills ordered by descending gap severity (9, 6, 4)
        var severities = result.MissingSkills.Select(s => s.GapSeverity).ToList();
        severities.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task AnalyzeAsync_ValidResponse_RecommendedActionsArePresent()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SkillGapFixture);

        // Act
        var result = await agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        result.RecommendedActions.Should().NotBeEmpty();
        result.RecommendedActions.Should().HaveCount(3);

        result.RecommendedActions.Should().AllSatisfy(action =>
        {
            action.Priority.Should().BeGreaterThan(0);
            action.Action.Should().NotBeNullOrWhiteSpace();
            action.EstimatedTime.Should().NotBeNullOrWhiteSpace();
            action.ResourceType.Should().BeOneOf("Course", "Project", "Certification", "Practice");
            action.Rationale.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public async Task AnalyzeAsync_ValidResponse_ActionsHaveSequentialPriority()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SkillGapFixture);

        // Act
        var result = await agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        var priorities = result.RecommendedActions.Select(a => a.Priority).ToList();
        priorities.Should().BeInAscendingOrder();
        priorities.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task AnalyzeAsync_ValidResponse_StrengthsArePopulated()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SkillGapFixture);

        // Act
        var result = await agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        result.Strengths.Should().NotBeEmpty();
        result.Strengths.Should().HaveCount(3);
        result.Strengths.Should().AllSatisfy(s => s.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public async Task AnalyzeAsync_ValidResponse_EstimatedTimeIsPresent()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SkillGapFixture);

        // Act
        var result = await agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        result.EstimatedTimeToReady.Should().Be("2-4 months");
    }

    [Fact]
    public async Task AnalyzeAsync_MalformedJson_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueMalformedJson();
        handler.EnqueueMalformedJson();
        handler.EnqueueMalformedJson();

        // Act
        var act = () => agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        await act.Should().ThrowAsync<System.Text.Json.JsonException>();
    }

    [Fact]
    public async Task AnalyzeAsync_RateLimited_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");

        // Act
        var act = () => agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*rate limit exceeded*");
    }

    [Fact]
    public async Task AnalyzeAsync_ServerError_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);

        // Act
        var act = () => agent.AnalyzeAsync("candidate data", "job data");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
