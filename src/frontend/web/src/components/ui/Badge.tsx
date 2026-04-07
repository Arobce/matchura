import { cn } from "@/lib/utils";
import type { ReactNode } from "react";

type Variant = "primary" | "success" | "warning" | "danger" | "muted" | "accent" | "tertiary";
type Size = "sm" | "md";

const variantStyles: Record<Variant, string> = {
  primary: "bg-primary-container/10 text-primary",
  success: "bg-green-100 text-green-700",
  warning: "bg-yellow-100 text-yellow-700",
  danger: "bg-error-container/20 text-error",
  muted: "bg-surface-container text-on-surface-variant",
  accent: "bg-cyan-100 text-cyan-700",
  tertiary: "bg-tertiary-container text-on-tertiary-container",
};

const sizeStyles: Record<Size, string> = {
  sm: "px-2 py-0.5 text-xs",
  md: "px-3 py-1 text-xs",
};

interface BadgeProps {
  variant?: Variant;
  size?: Size;
  className?: string;
  children: ReactNode;
}

export function Badge({ variant = "primary", size = "md", className, children }: BadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center font-bold rounded-full uppercase tracking-tighter",
        variantStyles[variant],
        sizeStyles[size],
        className
      )}
    >
      {children}
    </span>
  );
}
