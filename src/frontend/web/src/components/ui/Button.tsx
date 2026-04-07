import { cn } from "@/lib/utils";
import { Loader2 } from "lucide-react";
import Link from "next/link";
import type { ButtonHTMLAttributes, ReactNode } from "react";

type Variant = "primary" | "secondary" | "outline" | "ghost" | "danger";
type Size = "sm" | "md" | "lg";

const variantStyles: Record<Variant, string> = {
  primary:
    "bg-gradient-to-br from-primary to-primary-container text-on-primary font-semibold shadow-sm hover:shadow-md active:scale-[0.98]",
  secondary:
    "bg-secondary-container text-on-secondary-container hover:bg-secondary-fixed-dim",
  outline:
    "bg-surface-container-lowest text-primary border border-primary/20 hover:bg-surface-container-low",
  ghost:
    "text-on-surface-variant hover:bg-surface-container-low",
  danger:
    "bg-error text-on-error hover:bg-error-dim",
};

const sizeStyles: Record<Size, string> = {
  sm: "px-3 py-1.5 text-sm rounded-lg",
  md: "px-5 py-2.5 rounded-lg",
  lg: "px-10 py-4 text-lg rounded-lg",
};

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  loading?: boolean;
  href?: string;
  fullWidth?: boolean;
  children: ReactNode;
}

export function Button({
  variant = "primary",
  size = "md",
  loading = false,
  href,
  fullWidth = false,
  children,
  className,
  disabled,
  ...props
}: ButtonProps) {
  const classes = cn(
    "inline-flex items-center justify-center gap-2 font-medium transition-all duration-200",
    variantStyles[variant],
    sizeStyles[size],
    fullWidth && "w-full",
    (disabled || loading) && "opacity-50 pointer-events-none",
    className
  );

  if (href) {
    return (
      <Link href={href} className={classes}>
        {children}
      </Link>
    );
  }

  return (
    <button className={classes} disabled={disabled || loading} {...props}>
      {loading && <Loader2 className="h-4 w-4 animate-spin" />}
      {children}
    </button>
  );
}
