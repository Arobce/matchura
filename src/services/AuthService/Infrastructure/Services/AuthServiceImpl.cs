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

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            AccountStatus = AccountStatus.Active
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user, request.Role);

        _logger.LogInformation("User {Email} registered with role {Role}", request.Email, request.Role);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = request.Role,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

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
