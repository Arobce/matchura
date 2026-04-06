namespace JobService.Domain.Entities;

public class Skill
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? SkillCategory { get; set; }

    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}
