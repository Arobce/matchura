import { SectionHeader } from "@/components/ui";
import { JobMeta, SkillBadgeList } from "@/components/composed";
import type { Job } from "@/lib/types";
import Link from "next/link";

interface FeaturedJobsProps {
  jobs: Job[];
}

export function FeaturedJobs({ jobs }: FeaturedJobsProps) {
  if (jobs.length === 0) return null;

  return (
    <section className="py-32 bg-surface">
      <div className="max-w-screen-2xl mx-auto px-6">
        <SectionHeader
          title="Latest Opportunities"
          subtitle="Top matches currently available on our platform."
          action={{ label: "Browse all jobs", href: "/jobs" }}
          className="mb-16"
        />
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {jobs.map((job) => (
            <Link
              key={job.jobId}
              href={`/jobs/${job.jobId}`}
              className="bg-surface-container-lowest p-8 rounded-xl border border-transparent hover:border-primary/10 editorial-shadow transition-all group"
            >
              <div className="flex justify-between items-start mb-6">
                <div className="w-14 h-14 bg-surface-container-high rounded-lg flex items-center justify-center font-black text-primary text-xl">
                  {job.title.slice(0, 2).toUpperCase()}
                </div>
              </div>
              <h4 className="text-xl font-bold mb-2 text-on-surface group-hover:text-primary transition-colors">
                {job.title}
              </h4>
              <JobMeta
                location={job.location}
                employmentType={job.employmentType}
                minSalary={job.minSalary}
                maxSalary={job.maxSalary}
                className="mb-6"
              />
              <SkillBadgeList skills={job.skills} max={4} />
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}
