using JobService.Domain.Enums;

namespace JobService.Domain.Entities;

public class JobSkill
{
    public Guid JobSkillId { get; set; }
    public Guid JobId { get; set; }
    public Guid SkillId { get; set; }
    public ImportanceLevel ImportanceLevel { get; set; } = ImportanceLevel.Required;

    public Job Job { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
