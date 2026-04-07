using System.Security.Cryptography;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthServiceImpl> _logger;

    public AuthServiceImpl(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService,
        ILogger<AuthServiceImpl> logger)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("An account with this email already exists");

        var verificationCode = GenerateSecureCode();

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = $"{request.FirstName} {request.LastName}".Trim(),
            AccountStatus = AccountStatus.Active,
            EmailConfirmed = false,
            EmailVerificationCode = verificationCode,
            EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15)
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        await _emailService.SendVerificationCodeAsync(user.Email!, verificationCode);

        _logger.LogInformation("User {Email} registered with role {Role}, verification email sent", request.Email, request.Role);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = request.Role,
            RequiresEmailVerification = true
        };
    }

    public async Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("User not found");

        if (user.EmailConfirmed)
            throw new InvalidOperationException("Email is already verified");

        if (user.EmailVerificationCode != request.Code)
            throw new UnauthorizedAccessException("Invalid verification code");

        if (user.EmailVerificationCodeExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Verification code has expired");

        user.EmailConfirmed = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationCodeExpiry = null;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Candidate";
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user, role);

        _logger.LogInformation("User {Email} verified their email", request.Email);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = role,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public async Task ResendVerificationAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException("User not found");

        if (user.EmailConfirmed)
            throw new InvalidOperationException("Email is already verified");

        var code = GenerateSecureCode();
        user.EmailVerificationCode = code;
        user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);

        await _emailService.SendVerificationCodeAsync(user.Email!, code);
        _logger.LogInformation("Verification code resent to {Email}", email);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.EmailConfirmed)
            throw new UnauthorizedAccessException("Please verify your email before logging in");

        if (user.AccountStatus == AccountStatus.Suspended)
            throw new UnauthorizedAccessException("Account is suspended");

        if (user.AccountStatus == AccountStatus.Deactivated)
            throw new UnauthorizedAccessException("Account is deactivated");

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            throw new UnauthorizedAccessException("Invalid email or password");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Candidate";

        if (user.TwoFactorEmailEnabled)
        {
            var code = GenerateSecureCode();
            user.TwoFactorEmailCode = code;
            user.TwoFactorEmailCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);
            await _emailService.SendTwoFactorCodeAsync(user.Email!, code);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = role,
                RequiresTwoFactor = true
            };
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user, role);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = role,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponse> VerifyTwoFactorAsync(TwoFactorRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid request");

        if (user.TwoFactorEmailCode != request.Code)
            throw new UnauthorizedAccessException("Invalid verification code");

        if (user.TwoFactorEmailCodeExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Verification code has expired");

        user.TwoFactorEmailCode = null;
        user.TwoFactorEmailCodeExpiry = null;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Candidate";
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user, role);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = role,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public async Task Toggle2FAAsync(string userId, bool enable)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        user.TwoFactorEmailEnabled = enable;
        user.TwoFactorEmailCode = null;
        user.TwoFactorEmailCodeExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    public async Task<AuthResponse> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Candidate";

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = role
        };
    }

    private static string GenerateSecureCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }
}
