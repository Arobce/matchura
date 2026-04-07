import { StatCard } from "@/components/composed";

interface StatItem {
  label: string;
  value: string | number;
  borderColor?: string;
  href?: string;
}

interface DashboardStatsProps {
  stats: StatItem[];
}

export function DashboardStats({ stats }: DashboardStatsProps) {
  return (
    <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
      {stats.map((stat) => (
        <StatCard
          key={stat.label}
          label={stat.label}
          value={stat.value}
          borderColor={stat.borderColor}
          href={stat.href}
        />
      ))}
    </section>
  );
}
