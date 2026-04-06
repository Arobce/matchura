using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProfileService.Application.DTOs;
using ProfileService.Application.Interfaces;
using ProfileService.Domain.Entities;
using ProfileService.Infrastructure.Data;

namespace ProfileService.Infrastructure.Services;

public class ProfileServiceImpl : IProfileService
{
    private readonly ProfileDbContext _db;
    private readonly ILogger<ProfileServiceImpl> _logger;

    public ProfileServiceImpl(ProfileDbContext db, ILogger<ProfileServiceImpl> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CandidateProfileResponse> CreateCandidateProfileAsync(string userId, CreateCandidateProfileRequest request)
    {
        var existing = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null)
            throw new InvalidOperationException("Candidate profile already exists");

        var profile = new CandidateProfile
        {
            UserId = userId,
            Phone = request.Phone,
            Location = request.Location,
            ProfessionalSummary = request.ProfessionalSummary,
            YearsOfExperience = request.YearsOfExperience,
            HighestEducation = request.HighestEducation,
            LinkedinUrl = request.LinkedinUrl
        };

        _db.CandidateProfiles.Add(profile);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Candidate profile created for user {UserId}", userId);
        return MapToResponse(profile);
    }

    public async Task<CandidateProfileResponse> GetCandidateProfileAsync(string userId)
    {
        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new InvalidOperationException("Candidate profile not found");

        return MapToResponse(profile);
    }

    public async Task<CandidateProfileResponse> UpdateCandidateProfileAsync(string userId, UpdateCandidateProfileRequest request)
    {
        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new InvalidOperationException("Candidate profile not found");

        profile.Phone = request.Phone;
        profile.Location = request.Location;
        profile.ProfessionalSummary = request.ProfessionalSummary;
        profile.YearsOfExperience = request.YearsOfExperience;
        profile.HighestEducation = request.HighestEducation;
        profile.LinkedinUrl = request.LinkedinUrl;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Candidate profile updated for user {UserId}", userId);
        return MapToResponse(profile);
    }

    public async Task<CandidateProfilePublicResponse> GetCandidateProfilePublicAsync(Guid candidateId)
    {
        var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.CandidateId == candidateId)
            ?? throw new InvalidOperationException("Candidate profile not found");

        return new CandidateProfilePublicResponse
        {
            CandidateId = profile.CandidateId,
            Location = profile.Location,
            ProfessionalSummary = profile.ProfessionalSummary,
            YearsOfExperience = profile.YearsOfExperience,
            HighestEducation = profile.HighestEducation
        };
    }

    public async Task<EmployerProfileResponse> CreateEmployerProfileAsync(string userId, CreateEmployerProfileRequest request)
    {
        var existing = await _db.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing != null)
            throw new InvalidOperationException("Employer profile already exists");

        var profile = new EmployerProfile
        {
            UserId = userId,
            CompanyName = request.CompanyName,
            CompanyDescription = request.CompanyDescription,
            Industry = request.Industry,
            WebsiteUrl = request.WebsiteUrl,
            CompanyLocation = request.CompanyLocation,
            LogoUrl = request.LogoUrl
        };

        _db.EmployerProfiles.Add(profile);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Employer profile created for user {UserId}", userId);
        return MapToResponse(profile);
    }

    public async Task<EmployerProfileResponse> GetEmployerProfileAsync(string userId)
    {
        var profile = await _db.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new InvalidOperationException("Employer profile not found");

        return MapToResponse(profile);
    }

    public async Task<EmployerProfileResponse> UpdateEmployerProfileAsync(string userId, UpdateEmployerProfileRequest request)
    {
        var profile = await _db.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new InvalidOperationException("Employer profile not found");

        profile.CompanyName = request.CompanyName;
        profile.CompanyDescription = request.CompanyDescription;
        profile.Industry = request.Industry;
        profile.WebsiteUrl = request.WebsiteUrl;
        profile.CompanyLocation = request.CompanyLocation;
        profile.LogoUrl = request.LogoUrl;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Employer profile updated for user {UserId}", userId);
        return MapToResponse(profile);
    }

    public async Task<EmployerProfilePublicResponse> GetEmployerProfilePublicAsync(Guid employerId)
    {
        var profile = await _db.EmployerProfiles.FirstOrDefaultAsync(p => p.EmployerId == employerId)
            ?? throw new InvalidOperationException("Employer profile not found");

        return new EmployerProfilePublicResponse
        {
            EmployerId = profile.EmployerId,
            CompanyName = profile.CompanyName,
            CompanyDescription = profile.CompanyDescription,
            Industry = profile.Industry,
            WebsiteUrl = profile.WebsiteUrl,
            CompanyLocation = profile.CompanyLocation,
            LogoUrl = profile.LogoUrl
        };
    }

    private static CandidateProfileResponse MapToResponse(CandidateProfile p) => new()
    {
        CandidateId = p.CandidateId,
        UserId = p.UserId,
        Phone = p.Phone,
        Location = p.Location,
        ProfessionalSummary = p.ProfessionalSummary,
        YearsOfExperience = p.YearsOfExperience,
        HighestEducation = p.HighestEducation,
        LinkedinUrl = p.LinkedinUrl,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static EmployerProfileResponse MapToResponse(EmployerProfile p) => new()
    {
        EmployerId = p.EmployerId,
        UserId = p.UserId,
        CompanyName = p.CompanyName,
        CompanyDescription = p.CompanyDescription,
        Industry = p.Industry,
        WebsiteUrl = p.WebsiteUrl,
        CompanyLocation = p.CompanyLocation,
        LogoUrl = p.LogoUrl,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
