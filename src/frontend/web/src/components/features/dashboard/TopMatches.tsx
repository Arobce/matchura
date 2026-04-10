import { ScoreDisplay } from "@/components/composed";
import { Card } from "@/components/ui";
import type { MatchScoreResponse } from "@/lib/types";
import Link from "next/link";
import { ChevronRight } from "lucide-react";

interface TopMatchesProps {
  matches: MatchScoreResponse[];
  jobTitleMap?: Map<string, string>;
}

export function TopMatches({ matches, jobTitleMap }: TopMatchesProps) {
  return (
    <Card variant="high" padding="md" className="rounded-2xl">
      <div className="flex justify-between items-center mb-6">
        <h3 className="text-xl font-bold text-on-surface tracking-tight">Top Job Matches</h3>
        <Link href="/jobs" className="text-sm text-primary font-bold hover:underline">
          Browse Jobs
        </Link>
      </div>
      <div className="space-y-4">
        {matches.length > 0 ? (
          matches.map((match) => (
            <Link
              key={match.matchScoreId}
              href={`/jobs/${match.jobId}`}
              className="flex items-center gap-4 bg-surface/50 p-4 rounded-xl border border-outline-variant/10 hover:bg-surface-container-lowest transition-colors"
            >
              <ScoreDisplay score={match.overallScore} size="md" />
              <div className="flex-1">
                <p className="text-on-surface font-bold text-base leading-tight">
                  {jobTitleMap?.get(match.jobId) || `Job: ${match.jobId.slice(0, 8)}...`}
                </p>
                <p className="text-on-surface-variant text-xs">
                  Skills: {match.skillScore}% | Exp: {match.experienceScore}%
                </p>
              </div>
              <ChevronRight className="h-5 w-5 text-primary" />
            </Link>
          ))
        ) : (
          <p className="text-sm text-on-surface-variant py-4">
            Upload a resume and compute match scores to see recommendations.
          </p>
        )}
      </div>
    </Card>
  );
}
