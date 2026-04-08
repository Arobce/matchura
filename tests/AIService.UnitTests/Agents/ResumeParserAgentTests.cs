using System.Net;
using AIService.Agents;
using AIService.Agents.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.TestUtilities.Fakes;

namespace AIService.UnitTests.Agents;

public class ResumeParserAgentTests
{
    private static readonly string SeniorResumeFixture = """
        {
          "personalInfo": {
            "name": "Jane Smith",
            "email": "jane.smith@email.com",
            "phone": "+1-555-123-4567",
            "location": "San Francisco, CA"
          },
          "summary": "Senior full-stack developer with 10+ years of experience building scalable web applications. Expert in .NET, React, and cloud architecture with a track record of leading high-performing engineering teams.",
          "experience": [
            {
              "company": "TechCorp Inc.",
              "title": "Senior Software Engineer",
              "startDate": "2020-01",
              "endDate": null,
              "description": "Lead development of microservices architecture serving 5M+ users.",
              "highlights": ["Led team of 8 engineers", "Reduced API latency by 40%", "Implemented CI/CD pipeline"]
            },
            {
              "company": "WebDev Solutions",
              "title": "Software Engineer",
              "startDate": "2016-03",
              "endDate": "2019-12",
              "description": "Built customer-facing web applications using React and Node.js.",
              "highlights": ["Shipped 3 major product releases", "Mentored 4 junior developers"]
            }
          ],
          "education": [
            {
              "institution": "University of California, Berkeley",
              "degree": "Bachelor of Science",
              "field": "Computer Science",
              "graduationDate": "2014-05",
              "gpa": 3.8
            }
          ],
          "skills": [
            { "name": "C#", "category": "Programming", "proficiencyLevel": "Expert", "yearsUsed": 10 },
            { "name": ".NET", "category": "Framework", "proficiencyLevel": "Expert", "yearsUsed": 10 },
            { "name": "React", "category": "Framework", "proficiencyLevel": "Advanced", "yearsUsed": 7 },
            { "name": "TypeScript", "category": "Programming", "proficiencyLevel": "Advanced", "yearsUsed": 5 },
            { "name": "PostgreSQL", "category": "Database", "proficiencyLevel": "Advanced", "yearsUsed": 8 },
            { "name": "Docker", "category": "DevOps", "proficiencyLevel": "Advanced", "yearsUsed": 5 },
            { "name": "AWS", "category": "Cloud", "proficiencyLevel": "Intermediate", "yearsUsed": 4 },
            { "name": "RabbitMQ", "category": "DevOps", "proficiencyLevel": "Intermediate", "yearsUsed": 3 },
            { "name": "Redis", "category": "Database", "proficiencyLevel": "Intermediate", "yearsUsed": 4 },
            { "name": "Git", "category": "DevOps", "proficiencyLevel": "Expert", "yearsUsed": 10 },
            { "name": "Agile", "category": "Soft Skills", "proficiencyLevel": "Advanced", "yearsUsed": 8 },
            { "name": "Team Leadership", "category": "Soft Skills", "proficiencyLevel": "Advanced", "yearsUsed": 5 }
          ],
          "certifications": [
            { "name": "AWS Solutions Architect Associate", "issuer": "Amazon Web Services", "date": "2022-03" }
          ],
          "projects": [
            {
              "name": "Microservices Migration",
              "description": "Led the migration of a monolithic application to microservices architecture",
              "technologies": ["C#", ".NET", "Docker", "RabbitMQ", "PostgreSQL"]
            }
          ]
        }
        """;

