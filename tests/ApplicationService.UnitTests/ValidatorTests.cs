using FluentAssertions;
using FluentValidation.TestHelper;
using ApplicationService.Application.DTOs;
using ApplicationService.Application.Validators;

namespace ApplicationService.UnitTests;

public class CreateApplicationValidatorTests
{
    private readonly CreateApplicationValidator _validator = new();

    private static CreateApplicationRequest ValidRequest() => new()
    {
        JobId = Guid.NewGuid(),
        CoverLetter = "I am very interested in this position.",
        ResumeUrl = "https://example.com/resume.pdf",
        CoverLetterUrl = "https://example.com/cover.pdf"
    };

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidRequest_WithOptionalFieldsNull_ShouldPass()
    {
        var request = new CreateApplicationRequest
        {
            JobId = Guid.NewGuid(),
            CoverLetter = null,
            ResumeUrl = null,
            CoverLetterUrl = null
        };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task JobId_WhenEmpty_ShouldFail()
    {
        var request = ValidRequest();
        request.JobId = Guid.Empty;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.JobId)
            .WithErrorMessage("Job ID is required");
    }

    [Fact]
    public async Task CoverLetter_WhenExceeds3000Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.CoverLetter = new string('A', 3001);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter)
            .WithErrorMessage("Cover letter cannot exceed 3000 characters");
    }

    [Fact]
    public async Task CoverLetter_WhenExactly3000Characters_ShouldPass()
    {
        var request = ValidRequest();
        request.CoverLetter = new string('A', 3000);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.CoverLetter);
    }

    [Fact]
    public async Task ResumeUrl_WhenExceeds500Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.ResumeUrl = new string('A', 501);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ResumeUrl)
            .WithErrorMessage("Resume URL is too long");
    }

    [Fact]
    public async Task CoverLetterUrl_WhenExceeds500Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.CoverLetterUrl = new string('A', 501);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.CoverLetterUrl)
            .WithErrorMessage("Cover letter URL is too long");
    }
}

public class UpdateEmployerNotesValidatorTests
{
    private readonly UpdateEmployerNotesValidator _validator = new();

    [Fact]
    public async Task ValidNotes_ShouldPassValidation()
    {
        var request = new UpdateEmployerNotesRequest { Notes = "Good candidate, schedule interview." };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Notes_WhenEmpty_ShouldPass()
    {
        var request = new UpdateEmployerNotesRequest { Notes = "" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Notes_WhenExceeds2000Characters_ShouldFail()
    {
        var request = new UpdateEmployerNotesRequest { Notes = new string('A', 2001) };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 2000 characters");
    }

    [Fact]
    public async Task Notes_WhenExactly2000Characters_ShouldPass()
    {
        var request = new UpdateEmployerNotesRequest { Notes = new string('A', 2000) };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }
}
