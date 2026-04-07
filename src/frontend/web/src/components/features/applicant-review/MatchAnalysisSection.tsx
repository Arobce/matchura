import { ScoreBreakdown } from "@/components/composed";
import type { MatchScoreResponse } from "@/lib/types";

interface MatchAnalysisSectionProps {
  match: MatchScoreResponse;
}

export function MatchAnalysisSection({ match }: MatchAnalysisSectionProps) {
  return (
    <div className="bg-surface-container-lowest rounded-xl editorial-shadow p-6 space-y-6">
      <h3 className="text-lg font-bold text-on-surface">Match Analysis</h3>

      <ScoreBreakdown
        skillScore={match.skillScore}
        experienceScore={match.experienceScore}
        educationScore={match.educationScore}
      />

      {match.explanation && (
        <div>
          <h4 className="text-sm font-bold text-on-surface mb-2">Analysis</h4>
          <p className="text-sm text-on-surface-variant leading-relaxed">{match.explanation}</p>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {match.strengths.length > 0 && (
          <div>
            <h4 className="text-sm font-bold text-on-surface mb-3">Strengths</h4>
            <ul className="space-y-2">
              {match.strengths.map((s, i) => (
                <li key={i} className="flex items-start gap-2 text-sm text-on-surface-variant">
                  <span className="text-green-500 mt-0.5">&#10003;</span>
                  {s}
                </li>
              ))}
            </ul>
          </div>
        )}

        {match.gaps.length > 0 && (
          <div>
            <h4 className="text-sm font-bold text-on-surface mb-3">Gaps</h4>
            <ul className="space-y-2">
              {match.gaps.map((g, i) => (
                <li key={i} className="flex items-start gap-2 text-sm text-on-surface-variant">
                  <span className="text-warning mt-0.5">!</span>
                  {g}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}
