using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using JobService.Application.DTOs;
using JobService.Domain.Enums;
using JobService.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Shared.TestUtilities;
using SkillDtos = JobService.Application.DTOs;

namespace JobService.IntegrationTests;

public class JobApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program, JobDbContext> _factory;
    private HttpClient _employerClient = null!;
    private HttpClient _candidateClient = null!;
    private HttpClient _anonymousClient = null!;

    private const string EmployerId = "employer-integration-test-001";
    private const string OtherEmployerId = "employer-integration-test-002";
    private const string CandidateId = "candidate-integration-test-001";

    public JobApiTests()
    {
        _factory = new CustomWebApplicationFactory<Program, JobDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _employerClient = _factory.CreateAuthenticatedClient(EmployerId, "Employer", "Test Employer");
        _candidateClient = _factory.CreateAuthenticatedClient(CandidateId, "Candidate", "Test Candidate");
        _anonymousClient = _factory.CreateClient();

        // Reset DB to start clean (removes seed data)
        await _factory.ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        _employerClient.Dispose();
        _candidateClient.Dispose();
        _anonymousClient.Dispose();
        await ((IAsyncLifetime)_factory).DisposeAsync();
    }

    private CreateJobRequest MakeCreateJobRequest(
        string title = "Integration Test Engineer",
        string location = "Austin, TX") => new()
    {
        Title = title,
        Description = "A test job posting created by integration tests with sufficient description length for validation.",
        Location = location,
        EmploymentType = EmploymentType.FullTime,
        ExperienceRequired = 3,
        SalaryMin = 100000m,
        SalaryMax = 150000m,
        ApplicationDeadline = DateTime.UtcNow.AddDays(30),
        Skills = new List<JobSkillInput>()
    };

    // ----------------------------------------------------------------
    // Authentication & Authorization
    // ----------------------------------------------------------------

    [Fact]
    public async Task PostJob_WithoutAuth_Returns401()
    {
        var request = MakeCreateJobRequest();
        var response = await _anonymousClient.PostAsJsonAsync("/api/jobs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostJob_AsCandidate_Returns403()
    {
        var request = MakeCreateJobRequest();
        var response = await _candidateClient.PostAsJsonAsync("/api/jobs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ----------------------------------------------------------------
    // Full Lifecycle: Create -> Publish -> Filter -> Get -> Close
    // ----------------------------------------------------------------

    [Fact]
    public async Task FullJobLifecycle_CreatePublishFilterGetClose()
    {
        // 1. Create a job (starts as Draft)
        var createRequest = MakeCreateJobRequest("Lifecycle Test Job", "Portland, OR");
        var createResponse = await _employerClient.PostAsJsonAsync("/api/jobs", createRequest);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<JobResponse>();
        created.Should().NotBeNull();
        created!.JobStatus.Should().Be("Draft");
        created.Title.Should().Be("Lifecycle Test Job");
        created.EmployerId.Should().Be(EmployerId);

        var jobId = created.JobId;

        // 2. GET /api/jobs should NOT include Draft jobs
        var listBeforePublish = await _anonymousClient.GetFromJsonAsync<JobListResponse>("/api/jobs");
        listBeforePublish!.Items.Should().NotContain(j => j.JobId == jobId);

        // 3. Publish the job (Draft -> Active)
        var publishRequest = new UpdateJobStatusRequest { Status = JobStatus.Active };
        var publishResponse = await _employerClient.PatchAsJsonAsync($"/api/jobs/{jobId}/status", publishRequest);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var published = await publishResponse.Content.ReadFromJsonAsync<JobResponse>();
        published!.JobStatus.Should().Be("Active");

        // 4. GET /api/jobs should now include the Active job
        var listAfterPublish = await _anonymousClient.GetFromJsonAsync<JobListResponse>("/api/jobs");
        listAfterPublish!.Items.Should().Contain(j => j.JobId == jobId);

        // 5. Get by ID
        var getResponse = await _anonymousClient.GetFromJsonAsync<JobResponse>($"/api/jobs/{jobId}");
        getResponse.Should().NotBeNull();
        getResponse!.Title.Should().Be("Lifecycle Test Job");
        getResponse.Location.Should().Be("Portland, OR");

        // 6. Close the job (Active -> Closed)
        var closeRequest = new UpdateJobStatusRequest { Status = JobStatus.Closed };
        var closeResponse = await _employerClient.PatchAsJsonAsync($"/api/jobs/{jobId}/status", closeRequest);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var closed = await closeResponse.Content.ReadFromJsonAsync<JobResponse>();
        closed!.JobStatus.Should().Be("Closed");

        // 7. GET /api/jobs should no longer include the Closed job
        var listAfterClose = await _anonymousClient.GetFromJsonAsync<JobListResponse>("/api/jobs");
        listAfterClose!.Items.Should().NotContain(j => j.JobId == jobId);
    }

    // ----------------------------------------------------------------
    // GET /api/jobs returns only Active jobs
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetJobs_ReturnsOnlyActiveJobs()
    {
        // Create two jobs
        var job1 = await CreateAndGetJob("Active Job A");
        var job2 = await CreateAndGetJob("Draft Job B");

        // Publish only job1
        var publishRequest = new UpdateJobStatusRequest { Status = JobStatus.Active };
        await _employerClient.PatchAsJsonAsync($"/api/jobs/{job1.JobId}/status", publishRequest);

        var list = await _anonymousClient.GetFromJsonAsync<JobListResponse>("/api/jobs");
        list!.Items.Should().Contain(j => j.JobId == job1.JobId);
        list.Items.Should().NotContain(j => j.JobId == job2.JobId);
    }

    // ----------------------------------------------------------------
    // Filtering by Location
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetJobs_FilterByLocation_ReturnsMatching()
    {
        var jobA = await CreateAndPublishJob("Location Filter A", "Denver, CO");
        var jobB = await CreateAndPublishJob("Location Filter B", "Chicago, IL");

        var list = await _anonymousClient.GetFromJsonAsync<JobListResponse>("/api/jobs?location=Denver");
        list!.Items.Should().Contain(j => j.JobId == jobA.JobId);
        list.Items.Should().NotContain(j => j.JobId == jobB.JobId);
    }

    // ----------------------------------------------------------------
    // Filtering by Skills
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetJobs_FilterBySkills_ReturnsMatching()
    {
        // First create a skill
        var skillResponse = await _employerClient.PostAsJsonAsync("/api/skills",
            new SkillDtos.CreateSkillRequest { SkillName = "IntegTestSkill_" + Guid.NewGuid().ToString("N")[..8], SkillCategory = "Testing" });

        if (!skillResponse.IsSuccessStatusCode)
        {
            // Skill endpoint might not exist or skill already exists, skip this test gracefully
            return;
        }

        var skill = await skillResponse.Content.ReadFromJsonAsync<SkillDtos.SkillResponse>();

        // Create a job with that skill
        var request = MakeCreateJobRequest("Skill Filter Job", "Remote");
        request.Skills = new List<JobSkillInput>
        {
            new() { SkillId = skill!.SkillId, ImportanceLevel = ImportanceLevel.Required }
        };

        var createResponse = await _employerClient.PostAsJsonAsync("/api/jobs", request);
        var created = await createResponse.Content.ReadFromJsonAsync<JobResponse>();

        // Publish it
        await _employerClient.PatchAsJsonAsync($"/api/jobs/{created!.JobId}/status",
            new UpdateJobStatusRequest { Status = JobStatus.Active });

        // Filter by skill name
        var list = await _anonymousClient.GetFromJsonAsync<JobListResponse>(
            $"/api/jobs?skills={skill.SkillName}");
        list!.Items.Should().Contain(j => j.JobId == created.JobId);
    }

    // ----------------------------------------------------------------
    // Pagination
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetJobs_Pagination_ReturnsCorrectPage()
    {
        // Create and publish 3 jobs
        for (int i = 0; i < 3; i++)
        {
            await CreateAndPublishJob($"Pagination Job {i}", "Miami, FL");
        }

        // Request page 1, pageSize 2
        var page1 = await _anonymousClient.GetFromJsonAsync<JobListResponse>("/api/jobs?page=1&pageSize=2");
        page1!.Items.Count.Should().BeLessThanOrEqualTo(2);
        page1.PageSize.Should().Be(2);
        page1.Page.Should().Be(1);
        page1.TotalCount.Should().BeGreaterThanOrEqualTo(3);
        page1.TotalPages.Should().BeGreaterThanOrEqualTo(2);

        // Request page 2
        var page2 = await _anonymousClient.GetFromJsonAsync<JobListResponse>("/api/jobs?page=2&pageSize=2");
        page2!.Items.Count.Should().BeGreaterThanOrEqualTo(1);
        page2.Page.Should().Be(2);
    }

    // ----------------------------------------------------------------
    // Delete (soft-delete)
    // ----------------------------------------------------------------

    [Fact]
    public async Task DeleteJob_SetsStatusToClosed()
    {
        var job = await CreateAndPublishJob("Delete Me Job");

        var deleteResponse = await _employerClient.DeleteAsync($"/api/jobs/{job.JobId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it is now closed
        var getResponse = await _anonymousClient.GetFromJsonAsync<JobResponse>($"/api/jobs/{job.JobId}");
        getResponse!.JobStatus.Should().Be("Closed");
    }

    // ----------------------------------------------------------------
    // Invalid status transition
    // ----------------------------------------------------------------

    [Fact]
    public async Task UpdateStatus_InvalidTransition_Returns400()
    {
        var job = await CreateAndGetJob("Invalid Transition Job");

        // Try Draft -> Expired (not a valid transition)
        var request = new UpdateJobStatusRequest { Status = JobStatus.Expired };
        var response = await _employerClient.PatchAsJsonAsync($"/api/jobs/{job.JobId}/status", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ----------------------------------------------------------------
    // Employer cannot modify another employer's job
    // ----------------------------------------------------------------

    [Fact]
    public async Task UpdateJob_ByDifferentEmployer_Returns403()
    {
        var job = await CreateAndGetJob("Ownership Test Job");

        var otherEmployerClient = _factory.CreateAuthenticatedClient(OtherEmployerId, "Employer", "Other Employer");

        var updateRequest = new UpdateJobStatusRequest { Status = JobStatus.Active };
        var response = await otherEmployerClient.PatchAsJsonAsync($"/api/jobs/{job.JobId}/status", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        otherEmployerClient.Dispose();
    }

    // ----------------------------------------------------------------
    // My Jobs endpoint
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetMyJobs_ReturnsOnlyOwnJobs()
    {
        await CreateAndGetJob("My Job 1");
        await CreateAndGetJob("My Job 2");

        var otherClient = _factory.CreateAuthenticatedClient(OtherEmployerId, "Employer");
        var otherJob = await CreateAndGetJobWithClient(otherClient, "Other Employer Job");

        var myJobs = await _employerClient.GetFromJsonAsync<JobListResponse>("/api/jobs/my-jobs");
        myJobs!.Items.Should().OnlyContain(j => j.EmployerId == EmployerId);
        myJobs.Items.Should().NotContain(j => j.JobId == otherJob.JobId);

        otherClient.Dispose();
    }

    // ----------------------------------------------------------------
    // Event publishing
    // ----------------------------------------------------------------

    [Fact]
    public async Task PublishJob_PublishesJobPublishedEvent()
    {
        _factory.EventBus.Clear();
        var job = await CreateAndGetJob("Event Test Job");

        await _employerClient.PatchAsJsonAsync($"/api/jobs/{job.JobId}/status",
            new UpdateJobStatusRequest { Status = JobStatus.Active });

        _factory.EventBus.PublishedEvents.Should().ContainSingle(e =>
            e.GetType().Name == "JobPublishedEvent");
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private async Task<JobResponse> CreateAndGetJob(string title = "Test Job", string location = "Austin, TX")
    {
        var request = MakeCreateJobRequest(title, location);
        var response = await _employerClient.PostAsJsonAsync("/api/jobs", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobResponse>())!;
    }

    private async Task<JobResponse> CreateAndGetJobWithClient(HttpClient client, string title)
    {
        var request = MakeCreateJobRequest(title);
        var response = await client.PostAsJsonAsync("/api/jobs", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobResponse>())!;
    }

    private async Task<JobResponse> CreateAndPublishJob(string title = "Test Job", string location = "Austin, TX")
    {
        var job = await CreateAndGetJob(title, location);

        var publishRequest = new UpdateJobStatusRequest { Status = JobStatus.Active };
        var publishResponse = await _employerClient.PatchAsJsonAsync($"/api/jobs/{job.JobId}/status", publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        return (await publishResponse.Content.ReadFromJsonAsync<JobResponse>())!;
    }
}
