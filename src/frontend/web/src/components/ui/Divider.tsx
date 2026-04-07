import { cn } from "@/lib/utils";

interface DividerProps {
  label?: string;
  className?: string;
}

export function Divider({ label, className }: DividerProps) {
  if (label) {
    return (
      <div className={cn("relative my-8", className)}>
        <div className="absolute inset-0 flex items-center">
          <div className="w-full border-t border-outline-variant/15" />
        </div>
        <div className="relative flex justify-center text-xs">
          <span className="px-3 bg-surface-container-lowest text-outline font-medium">{label}</span>
        </div>
      </div>
    );
  }

  return <hr className={cn("border-t border-outline-variant/15", className)} />;
}
