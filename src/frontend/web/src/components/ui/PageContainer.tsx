import { cn } from "@/lib/utils";
import type { ReactNode } from "react";

interface PageContainerProps {
  children: ReactNode;
  className?: string;
}

export function PageContainer({ children, className }: PageContainerProps) {
  return (
    <main className="w-full">
      <div className={cn("max-w-screen-2xl mx-auto px-6 py-10", className)}>
        {children}
      </div>
    </main>
  );
}
