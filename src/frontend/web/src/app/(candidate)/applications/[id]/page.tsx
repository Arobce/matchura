"use client";

import { use, useState } from "react";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner, Button, Alert } from "@/components/ui";
import { StatusBadge, ScoreBreakdown } from "@/components/composed";
import { JobHeader } from "@/components/features/jobs";
import { useApi } from "@/hooks/useApi";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import { formatDate } from "@/lib/utils";
import { ArrowLeft, Download, FileText, Sparkles } from "lucide-react";
import Link from "next/link";
import type { Application, Job, MatchScoreResponse } from "@/lib/types";

export default function ApplicationDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: application, loading, refetch } = useApi<Application>(`/api/applications/${id}`);
  const { data: job } = useApi<Job>(application ? `/api/jobs/${application.jobId}` : null);
  const { addNotification } = useNotificationStore();

  const [matchScore, setMatchScore] = useState<MatchScoreResponse | null>(null);
  const [computing, setComputing] = useState(false);
  const [withdrawing, setWithdrawing] = useState(false);

  const computeMatch = async () => {
    if (!application) return;
    setComputing(true);
    try {
      const result = await api.post<MatchScoreResponse>("/api/matching/compute", { jobId: application.jobId });
      setMatchScore(result);
    } catch {
      addNotification({ type: "error", message: "Failed to compute match score" });
    } finally {
      setComputing(false);
    }
  };

  const handleDownloadResume = async () => {
    if (!application?.resumeUrl) return;
    try {
      const result = await api.get<{ downloadUrl: string }>(`/api/resumes/${application.resumeUrl}/download`);
      window.open(result.downloadUrl, "_blank");
    } catch {
      addNotification({ type: "error", message: "Failed to download resume" });
    }
  };

  const handleDownloadCoverLetter = async () => {
    if (!application?.coverLetterUrl) return;
    try {
      const result = await api.get<{ downloadUrl: string }>(`/api/documents/download?key=${encodeURIComponent(application.coverLetterUrl)}`);
      window.open(result.downloadUrl, "_blank");
    } catch {
      addNotification({ type: "error", message: "Failed to download cover letter" });
    }
  };

  const handleWithdraw = async () => {
    if (!application) return;
    setWithdrawing(true);
    try {
      await api.put(`/api/applications/${id}/withdraw`);
      addNotification({ type: "success", message: "Application withdrawn" });
      await refetch();
    } catch {
      addNotification({ type: "error", message: "Failed to withdraw application" });
    } finally {
      setWithdrawing(false);
    }
  };

  if (loading || !application) {
    return (
      <>
        <Navbar />
        <PageContainer><Spinner size="lg" /></PageContainer>
      </>
    );
  }

  const canWithdraw = ["Submitted", "Reviewed"].includes(application.status);

  return (
    <>
      <Navbar />
      <PageContainer className="space-y-6">
        {/* Back link */}
        <Link
          href="/applications"
          className="inline-flex items-center gap-2 text-sm text-on-surface-variant hover:text-primary transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Applications
        </Link>

        {/* Status header */}
        <div className="bg-surface-container-lowest rounded-xl editorial-shadow p-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div className="flex items-center gap-3 mb-2">
              <h1 className="text-2xl font-bold text-on-surface">Application</h1>
              <StatusBadge status={application.status} />
            </div>
            <div className="flex gap-6 text-sm text-on-surface-variant">
              <span>Applied {formatDate(application.appliedAt)}</span>
              {application.updatedAt !== application.appliedAt && (
                <span>Updated {formatDate(application.updatedAt)}</span>
              )}
            </div>
          </div>
          <div className="flex gap-2">
            {canWithdraw && (
              <Button variant="danger" size="sm" onClick={handleWithdraw} loading={withdrawing}>
                Withdraw
              </Button>
            )}
          </div>
        </div>

        {/* Job details */}
        {job && <JobHeader job={job} />}

        {/* Resume & Cover Letter */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Resume */}
          <div className="bg-surface-container-lowest rounded-xl editorial-shadow p-6 space-y-4">
            <h3 className="text-lg font-bold text-on-surface">Resume</h3>
            {application.resumeUrl ? (
              <button
                onClick={handleDownloadResume}
                className="flex items-center gap-3 w-full p-4 rounded-lg border border-outline-variant/20 hover:border-primary/30 hover:bg-primary-container/5 transition-all text-left"
              >
                <FileText className="h-8 w-8 text-primary shrink-0" />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-on-surface">Attached Resume</p>
                  <p className="text-xs text-on-surface-variant">Click to download PDF</p>
                </div>
                <Download className="h-5 w-5 text-on-surface-variant" />
              </button>
            ) : (
              <p className="text-sm text-on-surface-variant">No resume attached</p>
            )}
          </div>

          {/* Cover Letter */}
          <div className="bg-surface-container-lowest rounded-xl editorial-shadow p-6 space-y-4">
            <h3 className="text-lg font-bold text-on-surface">Cover Letter</h3>
            {application.coverLetterUrl && (
              <button
                onClick={handleDownloadCoverLetter}
                className="flex items-center gap-3 w-full p-4 rounded-lg border border-outline-variant/20 hover:border-primary/30 hover:bg-primary-container/5 transition-all text-left"
              >
                <FileText className="h-8 w-8 text-primary shrink-0" />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-on-surface">Cover Letter PDF</p>
                  <p className="text-xs text-on-surface-variant">Click to download PDF</p>
                </div>
                <Download className="h-5 w-5 text-on-surface-variant" />
              </button>
            )}
            {application.coverLetter && (
              <div className="bg-surface-container-low rounded-lg p-4 text-sm text-on-surface-variant leading-relaxed whitespace-pre-wrap">
                {application.coverLetter}
              </div>
            )}
            {!application.coverLetterUrl && !application.coverLetter && (
              <p className="text-sm text-on-surface-variant">No cover letter provided</p>
            )}
          </div>
        </div>

        {/* Match Score */}
        <div className="bg-surface-container-lowest rounded-xl editorial-shadow p-6 space-y-6">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-bold text-on-surface">Match Analysis</h3>
            {!matchScore && (
              <Button variant="outline" size="sm" onClick={computeMatch} disabled={computing}>
                <Sparkles className="h-4 w-4" />
                {computing ? "Computing..." : "Compute Match"}
              </Button>
            )}
          </div>

          {matchScore ? (
            <div className="space-y-6">
              <div className="flex items-center gap-4">
                <div className="h-16 w-16 rounded-full bg-primary/10 flex items-center justify-center">
                  <span className="text-xl font-bold text-primary">{matchScore.overallScore}%</span>
                </div>
                <div>
                  <p className="font-semibold text-on-surface">Overall Match Score</p>
                  <p className="text-sm text-on-surface-variant">Based on your resume and job requirements</p>
                </div>
              </div>

              <ScoreBreakdown
                skillScore={matchScore.skillScore}
                experienceScore={matchScore.experienceScore}
                educationScore={matchScore.educationScore}
              />

              {matchScore.explanation && (
                <div>
                  <h4 className="text-sm font-bold text-on-surface mb-2">Analysis</h4>
                  <p className="text-sm text-on-surface-variant leading-relaxed">{matchScore.explanation}</p>
                </div>
              )}

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {matchScore.strengths.length > 0 && (
                  <div>
                    <h4 className="text-sm font-bold text-on-surface mb-3">Strengths</h4>
                    <ul className="space-y-2">
                      {matchScore.strengths.map((s, i) => (
                        <li key={i} className="flex items-start gap-2 text-sm text-on-surface-variant">
                          <span className="text-green-500 mt-0.5">&#10003;</span>
                          {s}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
                {matchScore.gaps.length > 0 && (
                  <div>
                    <h4 className="text-sm font-bold text-on-surface mb-3">Gaps</h4>
                    <ul className="space-y-2">
                      {matchScore.gaps.map((g, i) => (
                        <li key={i} className="flex items-start gap-2 text-sm text-on-surface-variant">
                          <span className="text-warning mt-0.5">!</span>
                          {g}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <p className="text-sm text-on-surface-variant">
              Click &quot;Compute Match&quot; to see how well you match this job.
            </p>
          )}
        </div>
      </PageContainer>
    </>
  );
}
