import { SkillBadge } from "./SkillBadge";

interface Skill {
  name: string;
  importanceLevel?: string;
  proficiencyLevel?: string;
  skillId?: string;
}

interface SkillBadgeListProps {
  skills: Skill[];
  max?: number;
  className?: string;
}

export function SkillBadgeList({ skills, max, className }: SkillBadgeListProps) {
  const visible = max ? skills.slice(0, max) : skills;
  const remaining = max ? skills.length - max : 0;

  return (
    <div className={`flex flex-wrap gap-2 ${className ?? ""}`}>
      {visible.map((skill, i) => (
        <SkillBadge
          key={skill.skillId ?? i}
          name={skill.name}
          importanceLevel={skill.importanceLevel}
          proficiencyLevel={skill.proficiencyLevel}
        />
      ))}
      {remaining > 0 && (
        <span className="px-3 py-1 rounded-lg bg-surface-container text-on-surface-variant text-xs font-semibold">
          +{remaining}
        </span>
      )}
    </div>
  );
}
