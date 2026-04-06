using AIService.Application.DTOs;

namespace AIService.Application.Interfaces;

public interface IResumeService
{
    Task<ResumeUploadResponse> UploadResumeAsync(string candidateId, Stream fileStream, string fileName, string contentType);
    Task<ResumeResponse> GetResumeByIdAsync(Guid resumeId, string candidateId);
    Task<ResumeStatusResponse> GetResumeStatusAsync(Guid resumeId, string candidateId);
    Task<List<ResumeResponse>> GetResumesByCandidateAsync(string candidateId);
}
