import { cn } from "@/lib/utils";
import type { SelectHTMLAttributes } from "react";

interface SelectOption {
  value: string;
  label: string;
}

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  options: SelectOption[];
  placeholder?: string;
}

export function Select({ label, error, options, placeholder, className, id, ...props }: SelectProps) {
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
      <select
        id={inputId}
        className={cn(
          "w-full px-4 py-3 bg-transparent border border-outline-variant/30 rounded-lg text-on-surface",
          "focus:ring-2 focus:ring-primary-fixed focus:border-primary outline-none transition-all duration-200",
          error && "border-error focus:ring-error-container",
          className
        )}
        {...props}
      >
        {placeholder && <option value="">{placeholder}</option>}
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {error && <p className="text-xs text-error font-medium">{error}</p>}
    </div>
  );
}
