"use client";

import { useEffect, useState } from "react";
import { Button, Spinner } from "@/components/ui";
import { ReadinessScore, MissingSkillsTable, RecommendedActionsPanel, StrengthsPanel } from "@/components/features/skill-gap";
import { api } from "@/lib/api";
import type { SkillGapReportResponse } from "@/lib/types";
import { RefreshCw, Search } from "lucide-react";
import Link from "next/link";

interface SkillGapSectionProps {
  jobId: string;
}

export function SkillGapSection({ jobId }: SkillGapSectionProps) {
  const [report, setReport] = useState<SkillGapReportResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [analyzing, setAnalyzing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function fetchExisting() {
      try {
        const data = await api.get<SkillGapReportResponse>(`/api/skillgap/candidate/me/job/${jobId}`);
        if (!cancelled) setReport(data);
      } catch {
        // 404 = no report yet, that's fine
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    fetchExisting();
    return () => { cancelled = true; };
  }, [jobId]);

  const handleAnalyze = async () => {
    setAnalyzing(true);
    setError(null);
    try {
      const result = await api.post<SkillGapReportResponse>("/api/skillgap/analyze", { jobId });
      setReport(result);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Analysis failed";
      if (message.toLowerCase().includes("resume")) {
        setError("resume");
      } else {
        setError(message);
      }
    } finally {
      setAnalyzing(false);
    }
  };

  if (loading) {
    return <Spinner size="md" />;
  }

  if (error === "resume") {
    return (
      <div className="text-center py-12 text-on-surface-variant">
        <p className="text-lg font-medium">Resume required</p>
        <p className="text-sm mt-1 mb-4">Upload your resume first so we can analyze your skill gaps.</p>
        <Link href="/resumes" className="text-primary font-medium hover:underline">
          Go to Resumes →
        </Link>
      </div>
    );
  }

  if (!report) {
    return (
      <div className="text-center py-12">
        <p className="text-on-surface-variant mb-4">
          Analyze how your skills align with this role and get a personalized development roadmap.
        </p>
        {error && <p className="text-error text-sm mb-4">{error}</p>}
        <Button onClick={handleAnalyze} loading={analyzing}>
          <Search className="h-4 w-4" />
          Analyze Skill Gaps
        </Button>
      </div>
    );
  }

  const analyzedDate = new Date(report.generatedAt).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <p className="text-xs text-on-surface-variant">Analyzed {analyzedDate}</p>
        <button
          onClick={handleAnalyze}
          disabled={analyzing}
          className="flex items-center gap-1.5 text-xs text-primary hover:underline disabled:opacity-50"
        >
          <RefreshCw className={`h-3 w-3 ${analyzing ? "animate-spin" : ""}`} />
          Re-analyze
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        <div className="lg:col-span-4">
          <ReadinessScore score={report.overallReadiness} summary={report.summary} />
        </div>
        <div className="lg:col-span-8">
          <MissingSkillsTable skills={report.missingSkills} />
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <RecommendedActionsPanel actions={report.recommendedActions} />
        <StrengthsPanel strengths={report.strengths} />
      </div>
    </div>
  );
}
