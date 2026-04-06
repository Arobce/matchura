using AIService.Domain.Enums;

namespace AIService.Domain.Entities;

public class Resume
{
    public Guid ResumeId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? RawText { get; set; }
    public string? ParsedData { get; set; }
    public ParseStatus ParseStatus { get; set; } = ParseStatus.Uploaded;
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ParsedAt { get; set; }
}
