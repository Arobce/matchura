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
            .MaximumLength(500).WithMessage("Resume URL is too long");

        RuleFor(x => x.CoverLetterUrl)
            .MaximumLength(500).WithMessage("Cover letter URL is too long");
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
