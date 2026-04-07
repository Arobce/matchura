import { cn } from "@/lib/utils";
import type { TextareaHTMLAttributes } from "react";

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
}

export function Textarea({ label, error, className, id, ...props }: TextareaProps) {
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
      <textarea
        id={inputId}
        className={cn(
          "w-full px-4 py-3 bg-transparent border border-outline-variant/30 rounded-lg text-on-surface placeholder:text-outline",
          "focus:ring-2 focus:ring-primary-fixed focus:border-primary outline-none transition-all duration-200 resize-y",
          error && "border-error focus:ring-error-container",
          className
        )}
        {...props}
      />
      {error && <p className="text-xs text-error font-medium">{error}</p>}
    </div>
  );
}
