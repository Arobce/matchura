"use client";

import { use, useState, useMemo } from "react";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner, Button } from "@/components/ui";
import { KanbanBoard, ApplicantTable, ApplicantDetailPanel } from "@/components/features/applicant-review";
import { useApi } from "@/hooks/useApi";
import { api } from "@/lib/api";
import { useNotificationStore } from "@/stores";
import type { MatchListResponse, MatchScoreResponse, ApplicationListResponse } from "@/lib/types";

export default function ApplicantReviewPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: matchData, loading: matchLoading, refetch: refetchMatches } = useApi<MatchListResponse>(`/api/matching/job/${id}/candidates?pageSize=50`);
  const { data: appData, loading: appLoading, refetch: refetchApps } = useApi<ApplicationListResponse>(`/api/applications/job/${id}?pageSize=50`);
  const [view, setView] = useState<"board" | "table">("board");
  const [selected, setSelected] = useState<MatchScoreResponse | null>(null);
  const { addNotification } = useNotificationStore();

  const candidateToAppId = useMemo(() => {
    const map = new Map<string, string>();
    for (const app of appData?.items ?? []) map.set(app.candidateId, app.applicationId);
    return map;
  }, [appData]);

  const nameMap = useMemo(() => {
    const map = new Map<string, string>();
    for (const app of appData?.items ?? []) {
      if (app.candidateName) map.set(app.candidateId, app.candidateName);
    }
    return map;
  }, [appData]);

  const handleStatusChange = async (applicationId: string, status: string) => {
    try {
      await api.patch(`/api/applications/${applicationId}/status`, { status });
      addNotification({ type: "success", message: `Status updated to ${status}` });
      await Promise.all([refetchMatches(), refetchApps()]);
    } catch {
      addNotification({ type: "error", message: "Failed to update status" });
      throw new Error("Status update failed");
    }
  };

  const handleTableStatusChange = async (candidateId: string, status: string) => {
    const appId = candidateToAppId.get(candidateId);
    if (!appId) { addNotification({ type: "error", message: "Application not found" }); return; }
    await handleStatusChange(appId, status);
  };

  const loading = matchLoading || appLoading;

  return (
    <>
      <Navbar />
      <PageContainer>
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold tracking-tight text-on-surface">Pipeline</h1>
          <div className="flex gap-1 bg-surface-container-low rounded-lg p-1">
            <Button size="sm" variant={view === "board" ? "primary" : "ghost"} onClick={() => setView("board")}>
              Board
            </Button>
            <Button size="sm" variant={view === "table" ? "primary" : "ghost"} onClick={() => setView("table")}>
              Table
            </Button>
          </div>
        </div>

        {loading ? (
          <Spinner size="lg" />
        ) : view === "board" ? (
          <KanbanBoard
            candidates={matchData?.items ?? []}
            applications={appData?.items ?? []}
            jobId={id}
            onStatusChange={handleStatusChange}
          />
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
            <div className={selected ? "lg:col-span-7" : "lg:col-span-12"}>
              <ApplicantTable
                candidates={matchData?.items ?? []}
                nameMap={nameMap}
                onStatusChange={handleTableStatusChange}
                onSelect={setSelected}
              />
            </div>
            {selected && (
              <div className="lg:col-span-5">
                <ApplicantDetailPanel match={selected} nameMap={nameMap} />
              </div>
            )}
          </div>
        )}
      </PageContainer>
    </>
  );
}
