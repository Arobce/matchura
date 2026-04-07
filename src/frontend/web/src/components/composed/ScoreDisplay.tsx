import { cn } from "@/lib/utils";

type Size = "sm" | "md" | "lg";

const sizeStyles: Record<Size, string> = {
  sm: "w-10 h-10 text-sm",
  md: "w-14 h-14 text-xl",
  lg: "w-20 h-20 text-3xl",
};

interface ScoreDisplayProps {
  score: number;
  label?: string;
  size?: Size;
  className?: string;
}

function scoreColor(score: number): string {
  if (score >= 85) return "bg-tertiary-container text-on-tertiary-container";
  if (score >= 70) return "bg-tertiary-container/60 text-on-tertiary-container";
  if (score >= 50) return "bg-warning/20 text-warning";
  return "bg-error-container/20 text-error";
}

export function ScoreDisplay({ score, label, size = "md", className }: ScoreDisplayProps) {
  return (
    <div className={cn("flex items-center gap-3", className)}>
      <div className={cn("rounded-lg flex items-center justify-center font-black", sizeStyles[size], scoreColor(score))}>
        {score}%
      </div>
      {label && <span className="text-sm text-on-surface-variant">{label}</span>}
    </div>
  );
}
