namespace JobService.Application.DTOs;

public class CreateSkillRequest
{
    public string SkillName { get; set; } = string.Empty;
    public string? SkillCategory { get; set; }
}

public class SkillResponse
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? SkillCategory { get; set; }
}
