import { formatSalary } from "@/lib/utils";
import { DollarSign } from "lucide-react";

interface SalaryDisplayProps {
  min?: number;
  max?: number;
  className?: string;
  showIcon?: boolean;
}

export function SalaryDisplay({ min, max, className, showIcon = true }: SalaryDisplayProps) {
  const text = formatSalary(min, max);
  if (!text) return null;

  return (
    <span className={`flex items-center gap-2 text-sm font-bold text-on-surface ${className ?? ""}`}>
      {showIcon && <DollarSign className="h-4 w-4 text-primary" />}
      {text}
    </span>
  );
}
