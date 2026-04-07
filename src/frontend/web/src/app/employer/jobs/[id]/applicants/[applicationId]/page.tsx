"use client";

import { use, useMemo } from "react";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner } from "@/components/ui";
import { ApplicantHeader, MatchAnalysisSection, ApplicationDetailsSection, EmployerNotesSection } from "@/components/features/applicant-review";
import { useApi } from "@/hooks/useApi";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import type { Application, CandidateProfile, MatchListResponse } from "@/lib/types";

export default function ApplicantDetailPage({ params }: { params: Promise<{ id: string; applicationId: string }> }) {
  const { id: jobId, applicationId } = use(params);
  const { data: application, loading, refetch } = useApi<Application>(`/api/applications/${applicationId}`);
  const { data: profile } = useApi<CandidateProfile>(application ? `/api/profiles/candidate/${application.candidateId}` : null);
  const { data: matches } = useApi<MatchListResponse>(`/api/matching/job/${jobId}/candidates?pageSize=50`);
  const { addNotification } = useNotificationStore();

  const match = useMemo(() => {
    if (!matches || !application) return null;
    return matches.items.find((m) => m.candidateId === application.candidateId) ?? null;
  }, [matches, application]);

  const handleStatusChange = async (status: string) => {
    try {
      await api.patch(`/api/applications/${applicationId}/status`, { status });
      addNotification({ type: "success", message: `Status updated to ${status}` });
      await refetch();
    } catch {
      addNotification({ type: "error", message: "Failed to update status" });
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

  return (
    <>
      <Navbar />
      <PageContainer className="space-y-6">
        <ApplicantHeader
          application={application}
          profile={profile}
          score={match?.overallScore ?? null}
          jobId={jobId}
          onStatusChange={handleStatusChange}
        />
        {match && <MatchAnalysisSection match={match} />}
        <ApplicationDetailsSection application={application} />
        <EmployerNotesSection applicationId={applicationId} initialNotes={application.employerNotes ?? ""} />
      </PageContainer>
    </>
  );
}
