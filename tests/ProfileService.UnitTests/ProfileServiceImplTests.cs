using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProfileService.Application.DTOs;
using ProfileService.Domain.Entities;
using ProfileService.Infrastructure.Data;
using ProfileService.Infrastructure.Services;
using Shared.TestUtilities;

namespace ProfileService.UnitTests;

public class ProfileServiceImplTests : IDisposable
{
    private readonly ProfileDbContext _db;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly ProfileServiceImpl _sut;

    public ProfileServiceImplTests()
    {
        var (context, connection) = DbContextFactory.Create<ProfileDbContext>();
        _db = context;
        _connection = connection;

        // SQLite does not support gen_random_uuid(), so assign GUIDs client-side before saving.
        _db.SavingChanges += (sender, _) =>
        {
            if (sender is not ProfileDbContext ctx) return;
            foreach (var entry in ctx.ChangeTracker.Entries<CandidateProfile>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.CandidateId == Guid.Empty)
                    entry.Entity.CandidateId = Guid.NewGuid();
            }
            foreach (var entry in ctx.ChangeTracker.Entries<EmployerProfile>())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    && entry.Entity.EmployerId == Guid.Empty)
                    entry.Entity.EmployerId = Guid.NewGuid();
            }
        };

        var logger = new Mock<ILogger<ProfileServiceImpl>>();
        _sut = new ProfileServiceImpl(_db, logger.Object);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ───────────────────────────────────────────────
    //  Candidate Profile — Create
    // ───────────────────────────────────────────────

    [Fact]
    public async Task CreateCandidateProfileAsync_ShouldCreateAndReturnProfile()
    {
        var userId = "user-1";
        var request = MakeCandidateCreateRequest();

        var result = await _sut.CreateCandidateProfileAsync(userId, request);

        result.UserId.Should().Be(userId);
        result.Phone.Should().Be(request.Phone);
        result.Location.Should().Be(request.Location);
        result.ProfessionalSummary.Should().Be(request.ProfessionalSummary);
        result.YearsOfExperience.Should().Be(request.YearsOfExperience);
        result.HighestEducation.Should().Be(request.HighestEducation);
        result.LinkedinUrl.Should().Be(request.LinkedinUrl);
        result.CandidateId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCandidateProfileAsync_DuplicateUserId_ShouldThrow()
    {
        var userId = "user-dup";
        await _sut.CreateCandidateProfileAsync(userId, MakeCandidateCreateRequest());

        var act = () => _sut.CreateCandidateProfileAsync(userId, MakeCandidateCreateRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    // ───────────────────────────────────────────────
    //  Candidate Profile — Get
    // ───────────────────────────────────────────────

    [Fact]
    public async Task GetCandidateProfileAsync_ExistingUser_ShouldReturnProfile()
    {
        var userId = "user-get";
        var created = await _sut.CreateCandidateProfileAsync(userId, MakeCandidateCreateRequest());

        var result = await _sut.GetCandidateProfileAsync(userId);

        result.CandidateId.Should().Be(created.CandidateId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetCandidateProfileAsync_NotFound_ShouldThrow()
    {
        var act = () => _sut.GetCandidateProfileAsync("nonexistent-user");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ───────────────────────────────────────────────
    //  Candidate Profile — Update
    // ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateCandidateProfileAsync_ShouldUpdateAllFields()
    {
        var userId = "user-update";
        await _sut.CreateCandidateProfileAsync(userId, MakeCandidateCreateRequest());

        var updateRequest = new UpdateCandidateProfileRequest
        {
            Phone = "999-999-9999",
            Location = "Updated City",
            ProfessionalSummary = "Updated summary",
            YearsOfExperience = 10,
            HighestEducation = "PhD",
            LinkedinUrl = "https://linkedin.com/in/updated"
        };

        var result = await _sut.UpdateCandidateProfileAsync(userId, updateRequest);

        result.Phone.Should().Be(updateRequest.Phone);
        result.Location.Should().Be(updateRequest.Location);
        result.ProfessionalSummary.Should().Be(updateRequest.ProfessionalSummary);
        result.YearsOfExperience.Should().Be(updateRequest.YearsOfExperience);
        result.HighestEducation.Should().Be(updateRequest.HighestEducation);
        result.LinkedinUrl.Should().Be(updateRequest.LinkedinUrl);
    }

    [Fact]
    public async Task UpdateCandidateProfileAsync_NotFound_ShouldThrow()
    {
        var act = () => _sut.UpdateCandidateProfileAsync("no-such-user", new UpdateCandidateProfileRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ───────────────────────────────────────────────
    //  Candidate Profile — Public View
    // ───────────────────────────────────────────────

    [Fact]
    public async Task GetCandidateProfilePublicAsync_ShouldReturnLimitedFields()
    {
        var userId = "user-public";
        var created = await _sut.CreateCandidateProfileAsync(userId, MakeCandidateCreateRequest());

        var result = await _sut.GetCandidateProfilePublicAsync(created.CandidateId);

        result.CandidateId.Should().Be(created.CandidateId);
        result.Location.Should().Be(created.Location);
        result.ProfessionalSummary.Should().Be(created.ProfessionalSummary);
        result.YearsOfExperience.Should().Be(created.YearsOfExperience);
        result.HighestEducation.Should().Be(created.HighestEducation);
    }

    [Fact]
    public async Task GetCandidateProfilePublicAsync_ShouldNotExposePrivateFields()
    {
        var userId = "user-public-priv";
        var request = MakeCandidateCreateRequest();
        request.Phone = "555-123-4567";
        request.LinkedinUrl = "https://linkedin.com/in/secret";
        var created = await _sut.CreateCandidateProfileAsync(userId, request);

        var result = await _sut.GetCandidateProfilePublicAsync(created.CandidateId);

        // CandidateProfilePublicResponse does not have Phone, LinkedinUrl, UserId, CreatedAt, UpdatedAt
        var publicType = typeof(CandidateProfilePublicResponse);
        publicType.GetProperty("Phone").Should().BeNull();
        publicType.GetProperty("LinkedinUrl").Should().BeNull();
        publicType.GetProperty("UserId").Should().BeNull();
        publicType.GetProperty("CreatedAt").Should().BeNull();
        publicType.GetProperty("UpdatedAt").Should().BeNull();
    }

    [Fact]
    public async Task GetCandidateProfilePublicAsync_NotFound_ShouldThrow()
    {
        var act = () => _sut.GetCandidateProfilePublicAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ───────────────────────────────────────────────
    //  Employer Profile — Create
    // ───────────────────────────────────────────────

    [Fact]
    public async Task CreateEmployerProfileAsync_ShouldCreateAndReturnProfile()
    {
        var userId = "employer-1";
        var request = MakeEmployerCreateRequest();

        var result = await _sut.CreateEmployerProfileAsync(userId, request);

        result.UserId.Should().Be(userId);
        result.CompanyName.Should().Be(request.CompanyName);
        result.CompanyDescription.Should().Be(request.CompanyDescription);
        result.Industry.Should().Be(request.Industry);
        result.WebsiteUrl.Should().Be(request.WebsiteUrl);
        result.CompanyLocation.Should().Be(request.CompanyLocation);
        result.LogoUrl.Should().Be(request.LogoUrl);
        result.EmployerId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateEmployerProfileAsync_DuplicateUserId_ShouldThrow()
    {
        var userId = "employer-dup";
        await _sut.CreateEmployerProfileAsync(userId, MakeEmployerCreateRequest());

        var act = () => _sut.CreateEmployerProfileAsync(userId, MakeEmployerCreateRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    // ───────────────────────────────────────────────
    //  Employer Profile — Get
    // ───────────────────────────────────────────────

    [Fact]
    public async Task GetEmployerProfileAsync_ExistingUser_ShouldReturnProfile()
    {
        var userId = "employer-get";
        var created = await _sut.CreateEmployerProfileAsync(userId, MakeEmployerCreateRequest());

        var result = await _sut.GetEmployerProfileAsync(userId);

        result.EmployerId.Should().Be(created.EmployerId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetEmployerProfileAsync_NotFound_ShouldThrow()
    {
        var act = () => _sut.GetEmployerProfileAsync("nonexistent-employer");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ───────────────────────────────────────────────
    //  Employer Profile — Update
    // ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateEmployerProfileAsync_ShouldUpdateAllFields()
    {
        var userId = "employer-update";
        await _sut.CreateEmployerProfileAsync(userId, MakeEmployerCreateRequest());

        var updateRequest = new UpdateEmployerProfileRequest
        {
            CompanyName = "Updated Corp",
            CompanyDescription = "Updated description",
            Industry = "Finance",
            WebsiteUrl = "https://updated.com",
            CompanyLocation = "Updated Location",
            LogoUrl = "https://updated.com/logo.png"
        };

        var result = await _sut.UpdateEmployerProfileAsync(userId, updateRequest);

        result.CompanyName.Should().Be(updateRequest.CompanyName);
        result.CompanyDescription.Should().Be(updateRequest.CompanyDescription);
        result.Industry.Should().Be(updateRequest.Industry);
        result.WebsiteUrl.Should().Be(updateRequest.WebsiteUrl);
        result.CompanyLocation.Should().Be(updateRequest.CompanyLocation);
        result.LogoUrl.Should().Be(updateRequest.LogoUrl);
    }

    [Fact]
    public async Task UpdateEmployerProfileAsync_NotFound_ShouldThrow()
    {
        var act = () => _sut.UpdateEmployerProfileAsync("no-such-employer", new UpdateEmployerProfileRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ───────────────────────────────────────────────
    //  Employer Profile — Public View
    // ───────────────────────────────────────────────

    [Fact]
    public async Task GetEmployerProfilePublicAsync_ShouldReturnLimitedFields()
    {
        var userId = "employer-public";
        var created = await _sut.CreateEmployerProfileAsync(userId, MakeEmployerCreateRequest());

        var result = await _sut.GetEmployerProfilePublicAsync(created.EmployerId);

        result.EmployerId.Should().Be(created.EmployerId);
        result.CompanyName.Should().Be(created.CompanyName);
        result.CompanyDescription.Should().Be(created.CompanyDescription);
        result.Industry.Should().Be(created.Industry);
        result.WebsiteUrl.Should().Be(created.WebsiteUrl);
        result.CompanyLocation.Should().Be(created.CompanyLocation);
        result.LogoUrl.Should().Be(created.LogoUrl);
    }

    [Fact]
    public async Task GetEmployerProfilePublicAsync_ShouldNotExposePrivateFields()
    {
        var userId = "employer-public-priv";
        var created = await _sut.CreateEmployerProfileAsync(userId, MakeEmployerCreateRequest());

        var result = await _sut.GetEmployerProfilePublicAsync(created.EmployerId);

        // EmployerProfilePublicResponse does not have UserId, CreatedAt, UpdatedAt
        var publicType = typeof(EmployerProfilePublicResponse);
        publicType.GetProperty("UserId").Should().BeNull();
        publicType.GetProperty("CreatedAt").Should().BeNull();
        publicType.GetProperty("UpdatedAt").Should().BeNull();
    }

    [Fact]
    public async Task GetEmployerProfilePublicAsync_NotFound_ShouldThrow()
    {
        var act = () => _sut.GetEmployerProfilePublicAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ───────────────────────────────────────────────
    //  Helpers
    // ───────────────────────────────────────────────

    private static CreateCandidateProfileRequest MakeCandidateCreateRequest() => new()
    {
        Phone = "123-456-7890",
        Location = "New York, NY",
        ProfessionalSummary = "Experienced software engineer",
        YearsOfExperience = 5,
        HighestEducation = "Bachelor's in CS",
        LinkedinUrl = "https://linkedin.com/in/testuser"
    };

    private static CreateEmployerProfileRequest MakeEmployerCreateRequest() => new()
    {
        CompanyName = "Test Corp",
        CompanyDescription = "A test company",
        Industry = "Technology",
        WebsiteUrl = "https://testcorp.com",
        CompanyLocation = "San Francisco, CA",
        LogoUrl = "https://testcorp.com/logo.png"
    };
}
