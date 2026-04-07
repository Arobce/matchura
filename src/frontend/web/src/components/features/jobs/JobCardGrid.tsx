import { JobCard } from "./JobCard";
import { EmptyState } from "@/components/ui";
import { Briefcase } from "lucide-react";
import type { Job } from "@/lib/types";

interface JobCardGridProps {
  jobs: Job[];
}

export function JobCardGrid({ jobs }: JobCardGridProps) {
  if (jobs.length === 0) {
    return (
      <EmptyState
        icon={Briefcase}
        title="No jobs found matching your criteria"
        description="Try adjusting your search filters."
      />
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {jobs.map((job) => (
        <JobCard key={job.jobId} job={job} />
      ))}
    </div>
  );
}
