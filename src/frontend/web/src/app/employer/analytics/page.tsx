"use client";

import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner } from "@/components/ui";
import { TrendChart, SkillDemandChart } from "@/components/features/analytics";
import { useApi } from "@/hooks/useApi";
import type { TrendDataResponse } from "@/lib/types";

export default function AnalyticsPage() {
  const { data: trends, loading } = useApi<TrendDataResponse>("/api/analytics/employer/trends");

  return (
    <>
      <Navbar />
      <PageContainer>
        <h1 className="text-3xl font-bold tracking-tight text-on-surface mb-8">Analytics</h1>

        {loading || !trends ? (
          <Spinner size="lg" />
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <TrendChart data={trends.applicationsPerWeek} label="Applications per Week" />
            <TrendChart data={trends.averageScorePerWeek} label="Average Score per Week" color="#6f557d" />
            <div className="lg:col-span-2">
              <SkillDemandChart skills={trends.mostRequestedSkills} />
            </div>
          </div>
        )}
      </PageContainer>
    </>
  );
}
