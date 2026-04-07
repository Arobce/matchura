import { cn } from "@/lib/utils";
import type { LucideIcon } from "lucide-react";
import Link from "next/link";

interface EmptyStateProps {
  icon?: LucideIcon;
  title: string;
  description?: string;
  action?: { label: string; href: string };
  className?: string;
}

export function EmptyState({ icon: Icon, title, description, action, className }: EmptyStateProps) {
  return (
    <div className={cn("text-center py-20", className)}>
      {Icon && <Icon className="h-12 w-12 text-outline-variant mx-auto mb-4" />}
      <p className="text-lg font-medium text-on-surface">{title}</p>
      {description && <p className="text-sm text-on-surface-variant mt-1">{description}</p>}
      {action && (
        <Link
          href={action.href}
          className="text-primary font-bold mt-3 inline-block hover:text-primary-dim transition-colors"
        >
          {action.label}
        </Link>
      )}
    </div>
  );
}
