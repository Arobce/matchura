using FluentAssertions;
using JobService.Application.DTOs;
using JobService.Domain.Entities;
using JobService.Domain.Enums;
using JobService.Infrastructure.Data;
using JobService.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.TestUtilities;
using Shared.TestUtilities.Fakes;
using SharedKernel.Events;

namespace JobService.UnitTests;

public class JobServiceImplTests : IDisposable
{
    private readonly JobDbContext _db;
    private readonly SqliteConnection _connection;
    private readonly FakeEventBus _eventBus;
    private readonly JobServiceImpl _sut;

    private const string EmployerId = "employer-123";
    private const string OtherEmployerId = "employer-999";

    public JobServiceImplTests()
    {
        (_db, _connection) = DbContextFactory.Create<JobDbContext>();

        // SQLite does not support gen_random_uuid(), so assign GUIDs client-side before saving.
        _db.SavingChanges += (sender, _) =>
        {
            if (sender is not JobDbContext ctx) return;
            foreach (var entry in ctx.ChangeTracker.Entries<Job>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.JobId == Guid.Empty)
                    entry.Entity.JobId = Guid.NewGuid();
            }
            foreach (var entry in ctx.ChangeTracker.Entries<Skill>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.SkillId == Guid.Empty)
                    entry.Entity.SkillId = Guid.NewGuid();
            }
            foreach (var entry in ctx.ChangeTracker.Entries<JobSkill>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.JobSkillId == Guid.Empty)
                    entry.Entity.JobSkillId = Guid.NewGuid();
            }
        };

        _eventBus = new FakeEventBus();
        var logger = new Mock<ILogger<JobServiceImpl>>();
        _sut = new JobServiceImpl(_db, logger.Object, _eventBus);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ── Helpers ──

    private CreateJobRequest MakeCreateRequest(List<JobSkillInput>? skills = null) => new()
    {
        Title = "Software Engineer",
        Description = "Build cool stuff",
        Location = "New York",
        EmploymentType = EmploymentType.FullTime,
        ExperienceRequired = 3,
        SalaryMin = 80_000,
        SalaryMax = 120_000,
        ApplicationDeadline = DateTime.UtcNow.AddDays(30),
        Skills = skills ?? []
    };

    private async Task<Skill> SeedSkillAsync(string name = "C#", string? category = "Programming")
    {
        var skill = new Skill { SkillId = Guid.NewGuid(), SkillName = name, SkillCategory = category };
        _db.Skills.Add(skill);
        await _db.SaveChangesAsync();
        return skill;
    }

    private async Task<Job> SeedJobAsync(
        string employerId = EmployerId,
        JobStatus status = JobStatus.Draft,
        string title = "Test Job",
        string? location = "Remote",
        EmploymentType employmentType = EmploymentType.FullTime,
        decimal? salaryMin = null,
        decimal? salaryMax = null)
    {
        var job = new Job
        {
            JobId = Guid.NewGuid(),
            EmployerId = employerId,
            Title = title,
            Description = "A test job description",
            Location = location,
            EmploymentType = employmentType,
            ExperienceRequired = 2,
            SalaryMin = salaryMin,
            SalaryMax = salaryMax,
            JobStatus = status,
            PostedAt = status == JobStatus.Active ? DateTime.UtcNow : default,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
        return job;
    }

    private async Task<Job> SeedActiveJobWithSkillAsync(
        string skillName,
        string employerId = EmployerId,
        string? location = "Remote",
        EmploymentType employmentType = EmploymentType.FullTime,
        decimal? salaryMin = null,
        decimal? salaryMax = null)
    {
        var skill = await SeedSkillAsync(skillName, "Tech");
        var job = await SeedJobAsync(
            employerId, JobStatus.Active, $"Job requiring {skillName}",
            location, employmentType, salaryMin, salaryMax);
        _db.JobSkills.Add(new JobSkill
        {
            JobSkillId = Guid.NewGuid(),
            JobId = job.JobId,
            SkillId = skill.SkillId,
            ImportanceLevel = ImportanceLevel.Required
        });
        await _db.SaveChangesAsync();
        return job;
    }

    // ── CreateJobAsync ──

    [Fact]
    public async Task CreateJobAsync_SetsStatusToDraft_AndReturnsCorrectData()
    {
        var request = MakeCreateRequest();

        var result = await _sut.CreateJobAsync(EmployerId, request);

        result.JobStatus.Should().Be("Draft");
        result.EmployerId.Should().Be(EmployerId);
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Location.Should().Be(request.Location);
        result.EmploymentType.Should().Be(EmploymentType.FullTime.ToString());
        result.ExperienceRequired.Should().Be(3);
        result.SalaryMin.Should().Be(80_000);
        result.SalaryMax.Should().Be(120_000);
    }

    [Fact]
    public async Task CreateJobAsync_AttachesValidSkills()
    {
        var skill1 = await SeedSkillAsync("C#", "Programming");
        var skill2 = await SeedSkillAsync("SQL", "Database");
        var bogusId = Guid.NewGuid();

        var request = MakeCreateRequest(skills:
        [
            new JobSkillInput { SkillId = skill1.SkillId, ImportanceLevel = ImportanceLevel.Required },
            new JobSkillInput { SkillId = skill2.SkillId, ImportanceLevel = ImportanceLevel.Preferred },
            new JobSkillInput { SkillId = bogusId, ImportanceLevel = ImportanceLevel.NiceToHave }
        ]);

        var result = await _sut.CreateJobAsync(EmployerId, request);

        result.Skills.Should().HaveCount(2);
        result.Skills.Should().Contain(s => s.SkillName == "C#" && s.ImportanceLevel == "Required");
        result.Skills.Should().Contain(s => s.SkillName == "SQL" && s.ImportanceLevel == "Preferred");
    }

    [Fact]
    public async Task CreateJobAsync_WithNoSkills_CreatesJobWithEmptySkillsList()
    {
        var request = MakeCreateRequest();

        var result = await _sut.CreateJobAsync(EmployerId, request);

        result.Skills.Should().BeEmpty();
    }

    // ── UpdateJobStatusAsync ──

    [Fact]
    public async Task UpdateJobStatusAsync_DraftToActive_PublishesEventAndSetsPostedAt()
    {
        var job = await SeedJobAsync();

        var result = await _sut.UpdateJobStatusAsync(
            EmployerId, job.JobId,
            new UpdateJobStatusRequest { Status = JobStatus.Active });

        result.JobStatus.Should().Be("Active");
        result.PostedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var events = _eventBus.GetEvents<JobPublishedEvent>();
        events.Should().ContainSingle();
        events[0].JobId.Should().Be(job.JobId);
        events[0].EmployerId.Should().Be(EmployerId);
    }

    [Fact]
    public async Task UpdateJobStatusAsync_ActiveToClosed_Succeeds()
    {
        var job = await SeedJobAsync(status: JobStatus.Active);

        var result = await _sut.UpdateJobStatusAsync(
            EmployerId, job.JobId,
            new UpdateJobStatusRequest { Status = JobStatus.Closed });

        result.JobStatus.Should().Be("Closed");
        _eventBus.PublishedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateJobStatusAsync_DraftToClosed_Succeeds()
    {
        var job = await SeedJobAsync();

        var result = await _sut.UpdateJobStatusAsync(
            EmployerId, job.JobId,
            new UpdateJobStatusRequest { Status = JobStatus.Closed });

        result.JobStatus.Should().Be("Closed");
    }

    [Theory]
    [InlineData(JobStatus.Active, JobStatus.Draft)]
    [InlineData(JobStatus.Closed, JobStatus.Active)]
    [InlineData(JobStatus.Closed, JobStatus.Draft)]
    [InlineData(JobStatus.Active, JobStatus.Active)]
    public async Task UpdateJobStatusAsync_InvalidTransition_Throws(JobStatus from, JobStatus to)
    {
        var job = await SeedJobAsync(status: from);

        var act = () => _sut.UpdateJobStatusAsync(
            EmployerId, job.JobId,
            new UpdateJobStatusRequest { Status = to });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot transition*");
    }

    [Fact]
    public async Task UpdateJobStatusAsync_WrongEmployer_ThrowsUnauthorized()
    {
        var job = await SeedJobAsync();

        var act = () => _sut.UpdateJobStatusAsync(
            OtherEmployerId, job.JobId,
            new UpdateJobStatusRequest { Status = JobStatus.Active });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── UpdateJobAsync ──

    [Fact]
    public async Task UpdateJobAsync_UpdatesFields()
    {
        var job = await SeedJobAsync();
        var request = new UpdateJobRequest
        {
            Title = "Updated Title",
            Description = "Updated Desc",
            Location = "Boston",
            EmploymentType = EmploymentType.Contract,
            ExperienceRequired = 5,
            SalaryMin = 100_000,
            SalaryMax = 150_000,
            ApplicationDeadline = DateTime.UtcNow.AddDays(60),
            Skills = []
        };

        var result = await _sut.UpdateJobAsync(EmployerId, job.JobId, request);

        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Desc");
        result.Location.Should().Be("Boston");
        result.EmploymentType.Should().Be("Contract");
        result.ExperienceRequired.Should().Be(5);
        result.SalaryMin.Should().Be(100_000);
        result.SalaryMax.Should().Be(150_000);
    }

    [Fact]
    public async Task UpdateJobAsync_WrongEmployer_ThrowsUnauthorized()
    {
        var job = await SeedJobAsync();

        var act = () => _sut.UpdateJobAsync(
            OtherEmployerId, job.JobId,
            new UpdateJobRequest { Title = "X", Description = "Y", Skills = [] });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateJobAsync_ClosedJob_Throws()
    {
        var job = await SeedJobAsync(status: JobStatus.Closed);

        var act = () => _sut.UpdateJobAsync(
            EmployerId, job.JobId,
            new UpdateJobRequest { Title = "X", Description = "Y", Skills = [] });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*closed*");
    }

    [Fact]
    public async Task UpdateJobAsync_ReplacesSkills()
    {
        var skill1 = await SeedSkillAsync("C#", "Programming");
        var skill2 = await SeedSkillAsync("Go", "Programming");

        var createReq = MakeCreateRequest(skills:
        [
            new JobSkillInput { SkillId = skill1.SkillId, ImportanceLevel = ImportanceLevel.Required }
        ]);
        var created = await _sut.CreateJobAsync(EmployerId, createReq);
        created.Skills.Should().HaveCount(1);

        var updateReq = new UpdateJobRequest
        {
            Title = "Updated",
            Description = "Updated",
            Skills =
            [
                new JobSkillInput { SkillId = skill2.SkillId, ImportanceLevel = ImportanceLevel.Preferred }
            ]
        };

        var updated = await _sut.UpdateJobAsync(EmployerId, created.JobId, updateReq);

        updated.Skills.Should().HaveCount(1);
        updated.Skills[0].SkillName.Should().Be("Go");
        updated.Skills[0].ImportanceLevel.Should().Be("Preferred");
    }

    // ── DeleteJobAsync ──

    [Fact]
    public async Task DeleteJobAsync_SoftDeleteSetsStatusToClosed()
    {
        var job = await SeedJobAsync(status: JobStatus.Active);

        await _sut.DeleteJobAsync(EmployerId, job.JobId);

        var deleted = await _sut.GetJobByIdAsync(job.JobId);
        deleted.JobStatus.Should().Be("Closed");
    }

    [Fact]
    public async Task DeleteJobAsync_WrongEmployer_ThrowsUnauthorized()
    {
        var job = await SeedJobAsync();

        var act = () => _sut.DeleteJobAsync(OtherEmployerId, job.JobId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── GetJobsAsync ──

    [Fact]
    public async Task GetJobsAsync_OnlyReturnsActiveJobs()
    {
        await SeedJobAsync(status: JobStatus.Draft, title: "Draft Job");
        await SeedJobAsync(status: JobStatus.Active, title: "Active Job");
        await SeedJobAsync(status: JobStatus.Closed, title: "Closed Job");

        var result = await _sut.GetJobsAsync(new JobQueryParams());

        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Active Job");
    }

    [Fact]
    public async Task GetJobsAsync_FiltersByLocation()
    {
        await SeedJobAsync(status: JobStatus.Active, title: "NYC Job", location: "New York");
        await SeedJobAsync(status: JobStatus.Active, title: "LA Job", location: "Los Angeles");

        var result = await _sut.GetJobsAsync(new JobQueryParams { Location = "new york" });

        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("NYC Job");
    }

    [Fact]
    public async Task GetJobsAsync_FiltersByEmploymentType()
    {
        await SeedJobAsync(status: JobStatus.Active, title: "FT", employmentType: EmploymentType.FullTime);
        await SeedJobAsync(status: JobStatus.Active, title: "PT", employmentType: EmploymentType.PartTime);

        var result = await _sut.GetJobsAsync(new JobQueryParams { EmploymentType = EmploymentType.PartTime });

        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("PT");
    }

    [Fact]
    public async Task GetJobsAsync_FiltersBySalaryRange()
    {
        await SeedJobAsync(status: JobStatus.Active, title: "Low Pay", salaryMin: 30_000, salaryMax: 50_000);
        await SeedJobAsync(status: JobStatus.Active, title: "High Pay", salaryMin: 100_000, salaryMax: 150_000);

        var result = await _sut.GetJobsAsync(new JobQueryParams { MinSalary = 60_000 });

        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("High Pay");
    }

    [Fact]
    public async Task GetJobsAsync_FiltersBySkills()
    {
        await SeedActiveJobWithSkillAsync("React");
        await SeedActiveJobWithSkillAsync("Python");

        var result = await _sut.GetJobsAsync(new JobQueryParams { Skills = "React" });

        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Contain("React");
    }

    [Fact]
    public async Task GetJobsAsync_PaginationWorksCorrectly()
    {
        for (int i = 0; i < 5; i++)
            await SeedJobAsync(status: JobStatus.Active, title: $"Job {i}");

        var page1 = await _sut.GetJobsAsync(new JobQueryParams { Page = 1, PageSize = 2 });
        var page2 = await _sut.GetJobsAsync(new JobQueryParams { Page = 2, PageSize = 2 });
        var page3 = await _sut.GetJobsAsync(new JobQueryParams { Page = 3, PageSize = 2 });

        page1.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);
        page1.TotalPages.Should().Be(3);

        page2.Items.Should().HaveCount(2);
        page3.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetJobsAsync_SearchByTitleOrDescription()
    {
        await SeedJobAsync(status: JobStatus.Active, title: "Frontend Developer");
        await SeedJobAsync(status: JobStatus.Active, title: "Backend Engineer");

        var result = await _sut.GetJobsAsync(new JobQueryParams { Search = "frontend" });

        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Frontend Developer");
    }

    // ── GetMyJobsAsync ──

    [Fact]
    public async Task GetMyJobsAsync_ReturnsOnlyEmployersOwnJobs()
    {
        await SeedJobAsync(employerId: EmployerId, title: "My Job 1");
        await SeedJobAsync(employerId: EmployerId, title: "My Job 2");
        await SeedJobAsync(employerId: OtherEmployerId, title: "Someone Else Job");

        var result = await _sut.GetMyJobsAsync(EmployerId, 1, 10);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(j => j.EmployerId == EmployerId);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetMyJobsAsync_ReturnsAllStatuses()
    {
        await SeedJobAsync(employerId: EmployerId, status: JobStatus.Draft, title: "Draft");
        await SeedJobAsync(employerId: EmployerId, status: JobStatus.Active, title: "Active");
        await SeedJobAsync(employerId: EmployerId, status: JobStatus.Closed, title: "Closed");

        var result = await _sut.GetMyJobsAsync(EmployerId, 1, 10);

        result.Items.Should().HaveCount(3);
    }

    // ── GetSkillsAsync ──

    [Fact]
    public async Task GetSkillsAsync_ReturnsAllSkills()
    {
        await SeedSkillAsync("C#", "Programming");
        await SeedSkillAsync("SQL", "Database");
        await SeedSkillAsync("Docker", "DevOps");

        var result = await _sut.GetSkillsAsync(null);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetSkillsAsync_FiltersByCategory()
    {
        await SeedSkillAsync("C#", "Programming");
        await SeedSkillAsync("Java", "Programming");
        await SeedSkillAsync("SQL", "Database");

        var result = await _sut.GetSkillsAsync("Programming");

        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.SkillCategory == "Programming");
    }

    [Fact]
    public async Task GetSkillsAsync_ReturnsOrderedByName()
    {
        await SeedSkillAsync("Zebra", "Misc");
        await SeedSkillAsync("Alpha", "Misc");
        await SeedSkillAsync("Middle", "Misc");

        var result = await _sut.GetSkillsAsync(null);

        result.Select(s => s.SkillName).Should().BeInAscendingOrder();
    }
}
