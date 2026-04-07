interface ScoreBreakdownProps {
  skillScore: number;
  experienceScore: number;
  educationScore: number;
  className?: string;
}

export function ScoreBreakdown({ skillScore, experienceScore, educationScore, className }: ScoreBreakdownProps) {
  const items = [
    { label: "Skills", value: skillScore },
    { label: "Experience", value: experienceScore },
    { label: "Education", value: educationScore },
  ];

  return (
    <div className={`grid grid-cols-3 gap-4 text-sm ${className ?? ""}`}>
      {items.map((item) => (
        <div key={item.label}>
          <span className="text-on-surface-variant">{item.label}</span>
          <span className="block font-semibold text-on-surface">{item.value}%</span>
        </div>
      ))}
    </div>
  );
}
