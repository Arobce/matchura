import { ResumeCard } from "./ResumeCard";
import { Spinner } from "@/components/ui";
import type { ResumeResponse } from "@/lib/types";

interface ResumeListProps {
  resumes: ResumeResponse[];
  loading?: boolean;
}

export function ResumeList({ resumes, loading }: ResumeListProps) {
  if (loading) return <Spinner size="lg" />;

  if (resumes.length === 0) {
    return (
      <p className="text-center text-on-surface-variant py-10">
        No resumes uploaded yet. Upload your first resume to get started.
      </p>
    );
  }

  return (
    <div className="space-y-4">
      {resumes.map((resume) => (
        <ResumeCard key={resume.resumeId} resume={resume} />
      ))}
    </div>
  );
}
