using System.Security.Claims;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<TwoFactorRequest> _twoFactorValidator;
    private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<TwoFactorRequest> twoFactorValidator,
        IValidator<VerifyEmailRequest> verifyEmailValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _twoFactorValidator = twoFactorValidator;
        _verifyEmailValidator = verifyEmailValidator;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details including email, password, full name, and role</param>
    /// <returns>User info with JWT token</returns>
    /// <response code="200">Registration successful</response>
    /// <response code="400">Validation error or invalid role</response>
    /// <response code="409">Email already taken</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Email already taken",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Registration failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Verify email address with the code sent during registration
    /// </summary>
    /// <param name="request">Email and 6-digit verification code</param>
    /// <returns>JWT token on successful verification</returns>
    /// <response code="200">Email verified, token issued</response>
    /// <response code="400">Validation error or already verified</response>
    /// <response code="401">Invalid or expired code</response>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var validation = await _verifyEmailValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        try
        {
            var response = await _authService.VerifyEmailAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Verification failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Verification failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }

    /// <summary>
    /// Resend email verification code
    /// </summary>
    /// <param name="request">Email address to resend the code to</param>
    /// <response code="204">Verification code resent</response>
    /// <response code="400">Email already verified or user not found</response>
    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        try
        {
            await _authService.ResendVerificationAsync(request.Email);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Request failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Authenticate with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token or 2FA challenge</returns>
    /// <response code="200">Login successful or 2FA code sent</response>
    /// <response code="401">Invalid credentials or suspended account</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }

    /// <summary>
    /// Verify 2FA email code to complete login
    /// </summary>
    /// <param name="request">Email and 6-digit verification code</param>
    /// <returns>JWT token on success</returns>
    /// <response code="200">Verification successful, token issued</response>
    /// <response code="401">Invalid or expired code</response>
    [HttpPost("verify-2fa")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorRequest request)
    {
        var validation = await _twoFactorValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        try
        {
            var response = await _authService.VerifyTwoFactorAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Verification failed",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }

    /// <summary>
    /// Get current authenticated user info
    /// </summary>
    /// <returns>Current user details</returns>
    /// <response code="200">User info returned</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var response = await _authService.GetCurrentUserAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Enable or disable email-based two-factor authentication
    /// </summary>
    /// <param name="request">Whether to enable or disable 2FA</param>
    /// <response code="204">2FA setting updated</response>
    /// <response code="401">Not authenticated</response>
    [HttpPost("2fa/toggle")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Toggle2FA([FromBody] Enable2FARequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _authService.Toggle2FAAsync(userId, request.Enable);
        return NoContent();
    }
}
