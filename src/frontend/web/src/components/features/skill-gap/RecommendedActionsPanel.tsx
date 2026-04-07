import { Card } from "@/components/ui";
import { Clock, BookOpen } from "lucide-react";
import type { RecommendedAction } from "@/lib/types";

interface RecommendedActionsPanelProps {
  actions: RecommendedAction[];
}

export function RecommendedActionsPanel({ actions }: RecommendedActionsPanelProps) {
  if (actions.length === 0) return null;

  return (
    <Card variant="lowest" padding="lg" className="rounded-2xl">
      <h3 className="text-sm font-bold text-on-surface-variant uppercase tracking-widest mb-6">Development Roadmap</h3>
      <div className="space-y-6">
        {actions.map((action, i) => (
          <div key={i} className="flex gap-4 group">
            <div className="flex-shrink-0 w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center font-bold text-sm">
              {action.priority}
            </div>
            <div className="flex-1 pb-6 border-b border-outline-variant/10 last:border-0">
              <h4 className="text-sm font-bold text-on-surface group-hover:text-primary transition-colors mb-1">
                {action.action}
              </h4>
              <p className="text-xs text-on-surface-variant mb-3">{action.rationale}</p>
              <div className="flex gap-3">
                <span className="flex items-center gap-1 text-[10px] font-semibold text-on-surface-variant bg-surface-container-low px-2 py-1 rounded">
                  <Clock className="h-3 w-3" /> {action.estimatedTime}
                </span>
                <span className="flex items-center gap-1 text-[10px] font-semibold text-on-surface-variant bg-surface-container-low px-2 py-1 rounded">
                  <BookOpen className="h-3 w-3" /> {action.resourceType}
                </span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
}
