import { cn } from "@/lib/utils";

type Size = "sm" | "md" | "lg";

const sizeStyles: Record<Size, string> = {
  sm: "w-8 h-8 text-xs",
  md: "w-10 h-10 text-sm",
  lg: "w-14 h-14 text-xl",
};

interface AvatarProps {
  src?: string;
  fallback: string;
  size?: Size;
  className?: string;
}

export function Avatar({ src, fallback, size = "md", className }: AvatarProps) {
  if (src) {
    return (
      <img
        src={src}
        alt={fallback}
        className={cn("rounded-full object-cover border-2 border-primary-container/30", sizeStyles[size], className)}
      />
    );
  }

  return (
    <div
      className={cn(
        "rounded-full bg-surface-container-high flex items-center justify-center font-black text-primary",
        sizeStyles[size],
        className
      )}
    >
      {fallback.slice(0, 2).toUpperCase()}
    </div>
  );
}
