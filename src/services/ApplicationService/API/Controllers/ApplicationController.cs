using System.Security.Claims;
using ApplicationService.Application.DTOs;
using ApplicationService.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationService.API.Controllers;

[ApiController]
[Route("api/applications")]
[Produces("application/json")]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IValidator<CreateApplicationRequest> _createValidator;
    private readonly IValidator<UpdateEmployerNotesRequest> _notesValidator;

    public ApplicationController(
        IApplicationService applicationService,
        IValidator<CreateApplicationRequest> createValidator,
        IValidator<UpdateEmployerNotesRequest> notesValidator)
    {
        _applicationService = applicationService;
        _createValidator = createValidator;
        _notesValidator = notesValidator;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string GetRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Candidate";

    /// <summary>
    /// Apply to a job
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
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
            var candidateName = User.FindFirst(ClaimTypes.Name)?.Value;
            var result = await _applicationService.CreateApplicationAsync(userId, candidateName, request);
            return CreatedAtAction(nameof(GetApplication), new { id = result.ApplicationId }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already applied"))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Duplicate application",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// List own applications (candidate)
    /// </summary>
    [HttpGet("my-applications")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(ApplicationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyApplications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _applicationService.GetMyApplicationsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get application detail (candidate sees own, employer sees for own jobs)
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Candidate,Employer")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await _applicationService.GetApplicationByIdAsync(id, userId, GetRole());
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails { Title = "Application not found", Status = StatusCodes.Status404NotFound });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
    }

    /// <summary>
    /// Withdraw an application
    /// </summary>
    [HttpPut("{id:guid}/withdraw")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> WithdrawApplication(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await _applicationService.WithdrawApplicationAsync(userId, id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
    }

    /// <summary>
    /// List applications for a specific job (employer only)
    /// </summary>
    [HttpGet("job/{jobId:guid}")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(ApplicationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetApplicationsForJob(Guid jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await _applicationService.GetApplicationsForJobAsync(userId, jobId, page, pageSize);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
    }

    /// <summary>
    /// Update application status (employer only)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateApplicationStatus(Guid id, [FromBody] UpdateApplicationStatusRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await _applicationService.UpdateApplicationStatusAsync(userId, id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
    }

    /// <summary>
    /// Add or update employer notes on an application
    /// </summary>
    [HttpPatch("{id:guid}/notes")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateEmployerNotes(Guid id, [FromBody] UpdateEmployerNotesRequest request)
    {
        var validation = await _notesValidator.ValidateAsync(request);
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
            var result = await _applicationService.UpdateEmployerNotesAsync(userId, id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
    }
}
