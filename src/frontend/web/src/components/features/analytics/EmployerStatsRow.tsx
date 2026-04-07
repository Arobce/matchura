import { Card } from "@/components/ui";
import { Briefcase, Users, Brain, GitBranch } from "lucide-react";
import type { EmployerDashboardResponse } from "@/lib/types";

interface EmployerStatsRowProps {
  dashboard: EmployerDashboardResponse;
}

export function EmployerStatsRow({ dashboard }: EmployerStatsRowProps) {
  const stats = [
    { label: "Active Jobs", value: dashboard.totalActiveJobs, icon: Briefcase, color: "text-primary", bgColor: "bg-primary-container/10" },
    { label: "Total Applications", value: dashboard.totalApplications, icon: Users, color: "text-secondary", bgColor: "bg-secondary-container/30" },
    { label: "Avg. Match Score", value: `${dashboard.averageMatchScore}%`, icon: Brain, color: "text-tertiary", bgColor: "bg-tertiary-container/30" },
    {
      label: "Pipeline Stages",
      value: Object.values(dashboard.pipelineBreakdown).reduce((a, b) => a + b, 0),
      icon: GitBranch,
      color: "text-on-surface-variant",
      bgColor: "bg-surface-container-high",
    },
  ];

  return (
    <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
      {stats.map((stat) => (
        <Card key={stat.label} variant="lowest" padding="md" hover>
          <div className="flex justify-between items-start mb-4">
            <span className="text-on-surface-variant text-xs uppercase tracking-widest font-bold">{stat.label}</span>
            <div className={`${stat.bgColor} p-2 rounded-lg`}>
              <stat.icon className={`h-5 w-5 ${stat.color}`} />
            </div>
          </div>
          <div className="text-3xl font-black text-on-surface">{stat.value}</div>
        </Card>
      ))}
    </section>
  );
}
