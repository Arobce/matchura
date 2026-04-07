import { cn } from "@/lib/utils";
import type { ReactNode } from "react";

type MaxWidth = "sm" | "md" | "lg" | "xl" | "2xl";

const maxWidthStyles: Record<MaxWidth, string> = {
  sm: "max-w-2xl",
  md: "max-w-4xl",
  lg: "max-w-6xl",
  xl: "max-w-7xl",
  "2xl": "max-w-[1400px]",
};

interface PageContainerProps {
  children: ReactNode;
  maxWidth?: MaxWidth;
  className?: string;
}

export function PageContainer({ children, maxWidth = "2xl", className }: PageContainerProps) {
  return (
    <main className={cn("mx-auto px-6 sm:px-8 py-10", maxWidthStyles[maxWidth], className)}>
      {children}
    </main>
  );
}
