using System.Text.Json;
using System.Threading.Channels;
using AIService.Agents;
using AIService.Application.DTOs;
using AIService.Domain.Entities;
using AIService.Domain.Enums;
using AIService.Infrastructure.BackgroundJobs;
using AIService.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.TestUtilities;

namespace AIService.UnitTests.Workers;

public class ResumeParsingWorkerTests : IDisposable
{
    private readonly AIDbContext _db;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly Mock<ResumeParserAgent> _agentMock;
    private readonly ResumeParsingWorker _worker;

    public ResumeParsingWorkerTests()
    {
        (_db, _connection) = DbContextFactory.Create<AIDbContext>();
        RegisterGuidGeneration(_db);

        _agentMock = new Mock<ResumeParserAgent>(MockBehavior.Strict,
            new object[] { null! });

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(AIDbContext)))
            .Returns(_db);
        serviceProvider
            .Setup(sp => sp.GetService(typeof(ResumeParserAgent)))
            .Returns(_agentMock.Object);

        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory
            .Setup(f => f.CreateScope())
            .Returns(scope.Object);

        var channel = Channel.CreateUnbounded<Guid>();
        var logger = new Mock<ILogger<ResumeParsingWorker>>();

        _worker = new ResumeParsingWorker(channel, scopeFactory.Object, logger.Object);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ProcessResumeAsync_ValidText_ParsesAndStoresData()
    {
        // Arrange
        var resumeId = Guid.NewGuid();
        var candidateId = "candidate-123";
        var resume = new Resume
        {
            ResumeId = resumeId,
            CandidateId = candidateId,
            OriginalFileName = "resume.pdf",
            FileUrl = "https://example.com/resume.pdf",
            ContentType = "application/pdf",
            RawText = "John Doe, Software Engineer, 5 years C#",
            ParseStatus = ParseStatus.Uploaded
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        var parsedData = new ParsedResumeData
        {
            PersonalInfo = new PersonalInfo { Name = "John Doe" },
            Summary = "Experienced software engineer",
            Skills = new List<SkillEntry>
            {
                new() { Name = "C#", Category = "Programming", ProficiencyLevel = "Advanced", YearsUsed = 5 },
                new() { Name = "SQL", Category = "Database", ProficiencyLevel = "Intermediate", YearsUsed = 3 }
            }
        };

        _agentMock
            .Setup(a => a.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsedData);

        // Act
        await _worker.ProcessResumeAsync(resumeId, CancellationToken.None);

        // Assert
        var updated = await _db.Resumes.FirstAsync(r => r.ResumeId == resumeId);
        updated.ParseStatus.Should().Be(ParseStatus.Completed);
        updated.ParsedData.Should().NotBeNullOrEmpty();
        updated.ParsedAt.Should().NotBeNull();

        var deserialized = JsonSerializer.Deserialize<ParsedResumeData>(updated.ParsedData!);
        deserialized!.PersonalInfo!.Name.Should().Be("John Doe");
        deserialized.Skills.Should().HaveCount(2);

        var skills = await _db.CandidateSkills
            .Where(s => s.CandidateId == candidateId)
            .ToListAsync();
        skills.Should().HaveCount(2);
        skills.Should().Contain(s => s.SkillName == "C#" && s.ProficiencyLevel == ProficiencyLevel.Advanced);
        skills.Should().Contain(s => s.SkillName == "SQL" && s.ProficiencyLevel == ProficiencyLevel.Intermediate);
        skills.Should().OnlyContain(s => s.Source == "resume_parse");
    }

    [Fact]
    public async Task ProcessResumeAsync_AgentThrows_SetsStatusToFailedWithErrorMessage()
    {
        // Arrange
        var resumeId = Guid.NewGuid();
        var resume = new Resume
        {
            ResumeId = resumeId,
            CandidateId = "candidate-456",
            OriginalFileName = "resume.pdf",
            FileUrl = "https://example.com/resume.pdf",
            ContentType = "application/pdf",
            RawText = "Some resume text",
            ParseStatus = ParseStatus.Uploaded
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        _agentMock
            .Setup(a => a.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Claude API rate limit exceeded"));

        // Act
        await _worker.ProcessResumeAsync(resumeId, CancellationToken.None);

        // Assert
        var updated = await _db.Resumes.FirstAsync(r => r.ResumeId == resumeId);
        updated.ParseStatus.Should().Be(ParseStatus.Failed);
        updated.ErrorMessage.Should().Contain("Claude API rate limit exceeded");
    }

    [Fact]
    public async Task ProcessResumeAsync_EmptyRawText_SetsStatusToFailed()
    {
        // Arrange
        var resumeId = Guid.NewGuid();
        var resume = new Resume
        {
            ResumeId = resumeId,
            CandidateId = "candidate-789",
            OriginalFileName = "empty.pdf",
            FileUrl = "https://example.com/empty.pdf",
            ContentType = "application/pdf",
            RawText = "   ",
            ParseStatus = ParseStatus.Uploaded
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        // Act
        await _worker.ProcessResumeAsync(resumeId, CancellationToken.None);

        // Assert
        var updated = await _db.Resumes.FirstAsync(r => r.ResumeId == resumeId);
        updated.ParseStatus.Should().Be(ParseStatus.Failed);
        updated.ErrorMessage.Should().Be("No text content extracted from resume");
    }

    [Fact]
    public async Task ProcessResumeAsync_NullRawText_SetsStatusToFailed()
    {
        // Arrange
        var resumeId = Guid.NewGuid();
        var resume = new Resume
        {
            ResumeId = resumeId,
            CandidateId = "candidate-null",
            OriginalFileName = "null.pdf",
            FileUrl = "https://example.com/null.pdf",
            ContentType = "application/pdf",
            RawText = null,
            ParseStatus = ParseStatus.Uploaded
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        // Act
        await _worker.ProcessResumeAsync(resumeId, CancellationToken.None);

        // Assert
        var updated = await _db.Resumes.FirstAsync(r => r.ResumeId == resumeId);
        updated.ParseStatus.Should().Be(ParseStatus.Failed);
        updated.ErrorMessage.Should().Be("No text content extracted from resume");
    }

    [Fact]
    public async Task ProcessResumeAsync_ResumeNotFound_DoesNotThrow()
    {
        // Arrange - use a non-existent resume ID
        var resumeId = Guid.NewGuid();

        // Act & Assert - should not throw
        await _worker.Invoking(w => w.ProcessResumeAsync(resumeId, CancellationToken.None))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessResumeAsync_ReplacesOldParsedSkills()
    {
        // Arrange
        var resumeId = Guid.NewGuid();
        var candidateId = "candidate-replace";

        // Add existing skills from a previous parse
        _db.CandidateSkills.Add(new CandidateSkill
        {
            CandidateId = candidateId,
            SkillName = "OldSkill",
            SkillCategory = "Programming",
            ProficiencyLevel = ProficiencyLevel.Beginner,
            Source = "resume_parse"
        });

        // Add a manually added skill that should NOT be removed
        _db.CandidateSkills.Add(new CandidateSkill
        {
            CandidateId = candidateId,
            SkillName = "ManualSkill",
            SkillCategory = "Other",
            ProficiencyLevel = ProficiencyLevel.Expert,
            Source = "manual"
        });

        var resume = new Resume
        {
            ResumeId = resumeId,
            CandidateId = candidateId,
            OriginalFileName = "resume.pdf",
            FileUrl = "https://example.com/resume.pdf",
            ContentType = "application/pdf",
            RawText = "Updated resume content",
            ParseStatus = ParseStatus.Uploaded
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        var parsedData = new ParsedResumeData
        {
            Skills = new List<SkillEntry>
            {
                new() { Name = "NewSkill", Category = "Framework", ProficiencyLevel = "Expert", YearsUsed = 2 }
            }
        };

        _agentMock
            .Setup(a => a.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsedData);

        // Act
        await _worker.ProcessResumeAsync(resumeId, CancellationToken.None);

        // Assert
        var skills = await _db.CandidateSkills
            .Where(s => s.CandidateId == candidateId)
            .ToListAsync();

        skills.Should().HaveCount(2);
        skills.Should().Contain(s => s.SkillName == "NewSkill" && s.Source == "resume_parse");
        skills.Should().Contain(s => s.SkillName == "ManualSkill" && s.Source == "manual");
        skills.Should().NotContain(s => s.SkillName == "OldSkill");
    }

    private static void RegisterGuidGeneration(AIDbContext db)
    {
        db.SavingChanges += (sender, _) =>
        {
            if (sender is not AIDbContext ctx) return;
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
        };
    }
}
