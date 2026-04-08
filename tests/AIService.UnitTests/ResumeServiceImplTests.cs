using System.Text;
using System.Threading.Channels;
using AIService.Application.DTOs;
using AIService.Domain.Entities;
using AIService.Domain.Enums;
using AIService.Infrastructure.Data;
using AIService.Infrastructure.Services;
using AIService.Infrastructure.TextExtraction;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.TestUtilities;
using Shared.TestUtilities.Fakes;

namespace AIService.UnitTests;

public class ResumeServiceImplTests : IDisposable
{
    private readonly AIDbContext _db;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly FakeS3StorageService _s3;
    private readonly Channel<Guid> _channel;
    private readonly Mock<ITextExtractor> _pdfExtractor;
    private readonly ResumeServiceImpl _sut;

    public ResumeServiceImplTests()
    {
        (_db, _connection) = DbContextFactory.Create<AIDbContext>();
        RegisterGuidGeneration(_db);
        _s3 = new FakeS3StorageService();
        _channel = Channel.CreateUnbounded<Guid>();

        _pdfExtractor = new Mock<ITextExtractor>();
        _pdfExtractor.Setup(e => e.CanHandle("application/pdf")).Returns(true);
        _pdfExtractor.Setup(e => e.CanHandle(It.Is<string>(s => s != "application/pdf"))).Returns(false);

        _sut = new ResumeServiceImpl(
            _db,
            new[] { _pdfExtractor.Object },
            _channel,
            _s3,
            NullLogger<ResumeServiceImpl>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // -- Helpers --

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
            foreach (var entry in ctx.ChangeTracker.Entries<MatchScore>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.MatchScoreId == Guid.Empty)
                    entry.Entity.MatchScoreId = Guid.NewGuid();
            }
            foreach (var entry in ctx.ChangeTracker.Entries<SkillGapReport>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.ReportId == Guid.Empty)
                    entry.Entity.ReportId = Guid.NewGuid();
            }
        };
    }

    private static MemoryStream CreateFakeFileStream(string content = "fake pdf content")
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    // -- Tests --

    [Fact]
    public async Task UploadResumeAsync_UploadsToS3ExtractsTextAndEnqueuesForParsing()
    {
        // Arrange
        var candidateId = "candidate-upload";
        var fileName = "my-resume.pdf";
        var contentType = "application/pdf";
        using var fileStream = CreateFakeFileStream();

        _pdfExtractor
            .Setup(e => e.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Extracted resume text content");

        // Act
        var result = await _sut.UploadResumeAsync(candidateId, fileStream, fileName, contentType);

        // Assert
        result.Status.Should().Be("Uploaded");
        result.Message.Should().Contain("queued for parsing");

        // Verify S3 upload occurred
        _s3.Files.Should().HaveCount(1);
        var s3Key = _s3.Files.Keys.Single();
        s3Key.Should().Contain($"resumes/{candidateId}");
        s3Key.Should().EndWith(fileName);

        // Verify DB record
        var resume = _db.Resumes.Single(r => r.CandidateId == candidateId);
        resume.OriginalFileName.Should().Be(fileName);
        resume.ContentType.Should().Be(contentType);
        resume.RawText.Should().Be("Extracted resume text content");
        resume.ParseStatus.Should().Be(ParseStatus.Uploaded);
        resume.FileUrl.Should().Contain(candidateId);

        // Verify enqueued for parsing
        _channel.Reader.TryRead(out var enqueuedId).Should().BeTrue();
        enqueuedId.Should().Be(resume.ResumeId);
    }

    [Fact]
    public async Task UploadResumeAsync_UnsupportedContentType_Throws()
    {
        // Arrange
        var candidateId = "candidate-bad-type";
        using var fileStream = CreateFakeFileStream();

        // Act & Assert
        var act = () => _sut.UploadResumeAsync(candidateId, fileStream, "file.xyz", "application/octet-stream");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Unsupported file type*");
    }

    [Fact]
    public async Task GetResumesByCandidateAsync_ReturnsOrderedByUploadDate()
    {
        // Arrange
        var candidateId = "candidate-list";

        var older = new Resume
        {
            ResumeId = Guid.NewGuid(),
            CandidateId = candidateId,
            OriginalFileName = "old-resume.pdf",
            FileUrl = "resumes/old.pdf",
            ContentType = "application/pdf",
            ParseStatus = ParseStatus.Completed,
            UploadedAt = DateTime.UtcNow.AddDays(-5)
        };
        var newer = new Resume
        {
            ResumeId = Guid.NewGuid(),
            CandidateId = candidateId,
            OriginalFileName = "new-resume.pdf",
            FileUrl = "resumes/new.pdf",
            ContentType = "application/pdf",
            ParseStatus = ParseStatus.Uploaded,
            UploadedAt = DateTime.UtcNow.AddDays(-1)
        };

        _db.Resumes.AddRange(older, newer);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.GetResumesByCandidateAsync(candidateId);

        // Assert
        result.Should().HaveCount(2);
        result[0].OriginalFileName.Should().Be("new-resume.pdf");
        result[1].OriginalFileName.Should().Be("old-resume.pdf");
    }

    [Fact]
    public async Task GetResumeByIdAsync_WrongCandidate_Throws()
    {
        // Arrange
        var resumeId = Guid.NewGuid();
        _db.Resumes.Add(new Resume
        {
            ResumeId = resumeId,
            CandidateId = "owner-candidate",
            OriginalFileName = "resume.pdf",
            FileUrl = "resumes/resume.pdf",
            ContentType = "application/pdf",
            ParseStatus = ParseStatus.Uploaded
        });
        await _db.SaveChangesAsync();

        // Act & Assert
        var act = () => _sut.GetResumeByIdAsync(resumeId, "wrong-candidate");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
