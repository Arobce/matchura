"use client";

import { use, useState } from "react";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner } from "@/components/ui";
import { ApplicantTable, ApplicantDetailPanel } from "@/components/features/applicant-review";
import { useApi } from "@/hooks/useApi";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import type { MatchListResponse, MatchScoreResponse } from "@/lib/types";

export default function ApplicantReviewPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data, loading } = useApi<MatchListResponse>(`/api/matching/job/${id}/candidates?pageSize=50`);
  const [selected, setSelected] = useState<MatchScoreResponse | null>(null);
  const { addNotification } = useNotificationStore();

  const handleStatusChange = async (candidateId: string, status: string) => {
    try {
      await api.put(`/api/applications/${candidateId}/status`, { status });
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
