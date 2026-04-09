using System.Net;
using ApplicationService.Application.DTOs;
using ApplicationService.Domain.Entities;
using ApplicationService.Domain.Enums;
using ApplicationService.Infrastructure.Data;
using ApplicationService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.TestUtilities;
using Shared.TestUtilities.Fakes;
using SharedKernel.Events;

namespace ApplicationService.UnitTests;

public class ApplicationServiceImplTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly FakeEventBus _eventBus;
    private readonly MockHttpMessageHandler _httpHandler;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ApplicationServiceImpl _sut;

    private const string CandidateId = "candidate-1";
    private const string EmployerId = "employer-1";
    private static readonly Guid JobId = Guid.NewGuid();

    public ApplicationServiceImplTests()
    {
        (_db, _connection) = DbContextFactory.Create<ApplicationDbContext>();

        // SQLite does not support gen_random_uuid(), so assign GUIDs client-side before saving.
        _db.SavingChanges += (sender, _) =>
        {
            if (sender is not ApplicationDbContext ctx) return;
            foreach (var entry in ctx.ChangeTracker.Entries<JobApplication>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.ApplicationId == Guid.Empty)
                {
                    entry.Entity.ApplicationId = Guid.NewGuid();
                }
            }
        };

        _eventBus = new FakeEventBus();
        _httpHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_httpHandler);

        var configData = new Dictionary<string, string?>
        {
            { "JOB_SERVICE_URL", "http://job-service:8080" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var logger = Mock.Of<ILogger<ApplicationServiceImpl>>();

        _sut = new ApplicationServiceImpl(_db, _httpClient, _configuration, logger, _eventBus);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
        _httpClient.Dispose();
    }

    // -- Helpers --

    private void EnqueueJobOwnershipResponse(string employerId)
    {
        _httpHandler.EnqueueJsonResponse(new { employerId });
    }

    private void EnqueueJobOwnershipFailure()
    {
        _httpHandler.EnqueueResponse(HttpStatusCode.NotFound);
    }

    private async Task<JobApplication> SeedApplicationAsync(
        ApplicationStatus status = ApplicationStatus.Submitted,
        string? candidateId = null,
        Guid? jobId = null)
    {
        var app = new JobApplication
        {
            ApplicationId = Guid.NewGuid(),
            CandidateId = candidateId ?? CandidateId,
            JobId = jobId ?? JobId,
            CoverLetter = "Test cover letter",
            ResumeUrl = "https://example.com/resume.pdf",
            Status = status
        };
        _db.Applications.Add(app);
        await _db.SaveChangesAsync();
        return app;
    }

    // =================================================================
    // CreateApplicationAsync
    // =================================================================

    [Fact]
    public async Task CreateApplicationAsync_ShouldCreateWithSubmittedStatus_AndPublishEvent()
    {
        var request = new CreateApplicationRequest
        {
            JobId = JobId,
            CoverLetter = "I am interested",
            ResumeUrl = "https://example.com/resume.pdf"
        };

        var result = await _sut.CreateApplicationAsync(CandidateId, "Test Candidate", request);

        result.Should().NotBeNull();
        result.Status.Should().Be("Submitted");
        result.CandidateId.Should().Be(CandidateId);
        result.JobId.Should().Be(JobId);
        result.CoverLetter.Should().Be("I am interested");
        result.ResumeUrl.Should().Be("https://example.com/resume.pdf");
        result.CandidateName.Should().Be("Test Candidate");

        var saved = await _db.Applications.FindAsync(result.ApplicationId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(ApplicationStatus.Submitted);
        saved.CandidateName.Should().Be("Test Candidate");

        var events = _eventBus.GetEvents<ApplicationSubmittedEvent>();
        events.Should().HaveCount(1);
        events[0].CandidateId.Should().Be(CandidateId);
        events[0].JobId.Should().Be(JobId);
        events[0].ApplicationId.Should().Be(result.ApplicationId);
    }

    [Fact]
    public async Task CreateApplicationAsync_DuplicateApplication_ShouldThrowInvalidOperationException()
    {
        await SeedApplicationAsync();

        var request = new CreateApplicationRequest
        {
            JobId = JobId,
            CoverLetter = "Another attempt"
        };

        var act = () => _sut.CreateApplicationAsync(CandidateId, "Test Candidate", request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already applied*");
    }

    // =================================================================
    // UpdateApplicationStatusAsync — valid transitions
    // =================================================================

    [Theory]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Reviewed)]
    [InlineData(ApplicationStatus.Reviewed, ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.Reviewed, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Shortlisted, ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Shortlisted, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Rejected)]
    public async Task UpdateApplicationStatusAsync_ValidTransition_ShouldSucceedAndPublishEvent(
        ApplicationStatus from, ApplicationStatus to)
    {
        var app = await SeedApplicationAsync(status: from);
        EnqueueJobOwnershipResponse(EmployerId);

        var request = new UpdateApplicationStatusRequest { Status = to };

        var result = await _sut.UpdateApplicationStatusAsync(EmployerId, app.ApplicationId, request);

        result.Status.Should().Be(to.ToString());

        var events = _eventBus.GetEvents<ApplicationStatusChangedEvent>();
        events.Should().HaveCount(1);
        events[0].ApplicationId.Should().Be(app.ApplicationId);
        events[0].OldStatus.Should().Be(from.ToString());
        events[0].NewStatus.Should().Be(to.ToString());
    }

    // =================================================================
    // UpdateApplicationStatusAsync — invalid transitions
    // =================================================================

    [Theory]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.Submitted, ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Reviewed)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Accepted, ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.Accepted, ApplicationStatus.Reviewed)]
    [InlineData(ApplicationStatus.Accepted, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Reviewed, ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.Reviewed, ApplicationStatus.Accepted)]
    public async Task UpdateApplicationStatusAsync_InvalidTransition_ShouldThrowInvalidOperationException(
        ApplicationStatus from, ApplicationStatus to)
    {
        var app = await SeedApplicationAsync(status: from);
        EnqueueJobOwnershipResponse(EmployerId);

        var request = new UpdateApplicationStatusRequest { Status = to };

        var act = () => _sut.UpdateApplicationStatusAsync(EmployerId, app.ApplicationId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot transition*");
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_WithdrawnApplication_ShouldThrowInvalidOperationException()
    {
        var app = await SeedApplicationAsync(status: ApplicationStatus.Withdrawn);
        EnqueueJobOwnershipResponse(EmployerId);

        var request = new UpdateApplicationStatusRequest { Status = ApplicationStatus.Reviewed };

        var act = () => _sut.UpdateApplicationStatusAsync(EmployerId, app.ApplicationId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*withdrawn*");
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_NotJobOwner_ShouldThrowUnauthorizedAccessException()
    {
        var app = await SeedApplicationAsync();
        EnqueueJobOwnershipResponse("other-employer");

        var request = new UpdateApplicationStatusRequest { Status = ApplicationStatus.Reviewed };

        var act = () => _sut.UpdateApplicationStatusAsync(EmployerId, app.ApplicationId, request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_ApplicationNotFound_ShouldThrowInvalidOperationException()
    {
        var act = () => _sut.UpdateApplicationStatusAsync(
            EmployerId, Guid.NewGuid(), new UpdateApplicationStatusRequest { Status = ApplicationStatus.Reviewed });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // =================================================================
    // WithdrawApplicationAsync
    // =================================================================

    [Theory]
    [InlineData(ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.Reviewed)]
    [InlineData(ApplicationStatus.Shortlisted)]
    public async Task WithdrawApplicationAsync_FromValidState_ShouldSucceedAndPublishEvent(
        ApplicationStatus status)
    {
        var app = await SeedApplicationAsync(status: status);

        var result = await _sut.WithdrawApplicationAsync(CandidateId, app.ApplicationId);

        result.Status.Should().Be("Withdrawn");

        var events = _eventBus.GetEvents<ApplicationWithdrawnEvent>();
        events.Should().HaveCount(1);
        events[0].ApplicationId.Should().Be(app.ApplicationId);
        events[0].CandidateId.Should().Be(CandidateId);
        events[0].JobId.Should().Be(JobId);
    }

    [Fact]
    public async Task WithdrawApplicationAsync_WrongCandidate_ShouldThrowUnauthorizedAccessException()
    {
        var app = await SeedApplicationAsync();

        var act = () => _sut.WithdrawApplicationAsync("wrong-candidate", app.ApplicationId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*only withdraw your own*");
    }

    [Fact]
    public async Task WithdrawApplicationAsync_AlreadyWithdrawn_ShouldThrowInvalidOperationException()
    {
        var app = await SeedApplicationAsync(status: ApplicationStatus.Withdrawn);

        var act = () => _sut.WithdrawApplicationAsync(CandidateId, app.ApplicationId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already withdrawn*");
    }

    [Theory]
    [InlineData(ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Rejected)]
    public async Task WithdrawApplicationAsync_FromTerminalState_ShouldThrowInvalidOperationException(
        ApplicationStatus status)
    {
        var app = await SeedApplicationAsync(status: status);

        var act = () => _sut.WithdrawApplicationAsync(CandidateId, app.ApplicationId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Cannot withdraw*");
    }

    [Fact]
    public async Task WithdrawApplicationAsync_ApplicationNotFound_ShouldThrowInvalidOperationException()
    {
        var act = () => _sut.WithdrawApplicationAsync(CandidateId, Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // =================================================================
    // GetMyApplicationsAsync
    // =================================================================

    [Fact]
    public async Task GetMyApplicationsAsync_ShouldReturnPaginatedResults_ForCorrectCandidate()
    {
        for (int i = 0; i < 5; i++)
            await SeedApplicationAsync(candidateId: CandidateId, jobId: Guid.NewGuid());

        await SeedApplicationAsync(candidateId: "other-candidate", jobId: Guid.NewGuid());

        var result = await _sut.GetMyApplicationsAsync(CandidateId, page: 1, pageSize: 3);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.Items.Should().AllSatisfy(a => a.CandidateId.Should().Be(CandidateId));
    }

    [Fact]
    public async Task GetMyApplicationsAsync_SecondPage_ShouldReturnRemainingItems()
    {
        for (int i = 0; i < 5; i++)
            await SeedApplicationAsync(candidateId: CandidateId, jobId: Guid.NewGuid());

        var result = await _sut.GetMyApplicationsAsync(CandidateId, page: 2, pageSize: 3);

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
    }

    [Fact]
    public async Task GetMyApplicationsAsync_NoApplications_ShouldReturnEmptyList()
    {
        var result = await _sut.GetMyApplicationsAsync(CandidateId, page: 1, pageSize: 10);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    // =================================================================
    // GetApplicationByIdAsync
    // =================================================================

    [Fact]
    public async Task GetApplicationByIdAsync_CandidateViewsOwn_ShouldSucceed()
    {
        var app = await SeedApplicationAsync();

        var result = await _sut.GetApplicationByIdAsync(app.ApplicationId, CandidateId, "Candidate");

        result.Should().NotBeNull();
        result.ApplicationId.Should().Be(app.ApplicationId);
    }

    [Fact]
    public async Task GetApplicationByIdAsync_CandidateViewsOther_ShouldThrowUnauthorizedAccessException()
    {
        var app = await SeedApplicationAsync();

        var act = () => _sut.GetApplicationByIdAsync(app.ApplicationId, "other-candidate", "Candidate");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*only view your own*");
    }

    [Fact]
    public async Task GetApplicationByIdAsync_EmployerOwnsJob_ShouldSucceed()
    {
        var app = await SeedApplicationAsync();
        EnqueueJobOwnershipResponse(EmployerId);

        var result = await _sut.GetApplicationByIdAsync(app.ApplicationId, EmployerId, "Employer");

        result.Should().NotBeNull();
        result.ApplicationId.Should().Be(app.ApplicationId);
    }

    [Fact]
    public async Task GetApplicationByIdAsync_EmployerDoesNotOwnJob_ShouldThrowUnauthorizedAccessException()
    {
        var app = await SeedApplicationAsync();
        EnqueueJobOwnershipResponse("other-employer");

        var act = () => _sut.GetApplicationByIdAsync(app.ApplicationId, EmployerId, "Employer");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*only view applications for your own jobs*");
    }

    [Fact]
    public async Task GetApplicationByIdAsync_ApplicationNotFound_ShouldThrowInvalidOperationException()
    {
        var act = () => _sut.GetApplicationByIdAsync(Guid.NewGuid(), CandidateId, "Candidate");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // =================================================================
    // GetApplicationsForJobAsync
    // =================================================================

    [Fact]
    public async Task GetApplicationsForJobAsync_OwnerSeesApplications_ExcludesWithdrawn()
    {
        var jobId = Guid.NewGuid();
        await SeedApplicationAsync(status: ApplicationStatus.Submitted, jobId: jobId);
        await SeedApplicationAsync(status: ApplicationStatus.Reviewed, candidateId: "c2", jobId: jobId);
        await SeedApplicationAsync(status: ApplicationStatus.Withdrawn, candidateId: "c3", jobId: jobId);

        EnqueueJobOwnershipResponse(EmployerId);

        var result = await _sut.GetApplicationsForJobAsync(EmployerId, jobId, 1, 10);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().NotContain(a => a.Status == "Withdrawn");
    }

    [Fact]
    public async Task GetApplicationsForJobAsync_NotOwner_ShouldThrowUnauthorizedAccessException()
    {
        EnqueueJobOwnershipResponse("other-employer");

        var act = () => _sut.GetApplicationsForJobAsync(EmployerId, JobId, 1, 10);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // =================================================================
    // UpdateEmployerNotesAsync
    // =================================================================

    [Fact]
    public async Task UpdateEmployerNotesAsync_ShouldUpdateNotes()
    {
        var app = await SeedApplicationAsync();
        EnqueueJobOwnershipResponse(EmployerId);

        var request = new UpdateEmployerNotesRequest { Notes = "Good candidate" };

        var result = await _sut.UpdateEmployerNotesAsync(EmployerId, app.ApplicationId, request);

        result.EmployerNotes.Should().Be("Good candidate");
    }

    [Fact]
    public async Task UpdateEmployerNotesAsync_NotOwner_ShouldThrowUnauthorizedAccessException()
    {
        var app = await SeedApplicationAsync();
        EnqueueJobOwnershipResponse("other-employer");

        var request = new UpdateEmployerNotesRequest { Notes = "Notes" };

        var act = () => _sut.UpdateEmployerNotesAsync(EmployerId, app.ApplicationId, request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // =================================================================
    // HTTP / Job ownership verification edge cases
    // =================================================================

    [Fact]
    public async Task UpdateApplicationStatusAsync_JobServiceDown_ShouldThrowUnauthorizedAccessException()
    {
        var app = await SeedApplicationAsync();
        EnqueueJobOwnershipFailure();

        var request = new UpdateApplicationStatusRequest { Status = ApplicationStatus.Reviewed };

        var act = () => _sut.UpdateApplicationStatusAsync(EmployerId, app.ApplicationId, request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
