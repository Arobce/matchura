import { cn } from "@/lib/utils";
import type { ReactNode } from "react";

type Padding = "sm" | "md" | "lg";

const paddingStyles: Record<Padding, string> = {
  sm: "p-4",
  md: "p-6",
  lg: "p-8",
};

interface CardProps {
  children: ReactNode;
  className?: string;
  padding?: Padding;
  hover?: boolean;
  variant?: "lowest" | "low" | "default" | "high";
}

const variantStyles = {
  lowest: "bg-surface-container-lowest",
  low: "bg-surface-container-low",
  default: "bg-surface-container",
  high: "bg-surface-container-high",
};

export function Card({
  children,
  className,
  padding = "md",
  hover = false,
  variant = "lowest",
}: CardProps) {
  return (
    <div
      className={cn(
        "rounded-xl editorial-shadow",
        variantStyles[variant],
        paddingStyles[padding],
        hover && "hover:-translate-y-1 hover:shadow-lg transition-all duration-300",
        className
      )}
    >
      {children}
    </div>
  );
}
