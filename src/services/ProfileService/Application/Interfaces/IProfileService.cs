using ProfileService.Application.DTOs;

namespace ProfileService.Application.Interfaces;

public interface IProfileService
{
    Task<CandidateProfileResponse> CreateCandidateProfileAsync(string userId, CreateCandidateProfileRequest request);
    Task<CandidateProfileResponse> GetCandidateProfileAsync(string userId);
    Task<CandidateProfileResponse> UpdateCandidateProfileAsync(string userId, UpdateCandidateProfileRequest request);
    Task<CandidateProfilePublicResponse> GetCandidateProfilePublicAsync(Guid candidateId);

    Task<EmployerProfileResponse> CreateEmployerProfileAsync(string userId, CreateEmployerProfileRequest request);
    Task<EmployerProfileResponse> GetEmployerProfileAsync(string userId);
    Task<EmployerProfileResponse> UpdateEmployerProfileAsync(string userId, UpdateEmployerProfileRequest request);
    Task<EmployerProfilePublicResponse> GetEmployerProfilePublicAsync(Guid employerId);
}