    private (ResumeParserAgent Agent, FakeClaudeHandler Handler) CreateAgent()
    {
        var handler = new FakeClaudeHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.anthropic.com/")
        };
        var client = new ClaudeApiClient(httpClient, NullLogger<ClaudeApiClient>.Instance);
        var agent = new ResumeParserAgent(client);
        return (agent, handler);
    }

    [Fact]
    public async Task ParseAsync_ValidResponse_DeserializesAllFields()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SeniorResumeFixture);

        // Act
        var result = await agent.ParseAsync("some resume text");

        // Assert
        result.Should().NotBeNull();
        result.PersonalInfo.Should().NotBeNull();
        result.PersonalInfo!.Name.Should().Be("Jane Smith");
        result.PersonalInfo.Email.Should().Be("jane.smith@email.com");
        result.PersonalInfo.Phone.Should().Be("+1-555-123-4567");
        result.PersonalInfo.Location.Should().Be("San Francisco, CA");

        result.Summary.Should().Contain("Senior full-stack developer");

        result.Experience.Should().HaveCount(2);
        result.Experience[0].Company.Should().Be("TechCorp Inc.");
        result.Experience[0].Title.Should().Be("Senior Software Engineer");
        result.Experience[0].Highlights.Should().Contain("Led team of 8 engineers");

        result.Education.Should().HaveCount(1);
        result.Education[0].Institution.Should().Be("University of California, Berkeley");
        result.Education[0].Degree.Should().Be("Bachelor of Science");
        result.Education[0].Field.Should().Be("Computer Science");

        result.Certifications.Should().HaveCount(1);
        result.Certifications[0].Name.Should().Be("AWS Solutions Architect Associate");

        result.Projects.Should().HaveCount(1);
        result.Projects[0].Technologies.Should().Contain("Docker");
    }

    [Fact]
    public async Task ParseAsync_ValidResponse_SkillsHaveValidProficiencyLevels()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SeniorResumeFixture);
        var validLevels = new[] { "Beginner", "Intermediate", "Advanced", "Expert" };

        // Act
        var result = await agent.ParseAsync("some resume text");

        // Assert
        result.Skills.Should().NotBeEmpty();
        result.Skills.Should().AllSatisfy(skill =>
        {
            skill.ProficiencyLevel.Should().BeOneOf(validLevels,
                because: $"skill '{skill.Name}' should have a valid proficiency level");
        });
    }

    [Fact]
    public async Task ParseAsync_ValidResponse_SkillsCountIsReasonable()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SeniorResumeFixture);

        // Act
        var result = await agent.ParseAsync("some resume text");

        // Assert
        result.Skills.Count.Should().BeInRange(1, 50,
            because: "a real resume typically has between 1 and 50 skills");
    }

    [Fact]
    public async Task ParseAsync_ValidResponse_SkillsHaveCategoriesAndNames()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        handler.EnqueueSuccess(SeniorResumeFixture);

        // Act
        var result = await agent.ParseAsync("some resume text");

        // Assert
        result.Skills.Should().AllSatisfy(skill =>
        {
            skill.Name.Should().NotBeNullOrWhiteSpace();
            skill.Category.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public async Task ParseAsync_MalformedJson_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        // SendAndParseAsync retries 3 times on JSON parse failure.
        // Each retry calls SendMessageAsync again with a self-correction prompt.
        handler.EnqueueMalformedJson();
        handler.EnqueueMalformedJson();
        handler.EnqueueMalformedJson();

        // Act
        var act = () => agent.ParseAsync("some resume text");

        // Assert
        await act.Should().ThrowAsync<System.Text.Json.JsonException>();
    }

    [Fact]
    public async Task ParseAsync_RateLimited_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        // ClaudeApiClient.SendMessageAsync retries up to 3 times on 429.
        // After 3 retries (4 total requests) it throws.
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");
        handler.EnqueueRaw(HttpStatusCode.TooManyRequests, """{"error":"rate_limited"}""");

        // Act
        var act = () => agent.ParseAsync("some resume text");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*rate limit exceeded*");
    }

    [Fact]
    public async Task ParseAsync_ServerError_ThrowsAfterRetries()
    {
        // Arrange
        var (agent, handler) = CreateAgent();
        // HttpRequestException retries happen inside SendMessageAsync (up to 3 retries).
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);
        handler.EnqueueRaw(HttpStatusCode.InternalServerError);

        // Act
        var act = () => agent.ParseAsync("some resume text");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
