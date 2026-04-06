using System.Security.Claims;
using AIService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIService.API.Controllers;

[ApiController]
[Route("api/analytics/employer")]
[Authorize(Roles = "Employer")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var employerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _analyticsService.GetDashboardAsync(employerId);
        return Ok(result);
    }

    [HttpGet("job/{jobId:guid}")]
    public async Task<IActionResult> GetJobAnalytics(Guid jobId)
    {
        var employerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        try
        {
            var result = await _analyticsService.GetJobAnalyticsAsync(employerId, jobId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends()
    {
        var employerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _analyticsService.GetTrendsAsync(employerId);
        return Ok(result);
    }
}
