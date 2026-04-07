"use client";

import { use, useState, useMemo } from "react";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner } from "@/components/ui";
import { ApplicantTable, ApplicantDetailPanel } from "@/components/features/applicant-review";
import { useApi } from "@/hooks/useApi";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import type { MatchListResponse, MatchScoreResponse, ApplicationListResponse } from "@/lib/types";

export default function ApplicantReviewPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data, loading } = useApi<MatchListResponse>(`/api/matching/job/${id}/candidates?pageSize=50`);
  const { data: applications } = useApi<ApplicationListResponse>(`/api/applications/job/${id}?pageSize=50`);
  const [selected, setSelected] = useState<MatchScoreResponse | null>(null);
  const { addNotification } = useNotificationStore();

  const candidateToAppId = useMemo(() => {
    const map = new Map<string, string>();
    for (const app of applications?.items ?? []) {
      map.set(app.candidateId, app.applicationId);
    }
    return map;
  }, [applications]);

  const handleStatusChange = async (candidateId: string, status: string) => {
    const applicationId = candidateToAppId.get(candidateId);
    if (!applicationId) {
      addNotification({ type: "error", message: "Application not found for this candidate" });
      return;
    }
    try {
      await api.patch(`/api/applications/${applicationId}/status`, { status });
      addNotification({ type: "success", message: `Candidate ${status.toLowerCase()}` });
    } catch {
      addNotification({ type: "error", message: "Failed to update status" });
    }
  };

  return (
    <>
      <Navbar />
      <PageContainer>
        <h1 className="text-3xl font-bold tracking-tight text-on-surface mb-8">Applicant Review</h1>

        {loading ? (
          <Spinner size="lg" />
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
            <div className={selected ? "lg:col-span-7" : "lg:col-span-12"}>
              <ApplicantTable
                candidates={data?.items ?? []}
                onStatusChange={handleStatusChange}
                onSelect={setSelected}
              />
            </div>
            {selected && (
              <div className="lg:col-span-5">
                <ApplicantDetailPanel match={selected} />
              </div>
            )}
          </div>
        )}
      </PageContainer>
    </>
  );
}
