using ApplicationService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationService.API.Controllers;

/// <summary>
/// Internal endpoints for service-to-service communication (no auth required)
/// </summary>
[ApiController]
[Route("internal/applications")]
public class InternalController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public InternalController(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Get all applications for a job (used by analytics service)
    /// </summary>
    [HttpGet("job/{jobId:guid}")]
    public async Task<IActionResult> GetApplicationsForJob(Guid jobId)
    {
        var apps = await _db.Applications
            .Where(a => a.JobId == jobId)
            .Select(a => new { a.ApplicationId, a.CandidateId, a.JobId, Status = a.Status.ToString(), a.AppliedAt })
            .ToListAsync();

        return Ok(apps);
    }
}
