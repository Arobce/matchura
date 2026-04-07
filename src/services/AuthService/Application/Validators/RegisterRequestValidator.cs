using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private static readonly string[] AllowedRoles = ["Candidate", "Employer"];

    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => AllowedRoles.Contains(role))
            .WithMessage("Role must be either 'Candidate' or 'Employer'");
    }
}
