import { cn } from "@/lib/utils";

interface SkillBadgeProps {
  name: string;
  importanceLevel?: string;
  proficiencyLevel?: string;
  className?: string;
}

export function SkillBadge({ name, importanceLevel, proficiencyLevel, className }: SkillBadgeProps) {
  const style =
    importanceLevel === "Required"
      ? "bg-primary-container/10 text-primary border border-primary/20"
      : importanceLevel === "Preferred"
      ? "bg-tertiary-container/50 text-on-tertiary-container border border-tertiary/20"
      : "bg-surface-container text-on-surface-variant";

  return (
    <span className={cn("px-3 py-1 rounded-lg text-xs font-semibold inline-flex items-center gap-1", style, className)}>
      {name}
      {proficiencyLevel && <span className="opacity-60">({proficiencyLevel})</span>}
      {importanceLevel && !proficiencyLevel && (
        <span className="opacity-60">({importanceLevel})</span>
      )}
    </span>
  );
}
