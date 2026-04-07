"use client";

import { useState } from "react";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Button, Spinner, Select } from "@/components/ui";
import { ReadinessScore, MissingSkillsTable, RecommendedActionsPanel, StrengthsPanel } from "@/components/features/skill-gap";
import { useApi } from "@/hooks/useApi";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import type { SkillGapReportResponse, JobListResponse } from "@/lib/types";

export default function SkillGapPage() {
  const { data: jobs } = useApi<JobListResponse>("/api/jobs?pageSize=50");
  const { data: reports, loading, refetch } = useApi<SkillGapReportResponse[]>("/api/skillgap/candidate/me/reports");
  const [selectedJobId, setSelectedJobId] = useState("");
  const [analyzing, setAnalyzing] = useState(false);
  const { addNotification } = useNotificationStore();

  const handleAnalyze = async () => {
    if (!selectedJobId) return;
    setAnalyzing(true);
    try {
      await api.post("/api/skillgap/analyze", { jobId: selectedJobId });
      addNotification({ type: "success", message: "Skill gap analysis complete!" });
      await refetch();
    } catch (err) {
      addNotification({ type: "error", message: err instanceof Error ? err.message : "Analysis failed" });
    } finally {
      setAnalyzing(false);
    }
  };

  const jobOptions = (jobs?.items ?? []).map((j) => ({
    value: j.jobId,
    label: j.title,
  }));

  const latestReport = reports?.[0];

  return (
    <>
      <Navbar />
      <PageContainer maxWidth="lg">
        <h1 className="text-3xl font-bold tracking-tight text-on-surface mb-8">Skill Gap Analysis</h1>

        {/* Analysis Controls */}
        <div className="bg-surface-container-low rounded-xl p-8 mb-10">
          <div className="flex flex-col md:flex-row gap-4 items-end">
            <div className="flex-1 w-full">
              <Select
                label="Select Target Position"
                options={jobOptions}
                placeholder="Choose a job to analyze..."
                value={selectedJobId}
                onChange={(e) => setSelectedJobId(e.target.value)}
              />
            </div>
            <Button onClick={handleAnalyze} loading={analyzing} disabled={!selectedJobId} className="w-full md:w-auto">
              Analyze
            </Button>
          </div>
        </div>

        {loading ? (
          <Spinner size="lg" />
        ) : latestReport ? (
          <div className="space-y-8">
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
              <div className="lg:col-span-4">
                <ReadinessScore score={latestReport.overallReadiness} summary={latestReport.summary} />
              </div>
              <div className="lg:col-span-8">
                <MissingSkillsTable skills={latestReport.missingSkills} />
              </div>
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
              <RecommendedActionsPanel actions={latestReport.recommendedActions} />
              <StrengthsPanel strengths={latestReport.strengths} />
            </div>
          </div>
        ) : (
          <div className="text-center py-20 text-on-surface-variant">
            <p className="text-lg font-medium">No skill gap reports yet</p>
            <p className="text-sm mt-1">Select a job position above and run an analysis to get started.</p>
          </div>
        )}
      </PageContainer>
    </>
  );
}
