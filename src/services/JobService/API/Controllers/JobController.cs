using System.Security.Claims;
using FluentValidation;
using JobService.Application.DTOs;
using JobService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobService.API.Controllers;

[ApiController]
[Route("api/jobs")]
[Produces("application/json")]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IValidator<CreateJobRequest> _createValidator;
    private readonly IValidator<UpdateJobRequest> _updateValidator;

    public JobController(
        IJobService jobService,
        IValidator<CreateJobRequest> createValidator,
        IValidator<UpdateJobRequest> updateValidator)
    {
        _jobService = jobService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// List active jobs with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(JobListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobs([FromQuery] JobQueryParams queryParams)
    {
        var result = await _jobService.GetJobsAsync(queryParams);
        return Ok(result);
    }

    /// <summary>
    /// Get job detail by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(Guid id)
    {
        try
        {
            var result = await _jobService.GetJobByIdAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Job not found",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Create a new job posting
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
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

        var result = await _jobService.CreateJobAsync(userId, request);
        return CreatedAtAction(nameof(GetJob), new { id = result.JobId }, result);
    }

    /// <summary>
    /// Update an existing job posting
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateJob(Guid id, [FromBody] UpdateJobRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
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
            var result = await _jobService.UpdateJobAsync(userId, id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
    }

    /// <summary>
    /// Change job status (Draft→Active, Active→Closed)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateJobStatus(Guid id, [FromBody] UpdateJobStatusRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await _jobService.UpdateJobStatusAsync(userId, id, request);
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
    /// List own job postings
    /// </summary>
    [HttpGet("my-jobs")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(typeof(JobListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _jobService.GetMyJobsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Soft-delete a job (sets status to Closed)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Employer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJob(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _jobService.DeleteJobAsync(userId, id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
    }
}
