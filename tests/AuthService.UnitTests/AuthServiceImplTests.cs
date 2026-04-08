using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.TestUtilities.Fakes;

namespace AuthService.UnitTests;

public class AuthServiceImplTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator;
    private readonly FakeEmailService _emailService;
    private readonly Mock<ILogger<AuthServiceImpl>> _logger;
    private readonly AuthServiceImpl _sut;

    public AuthServiceImplTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _emailService = new FakeEmailService();
        _logger = new Mock<ILogger<AuthServiceImpl>>();

        _sut = new AuthServiceImpl(
            _userManager.Object,
            _jwtTokenGenerator.Object,
            _emailService,
            _logger.Object);
    }

    private static RegisterRequest CreateRegisterRequest(
        string email = "test@example.com",
        string password = "Password123!",
        string firstName = "John",
        string lastName = "Doe",
        string role = "Candidate") => new()
    {
        Email = email,
        Password = password,
        FirstName = firstName,
        LastName = lastName,
        Role = role
    };

    private static ApplicationUser CreateUser(
        string id = "user-1",
        string email = "test@example.com",
        string fullName = "John Doe",
        bool emailConfirmed = true,
        AccountStatus status = AccountStatus.Active,
        bool twoFactorEnabled = false,
        string? verificationCode = null,
        DateTime? verificationExpiry = null,
        string? twoFactorCode = null,
        DateTime? twoFactorExpiry = null) => new()
    {
        Id = id,
        Email = email,
        UserName = email,
        FullName = fullName,
        EmailConfirmed = emailConfirmed,
        AccountStatus = status,
        TwoFactorEmailEnabled = twoFactorEnabled,
        EmailVerificationCode = verificationCode,
        EmailVerificationCodeExpiry = verificationExpiry,
        TwoFactorEmailCode = twoFactorCode,
        TwoFactorEmailCodeExpiry = twoFactorExpiry
    };

    // ---------------------------------------------------------------
    // RegisterAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUserAssignsRoleAndSendsEmail()
    {
        var request = CreateRegisterRequest();

        _userManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);
        _userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), request.Role))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterAsync(request);

        result.Email.Should().Be(request.Email);
        result.FullName.Should().Be("John Doe");
        result.Role.Should().Be("Candidate");
        result.RequiresEmailVerification.Should().BeTrue();
        result.Token.Should().BeNull();

        _userManager.Verify(x => x.CreateAsync(
            It.Is<ApplicationUser>(u =>
                u.Email == request.Email &&
                u.FullName == "John Doe" &&
                u.AccountStatus == AccountStatus.Active &&
                !u.EmailConfirmed &&
                u.EmailVerificationCode != null),
            request.Password), Times.Once);

        _userManager.Verify(x => x.AddToRoleAsync(
            It.IsAny<ApplicationUser>(), "Candidate"), Times.Once);

        _emailService.SentVerificationCodes.Should().ContainSingle()
            .Which.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var request = CreateRegisterRequest();
        _userManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(CreateUser());

        var act = () => _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_CreateAsyncFails_ThrowsWithErrorDetails()
    {
        var request = CreateRegisterRequest();
        _userManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var errors = new[]
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Password too short" },
            new IdentityError { Code = "PasswordRequiresDigit", Description = "Needs a digit" }
        };
        _userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var act = () => _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Password too short*")
            .WithMessage("*Needs a digit*");
    }

    // ---------------------------------------------------------------
    // VerifyEmailAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task VerifyEmailAsync_ValidCode_ConfirmsEmailAndReturnsToken()
    {
        var code = "123456";
        var user = CreateUser(
            emailConfirmed: false,
            verificationCode: code,
            verificationExpiry: DateTime.UtcNow.AddMinutes(10));

        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Candidate" });
        _jwtTokenGenerator.Setup(x => x.GenerateToken(user, "Candidate"))
            .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

        var request = new VerifyEmailRequest { Email = user.Email!, Code = code };
        var result = await _sut.VerifyEmailAsync(request);

        result.Token.Should().Be("jwt-token");
        result.ExpiresAt.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be("Candidate");

        _userManager.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u =>
            u.EmailConfirmed &&
            u.EmailVerificationCode == null &&
            u.EmailVerificationCodeExpiry == null)), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidCode_ThrowsUnauthorizedAccessException()
    {
        var user = CreateUser(
            emailConfirmed: false,
            verificationCode: "123456",
            verificationExpiry: DateTime.UtcNow.AddMinutes(10));

        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var request = new VerifyEmailRequest { Email = user.Email!, Code = "999999" };
        var act = () => _sut.VerifyEmailAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid verification code*");
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredCode_ThrowsUnauthorizedAccessException()
    {
        var code = "123456";
        var user = CreateUser(
            emailConfirmed: false,
            verificationCode: code,
            verificationExpiry: DateTime.UtcNow.AddMinutes(-5));

        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var request = new VerifyEmailRequest { Email = user.Email!, Code = code };
        var act = () => _sut.VerifyEmailAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task VerifyEmailAsync_AlreadyVerified_ThrowsInvalidOperationException()
    {
        var user = CreateUser(emailConfirmed: true);
        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var request = new VerifyEmailRequest { Email = user.Email!, Code = "123456" };
        var act = () => _sut.VerifyEmailAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already verified*");
    }

    // ---------------------------------------------------------------
    // ResendVerificationAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task ResendVerificationAsync_UnverifiedUser_GeneratesNewCodeAndSendsEmail()
    {
        var user = CreateUser(emailConfirmed: false);
        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.ResendVerificationAsync(user.Email!);

        _userManager.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u =>
            u.EmailVerificationCode != null &&
            u.EmailVerificationCodeExpiry > DateTime.UtcNow)), Times.Once);

        _emailService.SentVerificationCodes.Should().ContainSingle()
            .Which.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task ResendVerificationAsync_AlreadyVerified_ThrowsInvalidOperationException()
    {
        var user = CreateUser(emailConfirmed: true);
        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var act = () => _sut.ResendVerificationAsync(user.Email!);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already verified*");
    }

    [Fact]
    public async Task ResendVerificationAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        _userManager.Setup(x => x.FindByEmailAsync("unknown@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.ResendVerificationAsync("unknown@example.com");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ---------------------------------------------------------------
    // LoginAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        var user = CreateUser();
        var request = new LoginRequest { Email = user.Email!, Password = "Password123!" };

        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManager.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _userManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Candidate" });
        _jwtTokenGenerator.Setup(x => x.GenerateToken(user, "Candidate"))
            .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.LoginAsync(request);

        result.Token.Should().Be("jwt-token");
        result.ExpiresAt.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
        result.RequiresTwoFactor.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_UnverifiedEmail_ThrowsUnauthorizedAccessException()
    {
        var user = CreateUser(emailConfirmed: false);
        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var request = new LoginRequest { Email = user.Email!, Password = "Password123!" };
        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*verify your email*");
    }

    [Fact]
    public async Task LoginAsync_SuspendedAccount_ThrowsUnauthorizedAccessException()
    {
        var user = CreateUser(status: AccountStatus.Suspended);
        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var request = new LoginRequest { Email = user.Email!, Password = "Password123!" };
        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*suspended*");
    }

    [Fact]
    public async Task LoginAsync_DeactivatedAccount_ThrowsUnauthorizedAccessException()
    {
        var user = CreateUser(status: AccountStatus.Deactivated);
        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var request = new LoginRequest { Email = user.Email!, Password = "Password123!" };
        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*deactivated*");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = CreateUser();
        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManager.Setup(x => x.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

        var request = new LoginRequest { Email = user.Email!, Password = "wrong" };
        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_NonexistentEmail_ThrowsUnauthorizedAccessException()
    {
        _userManager.Setup(x => x.FindByEmailAsync("nobody@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var request = new LoginRequest { Email = "nobody@example.com", Password = "Password123!" };
        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_TwoFactorEnabled_SendsCodeAndReturnsRequiresTwoFactor()
    {
        var user = CreateUser(twoFactorEnabled: true);
        var request = new LoginRequest { Email = user.Email!, Password = "Password123!" };

        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManager.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _userManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Candidate" });
        _userManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.LoginAsync(request);

        result.RequiresTwoFactor.Should().BeTrue();
        result.Token.Should().BeNull();

        _emailService.SentTwoFactorCodes.Should().ContainSingle()
            .Which.Email.Should().Be(user.Email);

        _userManager.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u =>
            u.TwoFactorEmailCode != null &&
            u.TwoFactorEmailCodeExpiry > DateTime.UtcNow)), Times.Once);
    }

    // ---------------------------------------------------------------
    // VerifyTwoFactorAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task VerifyTwoFactorAsync_ValidCode_ClearsCodeAndReturnsToken()
    {
        var code = "654321";
        var user = CreateUser(
            twoFactorEnabled: true,
            twoFactorCode: code,
            twoFactorExpiry: DateTime.UtcNow.AddMinutes(5));

        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Candidate" });
        _jwtTokenGenerator.Setup(x => x.GenerateToken(user, "Candidate"))
            .Returns(("jwt-token-2fa", DateTime.UtcNow.AddHours(1)));

        var request = new TwoFactorRequest { Email = user.Email!, Code = code };
        var result = await _sut.VerifyTwoFactorAsync(request);

        result.Token.Should().Be("jwt-token-2fa");
        result.ExpiresAt.Should().NotBeNull();

        _userManager.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u =>
            u.TwoFactorEmailCode == null &&
            u.TwoFactorEmailCodeExpiry == null)), Times.Once);
    }

    [Fact]
    public async Task VerifyTwoFactorAsync_InvalidCode_ThrowsUnauthorizedAccessException()
    {
        var user = CreateUser(
            twoFactorEnabled: true,
            twoFactorCode: "654321",
            twoFactorExpiry: DateTime.UtcNow.AddMinutes(5));

        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var request = new TwoFactorRequest { Email = user.Email!, Code = "000000" };
        var act = () => _sut.VerifyTwoFactorAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid verification code*");
    }

    [Fact]
    public async Task VerifyTwoFactorAsync_ExpiredCode_ThrowsUnauthorizedAccessException()
    {
        var code = "654321";
        var user = CreateUser(
            twoFactorEnabled: true,
            twoFactorCode: code,
            twoFactorExpiry: DateTime.UtcNow.AddMinutes(-5));

        _userManager.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var request = new TwoFactorRequest { Email = user.Email!, Code = code };
        var act = () => _sut.VerifyTwoFactorAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task VerifyTwoFactorAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _userManager.Setup(x => x.FindByEmailAsync("unknown@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var request = new TwoFactorRequest { Email = "unknown@example.com", Code = "123456" };
        var act = () => _sut.VerifyTwoFactorAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid request*");
    }

    // ---------------------------------------------------------------
    // Toggle2FAAsync
    // ---------------------------------------------------------------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Toggle2FAAsync_ValidUser_SetsEnabledFlagAndClearsCodes(bool enable)
    {
        var user = CreateUser(twoFactorEnabled: !enable, twoFactorCode: "111111",
            twoFactorExpiry: DateTime.UtcNow.AddMinutes(5));

        _userManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.Toggle2FAAsync(user.Id, enable);

        _userManager.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u =>
            u.TwoFactorEmailEnabled == enable &&
            u.TwoFactorEmailCode == null &&
            u.TwoFactorEmailCodeExpiry == null)), Times.Once);
    }

    [Fact]
    public async Task Toggle2FAAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        _userManager.Setup(x => x.FindByIdAsync("nonexistent"))
            .ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.Toggle2FAAsync("nonexistent", true);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ---------------------------------------------------------------
    // GetCurrentUserAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetCurrentUserAsync_ValidId_ReturnsUserInfo()
    {
        var user = CreateUser(id: "user-42", fullName: "Jane Smith");

        _userManager.Setup(x => x.FindByIdAsync("user-42")).ReturnsAsync(user);
        _userManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Employer" });

        var result = await _sut.GetCurrentUserAsync("user-42");

        result.UserId.Should().Be("user-42");
        result.Email.Should().Be(user.Email);
        result.FullName.Should().Be("Jane Smith");
        result.Role.Should().Be("Employer");
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_NoRoles_DefaultsToCandidate()
    {
        var user = CreateUser();
        _userManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var result = await _sut.GetCurrentUserAsync(user.Id);

        result.Role.Should().Be("Candidate");
    }

    [Fact]
    public async Task GetCurrentUserAsync_InvalidId_ThrowsInvalidOperationException()
    {
        _userManager.Setup(x => x.FindByIdAsync("bad-id"))
            .ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.GetCurrentUserAsync("bad-id");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
