using AIService.Domain.Enums;

namespace AIService.Domain.Entities;

public class CandidateSkill
{
    public Guid CandidateSkillId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string? SkillCategory { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Intermediate;
    public int? YearsUsed { get; set; }
    public string Source { get; set; } = "resume_parse";
}
