using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Application.DTOs;
using ProfileService.Application.Interfaces;

namespace ProfileService.API.Controllers;

[ApiController]
[Route("api/profiles")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IValidator<CreateCandidateProfileRequest> _createCandidateValidator;
    private readonly IValidator<UpdateCandidateProfileRequest> _updateCandidateValidator;
    private readonly IValidator<CreateEmployerProfileRequest> _createEmployerValidator;
    private readonly IValidator<UpdateEmployerProfileRequest> _updateEmployerValidator;

    public ProfileController(
        IProfileService profileService,
        IValidator<CreateCandidateProfileRequest> createCandidateValidator,
        IValidator<UpdateCandidateProfileRequest> updateCandidateValidator,
        IValidator<CreateEmployerProfileRequest> createEmployerValidator,
        IValidator<UpdateEmployerProfileRequest> updateEmployerValidator)
    {
        _profileService = profileService;
        _createCandidateValidator = createCandidateValidator;
        _updateCandidateValidator = updateCandidateValidator;
        _createEmployerValidator = createEmployerValidator;
        _updateEmployerValidator = updateEmployerValidator;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // ── Candidate endpoints ──

    /// <summary>
    /// Create candidate profile (linked to authenticated user)
    /// </summary>
    [HttpPost("candidate")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(CandidateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCandidateProfile([FromBody] CreateCandidateProfileRequest request)
    {
        var validation = await _createCandidateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _profileService.CreateCandidateProfileAsync(userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Profile already exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Get own candidate profile
    /// </summary>
    [HttpGet("candidate/me")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(CandidateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyCandidateProfile()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _profileService.GetCandidateProfileAsync(userId);
            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profile not found",
                Detail = "No candidate profile exists for this user",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Update own candidate profile
    /// </summary>
    [HttpPut("candidate/me")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(CandidateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyCandidateProfile([FromBody] UpdateCandidateProfileRequest request)
    {
        var validation = await _updateCandidateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _profileService.UpdateCandidateProfileAsync(userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profile not found",
                Detail = "No candidate profile exists for this user",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Get public candidate profile by ID (limited fields)
    /// </summary>
    [HttpGet("candidate/{id:guid}")]
    [ProducesResponseType(typeof(CandidateProfilePublicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidateProfilePublic(Guid id)
    {
        try
        {
            var response = await _profileService.GetCandidateProfilePublicAsync(id);
            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profile not found",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    // ── Employer endpoints ──

    /// <summary>
    /// Create employer profile (linked to authenticated user)
    /// </summary>
    [HttpPost("employer")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(EmployerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEmployerProfile([FromBody] CreateEmployerProfileRequest request)
    {
        var validation = await _createEmployerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _profileService.CreateEmployerProfileAsync(userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Profile already exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Get own employer profile
    /// </summary>
    [HttpGet("employer/me")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(EmployerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyEmployerProfile()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _profileService.GetEmployerProfileAsync(userId);
            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profile not found",
                Detail = "No employer profile exists for this user",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Update own employer profile
    /// </summary>
    [HttpPut("employer/me")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(EmployerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyEmployerProfile([FromBody] UpdateEmployerProfileRequest request)
    {
        var validation = await _updateEmployerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _profileService.UpdateEmployerProfileAsync(userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profile not found",
                Detail = "No employer profile exists for this user",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Get public employer/company profile by ID
    /// </summary>
    [HttpGet("employer/{id:guid}")]
    [ProducesResponseType(typeof(EmployerProfilePublicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployerProfilePublic(Guid id)
    {
        try
        {
            var response = await _profileService.GetEmployerProfilePublicAsync(id);
            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profile not found",
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}
