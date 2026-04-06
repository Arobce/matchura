using System.Security.Claims;
using AIService.Application.DTOs;
using AIService.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIService.API.Controllers;

[ApiController]
[Route("api/skillgap")]
[Authorize(Roles = "Candidate")]
public class SkillGapController : ControllerBase
{
    private readonly ISkillGapService _skillGapService;
    private readonly IValidator<AnalyzeSkillGapRequest> _validator;

    public SkillGapController(ISkillGapService skillGapService, IValidator<AnalyzeSkillGapRequest> validator)
    {
        _skillGapService = skillGapService;
        _validator = validator;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeSkillGapRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        try
        {
            var result = await _skillGapService.AnalyzeAsync(candidateId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("candidate/me/job/{jobId:guid}")]
    public async Task<IActionResult> GetReport(Guid jobId)
    {
        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _skillGapService.GetReportAsync(candidateId, jobId);
        if (result == null) return NotFound(new { error = "No skill gap report found for this job" });
        return Ok(result);
    }

    [HttpGet("candidate/me/reports")]
    public async Task<IActionResult> GetMyReports()
    {
        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _skillGapService.GetReportsForCandidateAsync(candidateId);
        return Ok(result);
    }
}
