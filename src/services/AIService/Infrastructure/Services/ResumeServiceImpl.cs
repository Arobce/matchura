using System.Text.Json;
using System.Threading.Channels;
using AIService.Application.DTOs;
using AIService.Application.Interfaces;
using AIService.Domain.Entities;
using AIService.Domain.Enums;
using AIService.Infrastructure.Data;
using AIService.Infrastructure.TextExtraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIService.Infrastructure.Services;

public class ResumeServiceImpl : IResumeService
{
    private readonly AIDbContext _db;
    private readonly IEnumerable<ITextExtractor> _extractors;
    private readonly Channel<Guid> _channel;
    private readonly ILogger<ResumeServiceImpl> _logger;

    public ResumeServiceImpl(
        AIDbContext db,
        IEnumerable<ITextExtractor> extractors,
        Channel<Guid> channel,
        ILogger<ResumeServiceImpl> logger)
    {
        _db = db;
        _extractors = extractors;
        _channel = channel;
        _logger = logger;
    }

    public async Task<ResumeUploadResponse> UploadResumeAsync(string candidateId, Stream fileStream, string fileName, string contentType)
    {
        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(contentType))
            ?? throw new InvalidOperationException($"Unsupported file type: {contentType}. Supported: PDF, DOCX");

        var resume = new Resume
        {
            CandidateId = candidateId,
            OriginalFileName = fileName,
            FileUrl = $"uploads/{candidateId}/{Guid.NewGuid()}/{fileName}",
            ContentType = contentType,
            ParseStatus = ParseStatus.Extracting
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        try
        {
            var rawText = await extractor.ExtractTextAsync(fileStream);

            if (string.IsNullOrWhiteSpace(rawText))
            {
                resume.ParseStatus = ParseStatus.Failed;
                resume.ErrorMessage = "No text content could be extracted from the file";
                await _db.SaveChangesAsync();
                return new ResumeUploadResponse
                {
                    ResumeId = resume.ResumeId,
                    Status = resume.ParseStatus.ToString(),
                    Message = resume.ErrorMessage
                };
            }

            resume.RawText = rawText;
            resume.ParseStatus = ParseStatus.Uploaded;
            await _db.SaveChangesAsync();

            // Queue for AI parsing
            await _channel.Writer.WriteAsync(resume.ResumeId);

            _logger.LogInformation("Resume {ResumeId} uploaded and queued for parsing", resume.ResumeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Text extraction failed for resume {ResumeId}", resume.ResumeId);
            resume.ParseStatus = ParseStatus.Failed;
            resume.ErrorMessage = $"Text extraction failed: {ex.Message}";
            await _db.SaveChangesAsync();
        }

        return new ResumeUploadResponse
        {
            ResumeId = resume.ResumeId,
            Status = resume.ParseStatus.ToString(),
            Message = resume.ParseStatus == ParseStatus.Failed
                ? resume.ErrorMessage!
                : "Resume uploaded and queued for parsing"
        };
    }

    public async Task<ResumeResponse> GetResumeByIdAsync(Guid resumeId, string candidateId)
    {
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.ResumeId == resumeId && r.CandidateId == candidateId)
            ?? throw new InvalidOperationException("Resume not found");

        return MapToResponse(resume);
    }

    public async Task<ResumeStatusResponse> GetResumeStatusAsync(Guid resumeId, string candidateId)
    {
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.ResumeId == resumeId && r.CandidateId == candidateId)
            ?? throw new InvalidOperationException("Resume not found");

        return new ResumeStatusResponse
        {
            ResumeId = resume.ResumeId,
            Status = resume.ParseStatus.ToString(),
            ErrorMessage = resume.ErrorMessage
        };
    }

    public async Task<List<ResumeResponse>> GetResumesByCandidateAsync(string candidateId)
    {
        var resumes = await _db.Resumes
            .Where(r => r.CandidateId == candidateId)
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync();

        return resumes.Select(MapToResponse).ToList();
    }

    private static ResumeResponse MapToResponse(Resume r) => new()
    {
        ResumeId = r.ResumeId,
        CandidateId = r.CandidateId,
        OriginalFileName = r.OriginalFileName,
        Status = r.ParseStatus.ToString(),
        ErrorMessage = r.ErrorMessage,
        ParsedData = string.IsNullOrEmpty(r.ParsedData) ? null
            : JsonSerializer.Deserialize<ParsedResumeData>(r.ParsedData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
        UploadedAt = r.UploadedAt,
        ParsedAt = r.ParsedAt
    };
}
