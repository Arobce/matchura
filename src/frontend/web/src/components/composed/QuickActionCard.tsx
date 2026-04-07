import type { LucideIcon } from "lucide-react";
import Link from "next/link";

interface QuickActionCardProps {
  icon: LucideIcon;
  label: string;
  description?: string;
  href: string;
}

export function QuickActionCard({ icon: Icon, label, description, href }: QuickActionCardProps) {
  return (
    <Link
      href={href}
      className="group bg-surface-container-lowest p-8 rounded-2xl editorial-shadow flex flex-col items-center text-center gap-4 transition-all hover:bg-primary-container/5 active:scale-95"
    >
      <div className="w-16 h-16 rounded-full signature-gradient text-white flex items-center justify-center">
        <Icon className="h-7 w-7" />
      </div>
      <div>
        <p className="text-xl font-bold text-on-surface">{label}</p>
        {description && <p className="text-on-surface-variant text-sm mt-1">{description}</p>}
      </div>
    </Link>
  );
}
