import { JobMeta, SkillBadgeList } from "@/components/composed";
import type { Job } from "@/lib/types";
import type { ReactNode } from "react";

interface JobHeaderProps {
  job: Job;
  actions?: ReactNode;
}

export function JobHeader({ job, actions }: JobHeaderProps) {
  return (
    <div className="bg-surface-container-lowest rounded-xl editorial-shadow border border-outline-variant/15 p-8">
      <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-bold text-on-surface">{job.title}</h1>
          <JobMeta
            location={job.location}
            employmentType={job.employmentType}
            minSalary={job.minSalary}
            maxSalary={job.maxSalary}
            experienceMin={job.experienceYearsMin}
            experienceMax={job.experienceYearsMax}
            className="mt-3"
          />
        </div>
        {actions && <div className="flex gap-2">{actions}</div>}
      </div>

      {job.skills.length > 0 && (
        <div className="mb-6">
          <h2 className="text-lg font-semibold text-on-surface mb-3">Required Skills</h2>
          <SkillBadgeList skills={job.skills} />
        </div>
      )}

      <div>
        <h2 className="text-lg font-semibold text-on-surface mb-3">Description</h2>
        <div className="prose prose-sm max-w-none text-on-surface-variant whitespace-pre-wrap leading-relaxed">
          {job.description}
        </div>
      </div>
    </div>
  );
}
