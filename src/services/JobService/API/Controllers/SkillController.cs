using FluentValidation;
using JobService.Application.DTOs;
using JobService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobService.API.Controllers;

[ApiController]
[Route("api/skills")]
[Produces("application/json")]
public class SkillController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IValidator<CreateSkillRequest> _createValidator;

    public SkillController(IJobService jobService, IValidator<CreateSkillRequest> createValidator)
    {
        _jobService = jobService;
        _createValidator = createValidator;
    }

    /// <summary>
    /// List all skills, optionally filtered by category
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SkillResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills([FromQuery] string? category)
    {
        var result = await _jobService.GetSkillsAsync(category);
        return Ok(result);
    }

    /// <summary>
    /// Add a new skill (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SkillResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });

        try
        {
            var result = await _jobService.CreateSkillAsync(request);
            return Created($"/api/skills/{result.SkillId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Skill already exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// List distinct skill categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _jobService.GetSkillCategoriesAsync();
        return Ok(result);
    }
}
