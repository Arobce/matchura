import { cn } from "@/lib/utils";
import type { LucideIcon } from "lucide-react";
import Link from "next/link";

interface StatCardProps {
  icon?: LucideIcon;
  label: string;
  value: string | number;
  borderColor?: string;
  href?: string;
  className?: string;
}

export function StatCard({ icon: Icon, label, value, borderColor = "border-primary", href, className }: StatCardProps) {
  const content = (
    <div className={cn("bg-surface-container-low p-6 rounded-xl space-y-2 border-b-4", borderColor, className)}>
      {Icon && <Icon className="h-5 w-5 text-on-surface-variant" />}
      <p className="text-on-surface-variant text-sm font-medium uppercase tracking-wider">{label}</p>
      <p className="text-4xl font-bold text-on-surface">{value}</p>
    </div>
  );

  if (href) {
    return (
      <Link href={href} className="block hover:opacity-90 transition-opacity">
        {content}
      </Link>
    );
  }

  return content;
}
