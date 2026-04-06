using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Code must be 6 digits")
            .Matches("^[0-9]+$").WithMessage("Code must contain only digits");
    }
}
