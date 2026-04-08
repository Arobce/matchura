using System.Security.Claims;
using AIService.Application.DTOs;
using AIService.Infrastructure.Services;
using AIService.Infrastructure.TextExtraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIService.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize(Roles = "Candidate")]
public class DocumentController : ControllerBase
{
    private readonly IS3StorageService _s3;
    private readonly IEnumerable<ITextExtractor> _extractors;

    public DocumentController(IS3StorageService s3, IEnumerable<ITextExtractor> extractors)
    {
        _s3 = s3;
        _extractors = extractors;
    }

    /// <summary>
    /// Upload a PDF document (e.g. cover letter), store in S3, and extract text.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { error = "File size cannot exceed 5MB" });

        if (file.ContentType != "application/pdf")
            return BadRequest(new { error = "Only PDF files are supported" });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();

        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(file.ContentType))
            ?? throw new InvalidOperationException("No extractor available for PDF");

        using var buffer = new MemoryStream();
        await file.OpenReadStream().CopyToAsync(buffer);

        // Upload to S3
        var s3Key = $"coverletters/{userId}/{Guid.NewGuid()}/{file.FileName}";
        buffer.Position = 0;
        await _s3.UploadFileAsync(buffer, s3Key, file.ContentType);

        // Extract text
        buffer.Position = 0;
        var extractedText = await extractor.ExtractTextAsync(buffer);

        return Ok(new DocumentUploadResponse
        {
            FileUrl = s3Key,
            ExtractedText = extractedText ?? string.Empty
        });
    }

    /// <summary>
    /// Get a presigned download URL for a document stored in S3.
    /// </summary>
    [HttpGet("download")]
    [Authorize(Roles = "Candidate,Employer")]
    public async Task<IActionResult> Download([FromQuery] string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return BadRequest(new { error = "File key is required" });

        var url = await _s3.GetPresignedUrlAsync(key, TimeSpan.FromMinutes(15));
        return Ok(new { downloadUrl = url });
    }
}
