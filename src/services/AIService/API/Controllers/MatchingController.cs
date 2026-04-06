using System.Security.Claims;
using AIService.Application.DTOs;
using AIService.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIService.API.Controllers;

[ApiController]
[Route("api/matching")]
[Authorize]
public class MatchingController : ControllerBase
{
    private readonly IMatchingService _matchingService;
    private readonly IValidator<ComputeMatchRequest> _validator;

    public MatchingController(IMatchingService matchingService, IValidator<ComputeMatchRequest> validator)
    {
        _matchingService = matchingService;
        _validator = validator;
    }

    [HttpPost("compute")]
    public async Task<IActionResult> ComputeMatch([FromBody] ComputeMatchRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        try
        {
            var result = await _matchingService.ComputeMatchAsync(candidateId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("job/{jobId:guid}/candidates")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetMatchesForJob(Guid jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var employerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        try
        {
            var result = await _matchingService.GetMatchesForJobAsync(employerId, jobId, page, pageSize);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("candidate/me/jobs")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetRecommendedJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _matchingService.GetRecommendedJobsAsync(candidateId, page, pageSize);
        return Ok(result);
    }
}
