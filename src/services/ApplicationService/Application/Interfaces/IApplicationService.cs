using ApplicationService.Application.DTOs;

namespace ApplicationService.Application.Interfaces;

public interface IApplicationService
{
    Task<ApplicationResponse> CreateApplicationAsync(string candidateId, CreateApplicationRequest request);
    Task<ApplicationListResponse> GetMyApplicationsAsync(string candidateId, int page, int pageSize);
    Task<ApplicationResponse> GetApplicationByIdAsync(Guid applicationId, string userId, string role);
    Task<ApplicationResponse> WithdrawApplicationAsync(string candidateId, Guid applicationId);
    Task<ApplicationListResponse> GetApplicationsForJobAsync(string employerId, Guid jobId, int page, int pageSize);
    Task<ApplicationResponse> UpdateApplicationStatusAsync(string employerId, Guid applicationId, UpdateApplicationStatusRequest request);
    Task<ApplicationResponse> UpdateEmployerNotesAsync(string employerId, Guid applicationId, UpdateEmployerNotesRequest request);
}
