"use client";

import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner, Card, SectionHeader, Badge, Button } from "@/components/ui";
import { EmployerStatsRow, PipelineChart, SkillDemandChart } from "@/components/features/analytics";
import { StatusBadge } from "@/components/composed";
import { useApi } from "@/hooks/useApi";
import type { EmployerDashboardResponse, JobListResponse } from "@/lib/types";
import Link from "next/link";
import { formatRelativeDate } from "@/lib/utils";
import { Briefcase, ChevronRight, Users } from "lucide-react";

export default function EmployerDashboard() {
  const { data: dashboard, loading } = useApi<EmployerDashboardResponse>("/api/analytics/employer/dashboard");
  const { data: myJobs, loading: jobsLoading } = useApi<JobListResponse>("/api/jobs/my-jobs?pageSize=5");

  return (
    <>
      <Navbar />
      <PageContainer>
        <header className="mb-10">
          <h1 className="text-4xl font-extrabold tracking-tight text-on-surface mb-2">Employer Dashboard</h1>
          <p className="text-on-surface-variant font-medium">Welcome back. Here&apos;s your workspace overview.</p>
        </header>

        {loading || !dashboard ? (
          <Spinner size="lg" />
        ) : (
          <div className="space-y-10">
            <EmployerStatsRow dashboard={dashboard} />

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
              <PipelineChart breakdown={dashboard.pipelineBreakdown} />
              <SkillDemandChart skills={dashboard.topSkillsInDemand} />
            </div>

            {/* Posted Jobs */}
            <section>
              <SectionHeader
                title="Your Job Postings"
                subtitle="Manage your active listings"
                action={{ label: "Post a Job", href: "/employer/jobs/create" }}
              />
              {jobsLoading ? (
                <Spinner />
              ) : myJobs && myJobs.items.length > 0 ? (
                <div className="space-y-3 mt-4">
                  {myJobs.items.map((job) => (
                    <Link key={job.jobId} href={`/employer/jobs/${job.jobId}/applicants`}>
                      <Card variant="lowest" padding="md" hover className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className="bg-primary-container/10 p-2.5 rounded-lg">
                            <Briefcase className="h-5 w-5 text-primary" />
                          </div>
                          <div>
                            <h3 className="font-bold text-on-surface">{job.title}</h3>
                            <div className="flex items-center gap-3 mt-1 text-sm text-on-surface-variant">
                              <span>{job.location || "Remote"}</span>
                              <span>&middot;</span>
                              <span>{job.employmentType}</span>
                              <span>&middot;</span>
                              <span>Posted {formatRelativeDate(job.createdAt)}</span>
                            </div>
                          </div>
                        </div>
                        <div className="flex items-center gap-4">
                          <Badge variant={job.status === "Active" ? "success" : "muted"}>{job.status}</Badge>
                          <div className="flex items-center gap-1 text-sm text-on-surface-variant">
                            <Users className="h-4 w-4" />
                            <span>{job.skills.length} skills</span>
                          </div>
                          <ChevronRight className="h-5 w-5 text-on-surface-variant" />
                        </div>
                      </Card>
                    </Link>
                  ))}
                  {myJobs.totalCount > 5 && (
                    <div className="text-center pt-2">
                      <Link href="/employer/jobs" className="text-sm text-primary font-medium hover:text-primary-dim">
                        View all {myJobs.totalCount} jobs &rarr;
                      </Link>
                    </div>
                  )}
                </div>
              ) : (
                <Card variant="low" padding="lg" className="text-center mt-4">
                  <p className="text-on-surface-variant mb-4">You haven&apos;t posted any jobs yet.</p>
                  <Button href="/employer/jobs/create">Post Your First Job</Button>
                </Card>
              )}
            </section>
          </div>
        )}
      </PageContainer>
    </>
  );
}
