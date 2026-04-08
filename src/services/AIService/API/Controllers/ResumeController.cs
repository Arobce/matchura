using System.Security.Claims;
using AIService.Application.Interfaces;
using AIService.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIService.API.Controllers;

[ApiController]
[Route("api/resumes")]
[Authorize(Roles = "Candidate")]
public class ResumeController : ControllerBase
{
    private readonly IResumeService _resumeService;
    private readonly IS3StorageService _s3;

    public ResumeController(IResumeService resumeService, IS3StorageService s3)
    {
        _resumeService = resumeService;
        _s3 = s3;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "File size cannot exceed 10MB" });

        var allowedTypes = new[] { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { error = "Only PDF and DOCX files are supported" });

        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        using var stream = file.OpenReadStream();
        var result = await _resumeService.UploadResumeAsync(candidateId, stream, file.FileName, file.ContentType);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        try
        {
            var result = await _resumeService.GetResumeByIdAsync(id, candidateId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        try
        {
            var result = await _resumeService.GetResumeStatusAsync(id, candidateId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyResumes()
    {
        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var result = await _resumeService.GetResumesByCandidateAsync(candidateId);
        return Ok(result);
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Roles = "Candidate,Employer")]
    public async Task<IActionResult> Download(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        try
        {
            var resume = await _resumeService.GetResumeByIdAsync(id, userId);
            var url = await _s3.GetPresignedUrlAsync(resume.FileUrl, TimeSpan.FromMinutes(15));
            return Ok(new { downloadUrl = url });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = "Resume not found" });
        }
    }
}
