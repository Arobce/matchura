import { JobMeta, SkillBadgeList } from "@/components/composed";
import type { Job } from "@/lib/types";
import Link from "next/link";

interface JobCardProps {
  job: Job;
}

export function JobCard({ job }: JobCardProps) {
  return (
    <Link
      href={`/jobs/${job.jobId}`}
      className="bg-surface-container-lowest p-6 rounded-xl border border-transparent hover:border-primary/20 hover:shadow-xl transition-all duration-300 group block"
    >
      <div className="flex items-start justify-between mb-4">
        <div className="w-12 h-12 rounded-lg bg-surface-container flex items-center justify-center font-black text-primary text-lg">
          {job.title.slice(0, 2).toUpperCase()}
        </div>
      </div>
      <h3 className="text-lg font-bold text-on-surface mb-1 group-hover:text-primary transition-colors">
        {job.title}
      </h3>
      <JobMeta
        location={job.location}
        employmentType={job.employmentType}
        minSalary={job.minSalary}
        maxSalary={job.maxSalary}
        className="mb-6"
      />
      <p className="text-sm text-on-surface-variant/80 mb-6 line-clamp-2 leading-relaxed">
        {job.description}
      </p>
      <SkillBadgeList skills={job.skills} max={4} />
    </Link>
  );
}
