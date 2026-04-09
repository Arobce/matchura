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
    private readonly IS3StorageService _s3;
    private readonly ILogger<ResumeServiceImpl> _logger;

    public ResumeServiceImpl(
        AIDbContext db,
        IEnumerable<ITextExtractor> extractors,
        Channel<Guid> channel,
        IS3StorageService s3,
        ILogger<ResumeServiceImpl> logger)
    {
        _db = db;
        _extractors = extractors;
        _channel = channel;
        _s3 = s3;
        _logger = logger;
    }

    public async Task<ResumeUploadResponse> UploadResumeAsync(string candidateId, Stream fileStream, string fileName, string contentType)
    {
        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(contentType))
            ?? throw new InvalidOperationException($"Unsupported file type: {contentType}. Supported: PDF, DOCX");

        // Buffer the stream so we can use it for both text extraction and S3 upload
        using var buffer = new MemoryStream();
        await fileStream.CopyToAsync(buffer);

        var resume = new Resume
        {
            CandidateId = candidateId,
            OriginalFileName = fileName,
            ContentType = contentType,
            ParseStatus = ParseStatus.Extracting
        };

        var s3Key = $"resumes/{candidateId}/{resume.ResumeId}/{fileName}";
        resume.FileUrl = s3Key;

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        try
        {
            // Upload to S3 (AWS SDK disposes the input stream, so use a copy)
            buffer.Position = 0;
            using var s3Stream = new MemoryStream(buffer.ToArray());
            await _s3.UploadFileAsync(s3Stream, s3Key, contentType);

            // Extract text
            buffer.Position = 0;
            var rawText = await extractor.ExtractTextAsync(buffer);

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

            _logger.LogInformation("Resume {ResumeId} uploaded to S3 and queued for parsing", resume.ResumeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload/extraction failed for resume {ResumeId}", resume.ResumeId);
            resume.ParseStatus = ParseStatus.Failed;
            resume.ErrorMessage = $"Upload failed: {ex.Message}";
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
        FileUrl = r.FileUrl,
        Status = r.ParseStatus.ToString(),
        ErrorMessage = r.ErrorMessage,
        ParsedData = string.IsNullOrEmpty(r.ParsedData) ? null
            : JsonSerializer.Deserialize<ParsedResumeData>(r.ParsedData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
        UploadedAt = r.UploadedAt,
        ParsedAt = r.ParsedAt
    };
}
