import { ScoreDisplay, ScoreBreakdown } from "@/components/composed";
import { Card } from "@/components/ui";
import type { MatchScoreResponse } from "@/lib/types";
import { CheckCircle2, AlertCircle } from "lucide-react";

interface ApplicantDetailPanelProps {
  match: MatchScoreResponse;
}

export function ApplicantDetailPanel({ match }: ApplicantDetailPanelProps) {
  return (
    <Card variant="lowest" padding="lg" className="rounded-2xl border border-outline-variant/15">
      <div className="flex items-center gap-4 mb-6">
        <ScoreDisplay score={match.overallScore} size="lg" />
        <div>
          <p className="font-bold text-on-surface">Candidate: {match.candidateId.slice(0, 8)}...</p>
          <p className="text-xs text-on-surface-variant">Generated {new Date(match.generatedAt).toLocaleDateString()}</p>
        </div>
      </div>

      <ScoreBreakdown
        skillScore={match.skillScore}
        experienceScore={match.experienceScore}
        educationScore={match.educationScore}
        className="mb-6"
      />

      {match.explanation && (
        <p className="text-sm text-on-surface-variant mb-6 leading-relaxed">{match.explanation}</p>
      )}

      {match.strengths.length > 0 && (
        <div className="mb-4">
          <h4 className="text-sm font-bold text-on-surface mb-2">Strengths</h4>
          <div className="space-y-2">
            {match.strengths.map((s, i) => (
              <div key={i} className="flex items-start gap-2 text-sm text-on-surface-variant">
                <CheckCircle2 className="h-4 w-4 text-success shrink-0 mt-0.5" /> {s}
              </div>
            ))}
          </div>
        </div>
      )}

      {match.gaps.length > 0 && (
        <div>
          <h4 className="text-sm font-bold text-on-surface mb-2">Gaps</h4>
          <div className="space-y-2">
            {match.gaps.map((g, i) => (
              <div key={i} className="flex items-start gap-2 text-sm text-on-surface-variant">
                <AlertCircle className="h-4 w-4 text-warning shrink-0 mt-0.5" /> {g}
              </div>
            ))}
          </div>
        </div>
      )}
    </Card>
  );
}
