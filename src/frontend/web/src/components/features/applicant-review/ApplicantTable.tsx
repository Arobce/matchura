"use client";

import { ScoreDisplay, StatusBadge } from "@/components/composed";
import { Button } from "@/components/ui";
import type { MatchScoreResponse } from "@/lib/types";

interface ApplicantTableProps {
  candidates: MatchScoreResponse[];
  nameMap?: Map<string, string>;
  onStatusChange?: (candidateId: string, status: string) => void;
  onSelect?: (match: MatchScoreResponse) => void;
}

export function ApplicantTable({ candidates, nameMap, onStatusChange, onSelect }: ApplicantTableProps) {
  if (candidates.length === 0) {
    return <p className="text-center py-10 text-on-surface-variant">No applicants yet.</p>;
  }

  return (
    <div className="bg-surface-container-lowest rounded-2xl editorial-shadow overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <thead>
            <tr className="bg-surface-container-low/50">
              <th className="px-6 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Candidate</th>
              <th className="px-6 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Match Score</th>
              <th className="px-6 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Skills</th>
              <th className="px-6 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Experience</th>
              <th className="px-6 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Education</th>
              <th className="px-6 py-4 text-xs font-bold text-on-surface-variant uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/5">
            {candidates.map((match) => (
              <tr
                key={match.matchScoreId}
                className="hover:bg-surface-container-low/30 transition-colors cursor-pointer"
                onClick={() => onSelect?.(match)}
              >
                <td className="px-6 py-5">
                  <span className="text-sm font-semibold text-on-surface">
                    {nameMap?.get(match.candidateId) || `Candidate ${match.candidateId.slice(0, 8)}...`}
                  </span>
                </td>
                <td className="px-6 py-5">
                  <ScoreDisplay score={match.overallScore} size="sm" />
                </td>
                <td className="px-6 py-5 text-sm font-medium text-on-surface">{match.skillScore}%</td>
                <td className="px-6 py-5 text-sm font-medium text-on-surface">{match.experienceScore}%</td>
                <td className="px-6 py-5 text-sm font-medium text-on-surface">{match.educationScore}%</td>
                <td className="px-6 py-5">
                  <div className="flex gap-2" onClick={(e) => e.stopPropagation()}>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => onStatusChange?.(match.candidateId, "Shortlisted")}
                    >
                      Shortlist
                    </Button>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => onStatusChange?.(match.candidateId, "Rejected")}
                    >
                      Reject
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
