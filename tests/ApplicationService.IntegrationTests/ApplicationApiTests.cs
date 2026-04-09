using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ApplicationService.Application.DTOs;
using ApplicationService.Application.Interfaces;
using ApplicationService.Domain.Enums;
using ApplicationService.Infrastructure.Data;
using ApplicationService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Shared.TestUtilities;
using Shared.TestUtilities.Fakes;

namespace ApplicationService.IntegrationTests;

public class ApplicationApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program, ApplicationDbContext> _factory;
    private HttpClient _candidateClient = null!;
    private HttpClient _employerClient = null!;
    private HttpClient _anonymousClient = null!;
    private HttpClient _otherCandidateClient = null!;

    private const string CandidateId = "candidate-integ-001";
    private const string OtherCandidateId = "candidate-integ-002";
    private const string EmployerId = "employer-integ-001";
    private static readonly Guid TestJobId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    private readonly MockHttpMessageHandler _mockHttp = new();

    public ApplicationApiTests()
    {
        _factory = new CustomWebApplicationFactory<Program, ApplicationDbContext>(services =>
        {
            // Replace the typed HttpClient for ApplicationServiceImpl
            // so that VerifyJobOwnershipAsync hits our mock instead of a real service
            var httpClientDescriptors = services
                .Where(d => d.ServiceType == typeof(IHttpClientFactory)
                            || d.ServiceType == typeof(HttpClient)
                            || (d.ServiceType == typeof(IApplicationService)))
                .ToList();

            // Remove existing IApplicationService registration
            var appServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IApplicationService));
            if (appServiceDescriptor != null)
                services.Remove(appServiceDescriptor);

            // Re-register ApplicationServiceImpl with our mock HTTP handler
            services.AddScoped<IApplicationService>(sp =>
            {
                var db = sp.GetRequiredService<ApplicationDbContext>();
                var config = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ApplicationServiceImpl>>();
                var eventBus = sp.GetRequiredService<SharedKernel.Events.IEventBus>();
                var httpClient = new HttpClient(_mockHttp);
                return new ApplicationServiceImpl(db, httpClient, config, logger, eventBus);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _candidateClient = _factory.CreateAuthenticatedClient(CandidateId, "Candidate", "Test Candidate");
        _otherCandidateClient = _factory.CreateAuthenticatedClient(OtherCandidateId, "Candidate", "Other Candidate");
        _employerClient = _factory.CreateAuthenticatedClient(EmployerId, "Employer", "Test Employer");
        _anonymousClient = _factory.CreateClient();

        await _factory.ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        _candidateClient.Dispose();
        _otherCandidateClient.Dispose();
        _employerClient.Dispose();
        _anonymousClient.Dispose();
        await ((IAsyncLifetime)_factory).DisposeAsync();
    }

    private void EnqueueJobOwnershipResponse(Guid jobId, string employerId)
    {
        var jobJson = JsonSerializer.Serialize(new { employerId, jobId, title = "Test Job" });
        _mockHttp.EnqueueResponse(HttpStatusCode.OK, jobJson);
    }

    private CreateApplicationRequest MakeApplicationRequest(Guid? jobId = null) => new()
    {
        JobId = jobId ?? TestJobId,
        CoverLetter = "I am very interested in this position and have relevant experience.",
        ResumeUrl = "https://example.com/resume.pdf"
    };

    // ----------------------------------------------------------------
    // Authentication & Authorization
    // ----------------------------------------------------------------

    [Fact]
    public async Task PostApplication_WithoutAuth_Returns401()
    {
        var request = MakeApplicationRequest();
        var response = await _anonymousClient.PostAsJsonAsync("/api/applications", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostApplication_AsEmployer_Returns403()
    {
        var request = MakeApplicationRequest();
        var response = await _employerClient.PostAsJsonAsync("/api/applications", request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ----------------------------------------------------------------
    // Submit application -> verify in list -> update status pipeline
    // ----------------------------------------------------------------

    [Fact]
    public async Task FullApplicationLifecycle_SubmitListUpdateStatus()
    {
        // 1. Submit application
        var request = MakeApplicationRequest();
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        created.Should().NotBeNull();
        created!.Status.Should().Be("Submitted");
        created.CandidateId.Should().Be(CandidateId);
        created.JobId.Should().Be(TestJobId);

        var appId = created.ApplicationId;

        // 2. Verify in my-applications list
        var myApps = await _candidateClient.GetFromJsonAsync<ApplicationListResponse>(
            "/api/applications/my-applications");
        myApps!.Items.Should().Contain(a => a.ApplicationId == appId);

        // 3. Employer reviews (Submitted -> Reviewed)
        EnqueueJobOwnershipResponse(TestJobId, EmployerId);
        var reviewResponse = await _employerClient.PatchAsJsonAsync(
            $"/api/applications/{appId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Reviewed });
        reviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewed = await reviewResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        reviewed!.Status.Should().Be("Reviewed");

        // 4. Employer shortlists (Reviewed -> Shortlisted)
        EnqueueJobOwnershipResponse(TestJobId, EmployerId);
        var shortlistResponse = await _employerClient.PatchAsJsonAsync(
            $"/api/applications/{appId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Shortlisted });
        shortlistResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var shortlisted = await shortlistResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        shortlisted!.Status.Should().Be("Shortlisted");

        // 5. Employer accepts (Shortlisted -> Accepted)
        EnqueueJobOwnershipResponse(TestJobId, EmployerId);
        var acceptResponse = await _employerClient.PatchAsJsonAsync(
            $"/api/applications/{appId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Accepted });
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var accepted = await acceptResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        accepted!.Status.Should().Be("Accepted");
    }

    // ----------------------------------------------------------------
    // Duplicate application returns 409
    // ----------------------------------------------------------------

    [Fact]
    public async Task SubmitDuplicateApplication_Returns409()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);

        // First application succeeds
        var first = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        // Duplicate should fail
        var duplicate = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ----------------------------------------------------------------
    // Invalid status transition returns 400
    // ----------------------------------------------------------------

    [Fact]
    public async Task UpdateStatus_InvalidTransition_Returns400()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        // Try Submitted -> Accepted directly (skipping Reviewed and Shortlisted)
        EnqueueJobOwnershipResponse(jobId, EmployerId);
        var response = await _employerClient.PatchAsJsonAsync(
            $"/api/applications/{created!.ApplicationId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Accepted });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ----------------------------------------------------------------
    // Withdraw application
    // ----------------------------------------------------------------

    [Fact]
    public async Task WithdrawApplication_Success()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var withdrawResponse = await _candidateClient.PutAsJsonAsync(
            $"/api/applications/{created!.ApplicationId}/withdraw", new { });
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var withdrawn = await withdrawResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        withdrawn!.Status.Should().Be("Withdrawn");
    }

    [Fact]
    public async Task WithdrawApplication_AlreadyWithdrawn_Returns400()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        // Withdraw first time
        await _candidateClient.PutAsJsonAsync(
            $"/api/applications/{created!.ApplicationId}/withdraw", new { });

        // Try withdrawing again
        var response = await _candidateClient.PutAsJsonAsync(
            $"/api/applications/{created.ApplicationId}/withdraw", new { });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WithdrawApplication_ByOtherCandidate_Returns403()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        // Other candidate tries to withdraw
        var response = await _otherCandidateClient.PutAsJsonAsync(
            $"/api/applications/{created!.ApplicationId}/withdraw", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ----------------------------------------------------------------
    // Cannot withdraw accepted/rejected applications
    // ----------------------------------------------------------------

    [Fact]
    public async Task WithdrawApplication_AfterAccepted_Returns400()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        var appId = created!.ApplicationId;

        // Move through pipeline to Accepted
        EnqueueJobOwnershipResponse(jobId, EmployerId);
        await _employerClient.PatchAsJsonAsync($"/api/applications/{appId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Reviewed });

        EnqueueJobOwnershipResponse(jobId, EmployerId);
        await _employerClient.PatchAsJsonAsync($"/api/applications/{appId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Shortlisted });

        EnqueueJobOwnershipResponse(jobId, EmployerId);
        await _employerClient.PatchAsJsonAsync($"/api/applications/{appId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Accepted });

        // Try to withdraw
        var response = await _candidateClient.PutAsJsonAsync(
            $"/api/applications/{appId}/withdraw", new { });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ----------------------------------------------------------------
    // Employer notes
    // ----------------------------------------------------------------

    [Fact]
    public async Task UpdateEmployerNotes_Success()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        EnqueueJobOwnershipResponse(jobId, EmployerId);
        var notesResponse = await _employerClient.PatchAsJsonAsync(
            $"/api/applications/{created!.ApplicationId}/notes",
            new UpdateEmployerNotesRequest { Notes = "Strong candidate, schedule interview." });

        notesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await notesResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        updated!.EmployerNotes.Should().Be("Strong candidate, schedule interview.");
    }

    // ----------------------------------------------------------------
    // Get application by ID - authorization
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetApplication_ByCandidateOwner_Returns200()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var getResponse = await _candidateClient.GetAsync(
            $"/api/applications/{created!.ApplicationId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetApplication_ByOtherCandidate_Returns403()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var getResponse = await _otherCandidateClient.GetAsync(
            $"/api/applications/{created!.ApplicationId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ----------------------------------------------------------------
    // Events published on submit
    // ----------------------------------------------------------------

    [Fact]
    public async Task SubmitApplication_PublishesApplicationSubmittedEvent()
    {
        _factory.EventBus.Clear();

        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        await _candidateClient.PostAsJsonAsync("/api/applications", request);

        _factory.EventBus.PublishedEvents.Should().ContainSingle(e =>
            e.GetType().Name == "ApplicationSubmittedEvent");
    }

    // ----------------------------------------------------------------
    // Withdraw publishes event
    // ----------------------------------------------------------------

    [Fact]
    public async Task WithdrawApplication_PublishesApplicationWithdrawnEvent()
    {
        _factory.EventBus.Clear();

        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        _factory.EventBus.Clear();
        await _candidateClient.PutAsJsonAsync(
            $"/api/applications/{created!.ApplicationId}/withdraw", new { });

        _factory.EventBus.PublishedEvents.Should().ContainSingle(e =>
            e.GetType().Name == "ApplicationWithdrawnEvent");
    }

    // ----------------------------------------------------------------
    // Status change publishes event
    // ----------------------------------------------------------------

    [Fact]
    public async Task UpdateStatus_PublishesApplicationStatusChangedEvent()
    {
        _factory.EventBus.Clear();

        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        _factory.EventBus.Clear();
        EnqueueJobOwnershipResponse(jobId, EmployerId);
        await _employerClient.PatchAsJsonAsync(
            $"/api/applications/{created!.ApplicationId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Reviewed });

        _factory.EventBus.PublishedEvents.Should().ContainSingle(e =>
            e.GetType().Name == "ApplicationStatusChangedEvent");
    }

    // ----------------------------------------------------------------
    // Cannot update withdrawn application
    // ----------------------------------------------------------------

    [Fact]
    public async Task UpdateStatus_WithdrawnApplication_Returns400()
    {
        var jobId = Guid.NewGuid();
        var request = MakeApplicationRequest(jobId);
        var createResponse = await _candidateClient.PostAsJsonAsync("/api/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
        var appId = created!.ApplicationId;

        // Withdraw it
        await _candidateClient.PutAsJsonAsync($"/api/applications/{appId}/withdraw", new { });

        // Try to update status
        EnqueueJobOwnershipResponse(jobId, EmployerId);
        var response = await _employerClient.PatchAsJsonAsync(
            $"/api/applications/{appId}/status",
            new UpdateApplicationStatusRequest { Status = ApplicationStatus.Reviewed });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
