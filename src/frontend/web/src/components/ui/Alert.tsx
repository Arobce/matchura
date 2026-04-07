import { cn } from "@/lib/utils";
import { X } from "lucide-react";
import type { ReactNode } from "react";

type Variant = "error" | "success" | "warning" | "info";

const variantStyles: Record<Variant, string> = {
  error: "bg-error-container/20 text-error",
  success: "bg-green-100 text-green-700",
  warning: "bg-yellow-100 text-yellow-700",
  info: "bg-primary-container/10 text-primary",
};

interface AlertProps {
  variant?: Variant;
  children: ReactNode;
  className?: string;
  onDismiss?: () => void;
}

export function Alert({ variant = "error", children, className, onDismiss }: AlertProps) {
  return (
    <div className={cn("text-sm p-3 rounded-lg flex items-start gap-2", variantStyles[variant], className)}>
      <span className="flex-1">{children}</span>
      {onDismiss && (
        <button onClick={onDismiss} className="shrink-0 hover:opacity-70 transition-opacity">
          <X className="h-4 w-4" />
        </button>
      )}
    </div>
  );
}
