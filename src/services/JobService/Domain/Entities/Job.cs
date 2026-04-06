using JobService.Domain.Enums;

namespace JobService.Domain.Entities;

public class Job
{
    public Guid JobId { get; set; }
    public string EmployerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public int ExperienceRequired { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public JobStatus JobStatus { get; set; } = JobStatus.Draft;
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApplicationDeadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}
