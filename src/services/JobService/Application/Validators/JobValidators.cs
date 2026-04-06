using FluentValidation;
using JobService.Application.DTOs;

namespace JobService.Application.Validators;

public class CreateJobValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");

        RuleFor(x => x.ExperienceRequired)
            .InclusiveBetween(0, 30).WithMessage("Experience must be between 0 and 30 years");

        RuleFor(x => x.SalaryMin)
            .GreaterThanOrEqualTo(0).When(x => x.SalaryMin.HasValue)
            .WithMessage("Minimum salary cannot be negative");

        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(x => x.SalaryMin ?? 0).When(x => x.SalaryMax.HasValue && x.SalaryMin.HasValue)
            .WithMessage("Maximum salary must be greater than minimum");

        RuleFor(x => x.EmploymentType)
            .IsInEnum().WithMessage("Invalid employment type");

        RuleFor(x => x.ApplicationDeadline)
            .GreaterThan(DateTime.UtcNow).When(x => x.ApplicationDeadline.HasValue)
            .WithMessage("Deadline must be in the future");
    }
}

public class UpdateJobValidator : AbstractValidator<UpdateJobRequest>
{
    public UpdateJobValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");

        RuleFor(x => x.ExperienceRequired)
            .InclusiveBetween(0, 30).WithMessage("Experience must be between 0 and 30 years");

        RuleFor(x => x.SalaryMin)
            .GreaterThanOrEqualTo(0).When(x => x.SalaryMin.HasValue)
            .WithMessage("Minimum salary cannot be negative");

        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(x => x.SalaryMin ?? 0).When(x => x.SalaryMax.HasValue && x.SalaryMin.HasValue)
            .WithMessage("Maximum salary must be greater than minimum");

        RuleFor(x => x.EmploymentType)
            .IsInEnum().WithMessage("Invalid employment type");
    }
}

public class CreateSkillValidator : AbstractValidator<CreateSkillRequest>
{
    public CreateSkillValidator()
    {
        RuleFor(x => x.SkillName)
            .NotEmpty().WithMessage("Skill name is required")
            .MaximumLength(100).WithMessage("Skill name cannot exceed 100 characters");

        RuleFor(x => x.SkillCategory)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters");
    }
}
