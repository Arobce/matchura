import Link from "next/link";

interface SectionHeaderProps {
  title: string;
  subtitle?: string;
  action?: { label: string; href: string };
  className?: string;
}

export function SectionHeader({ title, subtitle, action, className }: SectionHeaderProps) {
  return (
    <div className={`flex flex-col md:flex-row md:items-end justify-between gap-4 ${className ?? ""}`}>
      <div>
        <h2 className="text-3xl md:text-4xl font-bold tracking-tight text-on-surface">{title}</h2>
        {subtitle && <p className="text-lg text-on-surface-variant mt-1">{subtitle}</p>}
      </div>
      {action && (
        <Link
          href={action.href}
          className="text-primary font-bold inline-flex items-center gap-2 hover:gap-3 transition-all text-sm"
        >
          {action.label} →
        </Link>
      )}
    </div>
  );
}
