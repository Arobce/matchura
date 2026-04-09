"use client";

import { useState, useMemo, useCallback } from "react";
import { DndContext, DragOverlay, PointerSensor, useSensor, useSensors } from "@dnd-kit/core";
import type { DragEndEvent, DragStartEvent } from "@dnd-kit/core";
import { KanbanColumn } from "./KanbanColumn";
import { KanbanCard } from "./KanbanCard";
import type { KanbanCardData } from "./KanbanCard";
import type { MatchScoreResponse, Application, ApplicationStatus } from "@/lib/types";

const PIPELINE_COLUMNS: ApplicationStatus[] = ["Submitted", "Reviewed", "Shortlisted", "Accepted", "Rejected"];

interface KanbanBoardProps {
  candidates: MatchScoreResponse[];
  applications: Application[];
  jobId: string;
  onStatusChange: (applicationId: string, status: string) => Promise<void>;
}

export function KanbanBoard({ candidates, applications, jobId, onStatusChange }: KanbanBoardProps) {
  const [localApps, setLocalApps] = useState<Application[]>(applications);
  const [activeId, setActiveId] = useState<string | null>(null);

  // Sync when props change (e.g. after refetch)
  if (applications !== localApps && applications.length > 0 && JSON.stringify(applications) !== JSON.stringify(localApps)) {
    setLocalApps(applications);
  }

  const scoreMap = useMemo(() => {
    const map = new Map<string, MatchScoreResponse>();
    for (const m of candidates) map.set(m.candidateId, m);
    return map;
  }, [candidates]);

  const cards: KanbanCardData[] = useMemo(() => {
    return localApps.map((app) => {
      const match = scoreMap.get(app.candidateId);
      return {
        applicationId: app.applicationId,
        candidateId: app.candidateId,
        candidateName: app.candidateName,
        status: app.status,
        overallScore: match?.overallScore ?? 0,
        appliedAt: app.appliedAt,
        jobId,
      };
    });
  }, [localApps, scoreMap, jobId]);

  const columnCards = useMemo(() => {
    const grouped: Record<string, KanbanCardData[]> = {};
    for (const col of PIPELINE_COLUMNS) grouped[col] = [];
    for (const card of cards) {
      if (grouped[card.status]) {
        grouped[card.status].push(card);
      }
    }
    // Sort by score descending within each column
    for (const col of PIPELINE_COLUMNS) {
      grouped[col].sort((a, b) => b.overallScore - a.overallScore);
    }
    return grouped;
  }, [cards]);

  const activeCard = useMemo(() => {
    if (!activeId) return null;
    return cards.find((c) => c.applicationId === activeId) ?? null;
  }, [activeId, cards]);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } })
  );

  const handleDragStart = useCallback((event: DragStartEvent) => {
    setActiveId(event.active.id as string);
  }, []);

  const handleDragEnd = useCallback(async (event: DragEndEvent) => {
    setActiveId(null);
    const { active, over } = event;
    if (!over) return;

    const applicationId = active.id as string;
    const newStatus = over.id as string;
    const currentCard = cards.find((c) => c.applicationId === applicationId);
    if (!currentCard || currentCard.status === newStatus) return;

    // Optimistic update
    setLocalApps((prev) =>
      prev.map((app) =>
        app.applicationId === applicationId ? { ...app, status: newStatus } : app
      )
    );

    try {
      await onStatusChange(applicationId, newStatus);
    } catch {
      // Revert on failure
      setLocalApps((prev) =>
        prev.map((app) =>
          app.applicationId === applicationId ? { ...app, status: currentCard.status } : app
        )
      );
    }
  }, [cards, onStatusChange]);

  return (
    <DndContext sensors={sensors} onDragStart={handleDragStart} onDragEnd={handleDragEnd}>
      <div className="grid grid-cols-5 gap-4">
        {PIPELINE_COLUMNS.map((status) => (
          <KanbanColumn key={status} status={status} cards={columnCards[status]} />
        ))}
      </div>
      <DragOverlay>
        {activeCard ? (
          <div className="w-64 opacity-90">
            <KanbanCard data={activeCard} />
          </div>
        ) : null}
      </DragOverlay>
    </DndContext>
  );
}
