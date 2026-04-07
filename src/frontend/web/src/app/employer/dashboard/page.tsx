"use client";

import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner } from "@/components/ui";
import { EmployerStatsRow, PipelineChart, SkillDemandChart } from "@/components/features/analytics";
import { useApi } from "@/hooks/useApi";
import type { EmployerDashboardResponse } from "@/lib/types";

export default function EmployerDashboard() {
  const { data: dashboard, loading } = useApi<EmployerDashboardResponse>("/api/analytics/employer/dashboard");

  return (
    <>
      <Navbar />
      <PageContainer>
        <header className="mb-10">
          <h1 className="text-4xl font-extrabold tracking-tight text-on-surface mb-2">Employer Dashboard</h1>
          <p className="text-on-surface-variant font-medium">Welcome back. Here&apos;s your workspace overview.</p>
        </header>

        {loading || !dashboard ? (
          <Spinner size="lg" />
        ) : (
          <div className="space-y-10">
            <EmployerStatsRow dashboard={dashboard} />
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
              <PipelineChart breakdown={dashboard.pipelineBreakdown} />
              <SkillDemandChart skills={dashboard.topSkillsInDemand} />
            </div>
          </div>
        )}
      </PageContainer>
    </>
  );
}
