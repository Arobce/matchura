using ApplicationService.Application.DTOs;
using FluentValidation;

namespace ApplicationService.Application.Validators;

public class CreateApplicationValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty().WithMessage("Job ID is required");

        RuleFor(x => x.CoverLetter)
            .MaximumLength(3000).WithMessage("Cover letter cannot exceed 3000 characters");

        RuleFor(x => x.ResumeUrl)
            .MaximumLength(500).WithMessage("Resume URL is too long")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid resume URL format");
    }
}

public class UpdateEmployerNotesValidator : AbstractValidator<UpdateEmployerNotesRequest>
{
    public UpdateEmployerNotesValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }
}
