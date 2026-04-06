using JobService.Application.DTOs;

namespace JobService.Application.Interfaces;

public interface IJobService
{
    // Jobs
    Task<JobResponse> CreateJobAsync(string employerId, CreateJobRequest request);
    Task<JobResponse> UpdateJobAsync(string employerId, Guid jobId, UpdateJobRequest request);
    Task<JobResponse> UpdateJobStatusAsync(string employerId, Guid jobId, UpdateJobStatusRequest request);
    Task DeleteJobAsync(string employerId, Guid jobId);
    Task<JobResponse> GetJobByIdAsync(Guid jobId);
    Task<JobListResponse> GetJobsAsync(JobQueryParams queryParams);
    Task<JobListResponse> GetMyJobsAsync(string employerId, int page, int pageSize);

    // Skills
    Task<List<SkillResponse>> GetSkillsAsync(string? category);
    Task<SkillResponse> CreateSkillAsync(CreateSkillRequest request);
    Task<List<string>> GetSkillCategoriesAsync();
}
