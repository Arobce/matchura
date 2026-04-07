"use client";

import Link from "next/link";
import { useDraggable } from "@dnd-kit/core";
import { CSS } from "@dnd-kit/utilities";
import { ScoreDisplay } from "@/components/composed";
import { formatRelativeDate } from "@/lib/utils";
import { cn } from "@/lib/utils";

export interface KanbanCardData {
  applicationId: string;
  candidateId: string;
  status: string;
  overallScore: number;
  appliedAt: string;
  jobId: string;
}

interface KanbanCardProps {
  data: KanbanCardData;
}

export function KanbanCard({ data }: KanbanCardProps) {
  const isWithdrawn = data.status === "Withdrawn";

  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: data.applicationId,
    data: { candidateId: data.candidateId, currentStatus: data.status },
    disabled: isWithdrawn,
  });

  const style = transform
    ? { transform: CSS.Translate.toString(transform) }
    : undefined;

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn(
        "bg-surface-container-lowest rounded-lg editorial-shadow p-3 transition-shadow",
        isDragging && "shadow-lg opacity-80 z-50",
        isWithdrawn && "opacity-40 cursor-default",
        !isWithdrawn && "cursor-grab active:cursor-grabbing"
      )}
      {...listeners}
      {...attributes}
    >
      <Link
        href={`/employer/jobs/${data.jobId}/applicants/${data.applicationId}`}
        onClick={(e) => { if (isDragging) e.preventDefault(); }}
        className="block"
      >
        <div className="flex items-center justify-between gap-2 mb-2">
          <span className="text-sm font-semibold text-on-surface truncate">
            {data.candidateId.slice(0, 8)}...
          </span>
          <ScoreDisplay score={data.overallScore} size="sm" />
        </div>
        <p className="text-xs text-on-surface-variant">
          Applied {formatRelativeDate(data.appliedAt)}
        </p>
      </Link>
    </div>
  );
}
