using FluentAssertions;
using FluentValidation.TestHelper;
using JobService.Application.DTOs;
using JobService.Application.Validators;
using JobService.Domain.Enums;

namespace JobService.UnitTests;

public class CreateJobValidatorTests
{
    private readonly CreateJobValidator _validator = new();

    private static CreateJobRequest ValidRequest() => new()
    {
        Title = "Senior .NET Developer",
        Description = "We are looking for an experienced developer.",
        Location = "Remote",
        EmploymentType = EmploymentType.FullTime,
        ExperienceRequired = 5,
        SalaryMin = 80_000,
        SalaryMax = 120_000,
        ApplicationDeadline = DateTime.UtcNow.AddDays(30),
        Skills = new List<JobSkillInput>()
    };

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Title_WhenEmptyOrNull_ShouldFail(string? title)
    {
        var request = ValidRequest();
        request.Title = title!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public async Task Title_WhenExceeds200Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.Title = new string('A', 201);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Description_WhenEmptyOrNull_ShouldFail(string? description)
    {
        var request = ValidRequest();
        request.Description = description!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required");
    }

    [Fact]
    public async Task Description_WhenExceeds5000Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.Description = new string('A', 5001);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 5000 characters");
    }

    [Fact]
    public async Task Location_WhenExceeds200Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.Location = new string('A', 201);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location cannot exceed 200 characters");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(31)]
    public async Task ExperienceRequired_WhenOutOfRange_ShouldFail(int experience)
    {
        var request = ValidRequest();
        request.ExperienceRequired = experience;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ExperienceRequired)
            .WithErrorMessage("Experience must be between 0 and 30 years");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    public async Task ExperienceRequired_WhenWithinRange_ShouldPass(int experience)
    {
        var request = ValidRequest();
        request.ExperienceRequired = experience;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ExperienceRequired);
    }

    [Fact]
    public async Task SalaryMin_WhenNegative_ShouldFail()
    {
        var request = ValidRequest();
        request.SalaryMin = -1;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.SalaryMin)
            .WithErrorMessage("Minimum salary cannot be negative");
    }

    [Fact]
    public async Task SalaryMax_WhenLessThanSalaryMin_ShouldFail()
    {
        var request = ValidRequest();
        request.SalaryMin = 100_000;
        request.SalaryMax = 50_000;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.SalaryMax)
            .WithErrorMessage("Maximum salary must be greater than minimum");
    }

    [Fact]
    public async Task SalaryMax_WhenNull_ShouldPass()
    {
        var request = ValidRequest();
        request.SalaryMax = null;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SalaryMax);
    }

    [Fact]
    public async Task EmploymentType_WhenInvalid_ShouldFail()
    {
        var request = ValidRequest();
        request.EmploymentType = (EmploymentType)999;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType)
            .WithErrorMessage("Invalid employment type");
    }

    [Fact]
    public async Task ApplicationDeadline_WhenInPast_ShouldFail()
    {
        var request = ValidRequest();
        request.ApplicationDeadline = DateTime.UtcNow.AddDays(-1);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ApplicationDeadline)
            .WithErrorMessage("Deadline must be in the future");
    }

    [Fact]
    public async Task ApplicationDeadline_WhenNull_ShouldPass()
    {
        var request = ValidRequest();
        request.ApplicationDeadline = null;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ApplicationDeadline);
    }
}

public class UpdateJobValidatorTests
{
    private readonly UpdateJobValidator _validator = new();

    private static UpdateJobRequest ValidRequest() => new()
    {
        Title = "Senior .NET Developer",
        Description = "Updated job description with full details.",
        Location = "New York",
        EmploymentType = EmploymentType.Contract,
        ExperienceRequired = 3,
        SalaryMin = 90_000,
        SalaryMax = 130_000,
        Skills = new List<JobSkillInput>()
    };

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Title_WhenEmptyOrNull_ShouldFail(string? title)
    {
        var request = ValidRequest();
        request.Title = title!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public async Task Title_WhenExceeds200Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.Title = new string('A', 201);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Description_WhenEmptyOrNull_ShouldFail(string? description)
    {
        var request = ValidRequest();
        request.Description = description!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required");
    }

    [Fact]
    public async Task Description_WhenExceeds5000Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.Description = new string('A', 5001);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 5000 characters");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(31)]
    public async Task ExperienceRequired_WhenOutOfRange_ShouldFail(int experience)
    {
        var request = ValidRequest();
        request.ExperienceRequired = experience;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ExperienceRequired);
    }

    [Fact]
    public async Task SalaryMax_WhenLessThanMin_ShouldFail()
    {
        var request = ValidRequest();
        request.SalaryMin = 100_000;
        request.SalaryMax = 50_000;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.SalaryMax);
    }

    [Fact]
    public async Task EmploymentType_WhenInvalid_ShouldFail()
    {
        var request = ValidRequest();
        request.EmploymentType = (EmploymentType)999;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType);
    }
}

public class CreateSkillValidatorTests
{
    private readonly CreateSkillValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var request = new CreateSkillRequest { SkillName = "C#", SkillCategory = "Programming" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task SkillName_WhenEmptyOrNull_ShouldFail(string? name)
    {
        var request = new CreateSkillRequest { SkillName = name! };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.SkillName)
            .WithErrorMessage("Skill name is required");
    }

    [Fact]
    public async Task SkillName_WhenExceeds100Characters_ShouldFail()
    {
        var request = new CreateSkillRequest { SkillName = new string('A', 101) };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.SkillName)
            .WithErrorMessage("Skill name cannot exceed 100 characters");
    }

    [Fact]
    public async Task SkillCategory_WhenExceeds50Characters_ShouldFail()
    {
        var request = new CreateSkillRequest
        {
            SkillName = "C#",
            SkillCategory = new string('A', 51)
        };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.SkillCategory)
            .WithErrorMessage("Category cannot exceed 50 characters");
    }

    [Fact]
    public async Task SkillCategory_WhenNull_ShouldPass()
    {
        var request = new CreateSkillRequest { SkillName = "C#", SkillCategory = null };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SkillCategory);
    }
}
