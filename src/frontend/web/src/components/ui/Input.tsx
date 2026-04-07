import { cn } from "@/lib/utils";
import type { InputHTMLAttributes } from "react";
import type { LucideIcon } from "lucide-react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  icon?: LucideIcon;
}

export function Input({ label, error, icon: Icon, className, id, ...props }: InputProps) {
  const inputId = id || label?.toLowerCase().replace(/\s+/g, "-");

  return (
    <div className="space-y-1.5">
      {label && (
        <label
          htmlFor={inputId}
          className="text-[11px] uppercase tracking-widest font-bold text-on-surface-variant"
        >
          {label}
        </label>
      )}
      <div className="relative group">
        {Icon && (
          <Icon className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-outline" />
        )}
        <input
          id={inputId}
          className={cn(
            "w-full px-4 py-3 bg-transparent border border-outline-variant/30 rounded-lg text-on-surface placeholder:text-outline",
            "focus:ring-2 focus:ring-primary-fixed focus:border-primary outline-none transition-all duration-200",
            Icon && "pl-10",
            error && "border-error focus:ring-error-container",
            className
          )}
          {...props}
        />
      </div>
      {error && <p className="text-xs text-error font-medium">{error}</p>}
    </div>
  );
}
