import { cn } from "@/lib/utils";

interface ProgressBarProps {
  value: number;
  max?: number;
  label?: string;
  showValue?: boolean;
  variant?: "primary" | "success" | "warning" | "danger" | "tertiary";
  className?: string;
}

const variantStyles = {
  primary: "bg-primary",
  success: "bg-success",
  warning: "bg-warning",
  danger: "bg-danger",
  tertiary: "bg-tertiary",
};

export function ProgressBar({
  value,
  max = 100,
  label,
  showValue = false,
  variant = "primary",
  className,
}: ProgressBarProps) {
  const pct = Math.min(100, Math.max(0, (value / max) * 100));

  return (
    <div className={cn("space-y-1", className)}>
      {(label || showValue) && (
        <div className="flex justify-between text-sm">
          {label && <span className="text-on-surface-variant font-medium">{label}</span>}
          {showValue && <span className="text-on-surface font-semibold">{Math.round(pct)}%</span>}
        </div>
      )}
      <div className="h-2 bg-surface-container-high rounded-full overflow-hidden">
        <div
          className={cn("h-full rounded-full transition-all duration-500", variantStyles[variant])}
          style={{ width: `${pct}%` }}
        />
      </div>
    </div>
  );
}
