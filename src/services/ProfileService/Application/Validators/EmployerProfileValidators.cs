using FluentValidation;
using ProfileService.Application.DTOs;

namespace ProfileService.Application.Validators;

public class CreateEmployerProfileValidator : AbstractValidator<CreateEmployerProfileRequest>
{
    public CreateEmployerProfileValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters");

        RuleFor(x => x.CompanyDescription)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Industry)
            .MaximumLength(100).WithMessage("Industry cannot exceed 100 characters");

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500).WithMessage("Website URL is too long")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid website URL format");

        RuleFor(x => x.CompanyLocation)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");
    }
}

public class UpdateEmployerProfileValidator : AbstractValidator<UpdateEmployerProfileRequest>
{
    public UpdateEmployerProfileValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters");

        RuleFor(x => x.CompanyDescription)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Industry)
            .MaximumLength(100).WithMessage("Industry cannot exceed 100 characters");

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(500).WithMessage("Website URL is too long")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid website URL format");

        RuleFor(x => x.CompanyLocation)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");
    }
}
