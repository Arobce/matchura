using AIService.Application.DTOs;

namespace AIService.Application.Interfaces;

public interface IAnalyticsService
{
    Task<EmployerDashboardResponse> GetDashboardAsync(string employerId);
    Task<JobAnalyticsResponse> GetJobAnalyticsAsync(string employerId, Guid jobId);
    Task<TrendDataResponse> GetTrendsAsync(string employerId);
}
