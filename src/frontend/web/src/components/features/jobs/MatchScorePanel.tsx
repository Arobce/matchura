import { ScoreDisplay, ScoreBreakdown } from "@/components/composed";
import type { MatchScoreResponse } from "@/lib/types";

interface MatchScorePanelProps {
  matchScore: MatchScoreResponse;
}

export function MatchScorePanel({ matchScore }: MatchScorePanelProps) {
  return (
    <div className="bg-primary-container/10 rounded-xl p-6">
      <div className="flex items-center gap-3 mb-4">
        <ScoreDisplay score={matchScore.overallScore} label="Overall Match" size="lg" />
      </div>
      <ScoreBreakdown
        skillScore={matchScore.skillScore}
        experienceScore={matchScore.experienceScore}
        educationScore={matchScore.educationScore}
        className="mb-4"
      />
      {matchScore.explanation && (
        <p className="text-sm text-on-surface-variant leading-relaxed">{matchScore.explanation}</p>
      )}
      {matchScore.strengths.length > 0 && (
        <div className="mt-4">
          <h4 className="text-sm font-semibold text-on-surface mb-2">Strengths</h4>
          <ul className="text-sm text-on-surface-variant space-y-1">
            {matchScore.strengths.map((s, i) => (
              <li key={i} className="flex items-start gap-2">
                <span className="text-success mt-0.5">+</span> {s}
              </li>
            ))}
          </ul>
        </div>
      )}
      {matchScore.gaps.length > 0 && (
        <div className="mt-4">
          <h4 className="text-sm font-semibold text-on-surface mb-2">Gaps</h4>
          <ul className="text-sm text-on-surface-variant space-y-1">
            {matchScore.gaps.map((g, i) => (
              <li key={i} className="flex items-start gap-2">
                <span className="text-warning mt-0.5">-</span> {g}
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
