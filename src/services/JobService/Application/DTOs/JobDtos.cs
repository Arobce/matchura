using JobService.Domain.Enums;

namespace JobService.Application.DTOs;

public class CreateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public int ExperienceRequired { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTime? ApplicationDeadline { get; set; }
    public List<JobSkillInput> Skills { get; set; } = new();
}

public class UpdateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public int ExperienceRequired { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTime? ApplicationDeadline { get; set; }
    public List<JobSkillInput> Skills { get; set; } = new();
}

public class JobSkillInput
{
    public Guid SkillId { get; set; }
    public ImportanceLevel ImportanceLevel { get; set; } = ImportanceLevel.Required;
}

public class UpdateJobStatusRequest
{
    public JobStatus Status { get; set; }
}

public class JobResponse
{
    public Guid JobId { get; set; }
    public string EmployerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public int ExperienceRequired { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string JobStatus { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
    public DateTime? ApplicationDeadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<JobSkillResponse> Skills { get; set; } = new();
}

public class JobSkillResponse
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? SkillCategory { get; set; }
    public string ImportanceLevel { get; set; } = string.Empty;
}

public class JobListResponse
{
    public List<JobResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class JobQueryParams
{
    public string? Search { get; set; }
    public string? Location { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? Skills { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
}
