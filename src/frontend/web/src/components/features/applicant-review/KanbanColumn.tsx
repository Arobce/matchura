"use client";

import { useDroppable } from "@dnd-kit/core";
import { Badge } from "@/components/ui";
import { KanbanCard } from "./KanbanCard";
import { cn } from "@/lib/utils";
import type { KanbanCardData } from "./KanbanCard";
import type { ApplicationStatus } from "@/lib/types";

const columnColors: Record<ApplicationStatus, string> = {
  Submitted: "border-t-primary",
  Reviewed: "border-t-cyan-500",
  Shortlisted: "border-t-yellow-500",
  Accepted: "border-t-green-500",
  Rejected: "border-t-error",
  Withdrawn: "border-t-outline-variant",
};

const badgeVariants: Record<ApplicationStatus, "primary" | "accent" | "warning" | "success" | "danger" | "muted"> = {
  Submitted: "primary",
  Reviewed: "accent",
  Shortlisted: "warning",
  Accepted: "success",
  Rejected: "danger",
  Withdrawn: "muted",
};

interface KanbanColumnProps {
  status: ApplicationStatus;
  cards: KanbanCardData[];
}

export function KanbanColumn({ status, cards }: KanbanColumnProps) {
  const { isOver, setNodeRef } = useDroppable({ id: status });

  return (
    <div
      ref={setNodeRef}
      className={cn(
        "flex flex-col bg-surface-container-low/50 rounded-xl border-t-4 min-h-[400px]",
        columnColors[status],
        isOver && "ring-2 ring-primary/30 bg-primary-container/5"
      )}
    >
      <div className="flex items-center justify-between px-4 py-3">
        <h3 className="text-sm font-bold text-on-surface">{status}</h3>
        <Badge variant={badgeVariants[status]} size="sm">{cards.length}</Badge>
      </div>
      <div className="flex-1 px-3 pb-3 space-y-2 overflow-y-auto max-h-[calc(100vh-280px)]">
        {cards.map((card) => (
          <KanbanCard key={card.applicationId} data={card} />
        ))}
        {cards.length === 0 && (
          <p className="text-xs text-on-surface-variant text-center py-8">No candidates</p>
        )}
      </div>
    </div>
  );
}
