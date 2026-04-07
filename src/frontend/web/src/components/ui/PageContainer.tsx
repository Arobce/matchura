import { cn } from "@/lib/utils";
import type { ReactNode } from "react";

interface PageContainerProps {
  children: ReactNode;
  className?: string;
}

export function PageContainer({ children, className }: PageContainerProps) {
  return (
    <main className={cn("px-8 sm:px-12 lg:px-16 py-10 max-w-screen-2xl mx-auto", className)}>
      {children}
    </main>
  );
}
