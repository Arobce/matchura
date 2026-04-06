using AIService.Application.DTOs;

namespace AIService.Application.Interfaces;

public interface IMatchingService
{
    Task<MatchScoreResponse> ComputeMatchAsync(string candidateId, ComputeMatchRequest request);
    Task<MatchListResponse> GetMatchesForJobAsync(string employerId, Guid jobId, int page, int pageSize);
    Task<MatchListResponse> GetRecommendedJobsAsync(string candidateId, int page, int pageSize);
}
