using FluentValidation;
using ProfileService.Application.DTOs;

namespace ProfileService.Application.Validators;

public class CreateCandidateProfileValidator : AbstractValidator<CreateCandidateProfileRequest>
{
    public CreateCandidateProfileValidator()
    {
        RuleFor(x => x.ProfessionalSummary)
            .MaximumLength(2000).WithMessage("Professional summary cannot exceed 2000 characters");

        RuleFor(x => x.YearsOfExperience)
            .InclusiveBetween(0, 60).WithMessage("Years of experience must be between 0 and 60");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number is too long");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");

        RuleFor(x => x.LinkedinUrl)
            .MaximumLength(500).WithMessage("LinkedIn URL is too long")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid LinkedIn URL format");
    }
}

public class UpdateCandidateProfileValidator : AbstractValidator<UpdateCandidateProfileRequest>
{
    public UpdateCandidateProfileValidator()
    {
        RuleFor(x => x.ProfessionalSummary)
            .MaximumLength(2000).WithMessage("Professional summary cannot exceed 2000 characters");

        RuleFor(x => x.YearsOfExperience)
            .InclusiveBetween(0, 60).WithMessage("Years of experience must be between 0 and 60");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number is too long");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");

        RuleFor(x => x.LinkedinUrl)
            .MaximumLength(500).WithMessage("LinkedIn URL is too long")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid LinkedIn URL format");
    }
}
