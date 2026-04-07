import { cn } from "@/lib/utils";
import { Loader2 } from "lucide-react";

type Size = "sm" | "md" | "lg";

const sizeStyles: Record<Size, string> = {
  sm: "h-4 w-4",
  md: "h-6 w-6",
  lg: "h-8 w-8",
};

interface SpinnerProps {
  size?: Size;
  className?: string;
}

export function Spinner({ size = "md", className }: SpinnerProps) {
  return (
    <div className="flex justify-center py-10">
      <Loader2 className={cn("animate-spin text-primary", sizeStyles[size], className)} />
    </div>
  );
}
