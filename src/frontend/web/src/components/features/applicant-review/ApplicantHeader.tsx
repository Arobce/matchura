"use client";

import Link from "next/link";
import { ScoreDisplay, StatusBadge } from "@/components/composed";
import { Button } from "@/components/ui";
import type { Application, CandidateProfile } from "@/lib/types";

interface ApplicantHeaderProps {
  application: Application;
  profile: CandidateProfile | null;
  score: number | null;
  jobId: string;
  onStatusChange: (status: string) => void;
}

const statusActions: Record<string, { label: string; status: string; variant: "primary" | "danger" }[]> = {
  Submitted: [{ label: "Mark Reviewed", status: "Reviewed", variant: "primary" }],
  Reviewed: [
    { label: "Shortlist", status: "Shortlisted", variant: "primary" },
    { label: "Reject", status: "Rejected", variant: "danger" },
  ],
  Shortlisted: [
    { label: "Accept", status: "Accepted", variant: "primary" },
    { label: "Reject", status: "Rejected", variant: "danger" },
  ],
};

export function ApplicantHeader({ application, profile, score, jobId, onStatusChange }: ApplicantHeaderProps) {
  const actions = statusActions[application.status] ?? [];

  return (
    <div className="bg-surface-container-lowest rounded-xl editorial-shadow p-6">
      <div className="flex items-center gap-2 mb-6">
        <Link
          href={`/employer/jobs/${jobId}/applicants`}
          className="text-sm text-primary hover:underline"
        >
          &larr; Back to Pipeline
        </Link>
      </div>

      <div className="flex flex-col md:flex-row md:items-start justify-between gap-6">
        <div className="flex items-start gap-5">
          {score !== null && <ScoreDisplay score={score} size="lg" />}
          <div>
            <h2 className="text-xl font-bold text-on-surface mb-1">
              {application.candidateName || `Candidate ${application.candidateId.slice(0, 8)}...`}
            </h2>
            <div className="flex items-center gap-3 mb-3">
              <StatusBadge status={application.status} />
            </div>
            {profile && (
              <div className="space-y-1 text-sm text-on-surface-variant">
                {profile.location && <p>{profile.location}</p>}
                {profile.yearsOfExperience > 0 && <p>{profile.yearsOfExperience} years of experience</p>}
                {profile.highestEducation && <p>{profile.highestEducation}</p>}
              </div>
            )}
          </div>
        </div>

        {actions.length > 0 && (
          <div className="flex gap-3">
            {actions.map((action) => (
              <Button
                key={action.status}
                variant={action.variant}
                onClick={() => onStatusChange(action.status)}
              >
                {action.label}
              </Button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
