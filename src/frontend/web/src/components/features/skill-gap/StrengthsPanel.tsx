import { CheckCircle2 } from "lucide-react";

interface StrengthsPanelProps {
  strengths: string[];
}

export function StrengthsPanel({ strengths }: StrengthsPanelProps) {
  if (strengths.length === 0) return null;

  return (
    <div className="bg-surface-container-low/40 p-8 rounded-2xl border border-outline-variant/10">
      <h3 className="text-sm font-bold text-on-surface-variant uppercase tracking-widest mb-6">Core Strengths</h3>
      <div className="grid grid-cols-1 gap-4">
        {strengths.map((strength, i) => (
          <div key={i} className="flex items-center gap-4 p-4 bg-surface-container-lowest rounded-2xl editorial-shadow border-l-4 border-primary">
            <CheckCircle2 className="h-5 w-5 text-primary shrink-0" />
            <p className="text-sm font-medium text-on-surface">{strength}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
