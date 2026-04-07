import { ProgressBar } from "@/components/ui";
import type { MissingSkillEntry } from "@/lib/types";

interface MissingSkillsTableProps {
  skills: MissingSkillEntry[];
}

const severityVariant = (severity: number) => {
  if (severity >= 7) return "danger";
  if (severity >= 4) return "warning";
  return "primary";
};

export function MissingSkillsTable({ skills }: MissingSkillsTableProps) {
  if (skills.length === 0) return null;

  return (
    <div className="bg-surface-container-lowest rounded-2xl editorial-shadow overflow-hidden">
      <div className="p-8 pb-4">
        <h3 className="text-sm font-bold text-on-surface-variant uppercase tracking-widest">Identified Skill Gaps</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <thead>
            <tr className="bg-surface-container-low/50">
              <th className="px-8 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Skill Name</th>
              <th className="px-6 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Importance</th>
              <th className="px-6 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Current / Req</th>
              <th className="px-8 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Gap Severity</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/5">
            {skills.map((skill, i) => (
              <tr key={i} className="hover:bg-surface-container-low/30 transition-colors">
                <td className="px-8 py-5">
                  <span className="text-sm font-semibold text-on-surface">{skill.skillName}</span>
                </td>
                <td className="px-6 py-5">
                  <span className={`px-3 py-1 text-[10px] font-black uppercase tracking-tighter rounded-full ${
                    skill.importance === "Required"
                      ? "bg-error/10 text-error border border-error/20"
                      : skill.importance === "Preferred"
                      ? "bg-tertiary-container text-on-tertiary-container"
                      : "bg-surface-container-high text-on-secondary-container"
                  }`}>
                    {skill.importance}
                  </span>
                </td>
                <td className="px-6 py-5">
                  <span className="text-sm font-medium text-on-surface-variant">
                    {skill.currentLevel ?? "0"} / <span className="text-primary font-bold">{skill.requiredLevel}</span>
                  </span>
                </td>
                <td className="px-8 py-5">
                  <ProgressBar
                    value={skill.gapSeverity}
                    max={10}
                    variant={severityVariant(skill.gapSeverity)}
                    className="max-w-[160px]"
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
