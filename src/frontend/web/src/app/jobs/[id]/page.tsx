"use client";

import { use, useState } from "react";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner, Button } from "@/components/ui";
import { JobHeader, CandidateInsightsPanel, ApplyModal } from "@/components/features/jobs";
import { useApi } from "@/hooks/useApi";
import { useAuth } from "@/hooks/useAuth";
import { api } from "@/lib/api";
import type { Job, MatchScoreResponse } from "@/lib/types";
import { CheckCircle2 } from "lucide-react";

export default function JobDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { user, isAuthenticated } = useAuth();
  const { data: job, loading } = useApi<Job>(`/api/jobs/${id}`);
  const [applied, setApplied] = useState(false);
  const [showApplyModal, setShowApplyModal] = useState(false);
  const [matchScore, setMatchScore] = useState<MatchScoreResponse | null>(null);
  const [computing, setComputing] = useState(false);

  const computeMatch = async () => {
    setComputing(true);
    try {
      const result = await api.post<MatchScoreResponse>("/api/matching/compute", { jobId: id });
      setMatchScore(result);
    } catch {
      // handled by notification store
    } finally {
      setComputing(false);
    }
  };

  if (loading) {
    return (
      <>
        <Navbar />
        <Spinner size="lg" />
      </>
    );
  }

  if (!job) {
    return (
      <>
        <Navbar />
        <div className="text-center py-20 text-on-surface-variant">Job not found</div>
      </>
    );
  }

  const isCandidate = isAuthenticated && user?.role === "Candidate";

  const actions = isCandidate && (
    <>
      {!applied ? (
        <Button onClick={() => setShowApplyModal(true)}>Apply Now</Button>
      ) : (
        <span className="flex items-center gap-1 text-success font-medium">
          <CheckCircle2 className="h-5 w-5" /> Applied
        </span>
      )}
    </>
  );

  return (
    <>
      <Navbar />
      <PageContainer>
        <JobHeader job={job} actions={actions} />
        {isCandidate && (
          <CandidateInsightsPanel
            jobId={id}
            matchScore={matchScore}
            onComputeMatch={computeMatch}
            computing={computing}
          />
        )}
        <ApplyModal
          jobId={id}
          jobTitle={job.title}
          open={showApplyModal}
          onClose={() => setShowApplyModal(false)}
          onSuccess={() => setApplied(true)}
        />
      </PageContainer>
    </>
  );
}
