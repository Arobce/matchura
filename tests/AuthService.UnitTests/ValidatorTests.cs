using FluentAssertions;
using FluentValidation.TestHelper;
using AuthService.Application.DTOs;
using AuthService.Application.Validators;

namespace AuthService.UnitTests;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest ValidRequest() => new()
    {
        Email = "user@example.com",
        Password = "SecurePass1",
        FirstName = "John",
        LastName = "Doe",
        Role = "Candidate"
    };

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Candidate")]
    [InlineData("Employer")]
    public async Task ValidRoles_ShouldPass(string role)
    {
        var request = ValidRequest();
        request.Role = role;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    // --- Email validation ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Email_WhenEmptyOrNull_ShouldFail(string? email)
    {
        var request = ValidRequest();
        request.Email = email!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@missing.com")]
    public async Task Email_WhenInvalidFormat_ShouldFail(string email)
    {
        var request = ValidRequest();
        request.Email = email;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    // --- Password validation ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Password_WhenEmptyOrNull_ShouldFail(string? password)
    {
        var request = ValidRequest();
        request.Password = password!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Password_WhenTooShort_ShouldFail()
    {
        var request = ValidRequest();
        request.Password = "Short1";
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters");
    }

    [Fact]
    public async Task Password_WhenNoUppercase_ShouldFail()
    {
        var request = ValidRequest();
        request.Password = "lowercase1";
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Fact]
    public async Task Password_WhenNoNumber_ShouldFail()
    {
        var request = ValidRequest();
        request.Password = "NoNumberHere";
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one number");
    }

    // --- FirstName validation ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task FirstName_WhenEmptyOrNull_ShouldFail(string? firstName)
    {
        var request = ValidRequest();
        request.FirstName = firstName!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public async Task FirstName_WhenExceeds50Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.FirstName = new string('A', 51);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name cannot exceed 50 characters");
    }

    // --- LastName validation ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task LastName_WhenEmptyOrNull_ShouldFail(string? lastName)
    {
        var request = ValidRequest();
        request.LastName = lastName!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required");
    }

    [Fact]
    public async Task LastName_WhenExceeds50Characters_ShouldFail()
    {
        var request = ValidRequest();
        request.LastName = new string('A', 51);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name cannot exceed 50 characters");
    }

    // --- Role validation ---

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Role_WhenEmptyOrNull_ShouldFail(string? role)
    {
        var request = ValidRequest();
        request.Role = role!;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    [InlineData("candidate")]
    [InlineData("employer")]
    public async Task Role_WhenInvalid_ShouldFail(string role)
    {
        var request = ValidRequest();
        request.Role = role;
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("Role must be either 'Candidate' or 'Employer'");
    }
}

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var request = new LoginRequest { Email = "user@example.com", Password = "password123" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Email_WhenEmptyOrNull_ShouldFail(string? email)
    {
        var request = new LoginRequest { Email = email!, Password = "password123" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Email_WhenInvalidFormat_ShouldFail()
    {
        var request = new LoginRequest { Email = "not-an-email", Password = "password123" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Password_WhenEmptyOrNull_ShouldFail(string? password)
    {
        var request = new LoginRequest { Email = "user@example.com", Password = password! };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }
}

public class TwoFactorRequestValidatorTests
{
    private readonly TwoFactorRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var request = new TwoFactorRequest { Email = "user@example.com", Code = "123456" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Email_WhenEmptyOrNull_ShouldFail(string? email)
    {
        var request = new TwoFactorRequest { Email = email!, Code = "123456" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Email_WhenInvalidFormat_ShouldFail()
    {
        var request = new TwoFactorRequest { Email = "bad-email", Code = "123456" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Code_WhenEmptyOrNull_ShouldFail(string? code)
    {
        var request = new TwoFactorRequest { Email = "user@example.com", Code = code! };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    public async Task Code_WhenNotSixCharacters_ShouldFail(string code)
    {
        var request = new TwoFactorRequest { Email = "user@example.com", Code = code };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code must be 6 digits");
    }

    [Theory]
    [InlineData("abcdef")]
    [InlineData("12ab56")]
    [InlineData("12 456")]
    public async Task Code_WhenContainsNonDigits_ShouldFail(string code)
    {
        var request = new TwoFactorRequest { Email = "user@example.com", Code = code };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
}

public class VerifyEmailRequestValidatorTests
{
    private readonly VerifyEmailRequestValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldPassValidation()
    {
        var request = new VerifyEmailRequest { Email = "user@example.com", Code = "654321" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Email_WhenEmptyOrNull_ShouldFail(string? email)
    {
        var request = new VerifyEmailRequest { Email = email!, Code = "654321" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Email_WhenInvalidFormat_ShouldFail()
    {
        var request = new VerifyEmailRequest { Email = "bad-email", Code = "654321" };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Code_WhenEmptyOrNull_ShouldFail(string? code)
    {
        var request = new VerifyEmailRequest { Email = "user@example.com", Code = code! };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    public async Task Code_WhenNotSixCharacters_ShouldFail(string code)
    {
        var request = new VerifyEmailRequest { Email = "user@example.com", Code = code };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code must be 6 digits");
    }

    [Theory]
    [InlineData("abcdef")]
    [InlineData("12ab56")]
    public async Task Code_WhenContainsNonDigits_ShouldFail(string code)
    {
        var request = new VerifyEmailRequest { Email = "user@example.com", Code = code };
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
}
