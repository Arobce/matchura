using AIService.Application.DTOs;

namespace AIService.Application.Interfaces;

public interface ISkillGapService
{
    Task<SkillGapReportResponse> AnalyzeAsync(string candidateId, AnalyzeSkillGapRequest request);
    Task<SkillGapReportResponse?> GetReportAsync(string candidateId, Guid jobId);
    Task<List<SkillGapReportResponse>> GetReportsForCandidateAsync(string candidateId);
}
