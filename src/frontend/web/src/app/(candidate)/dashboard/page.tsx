"use client";

import { useMemo, useEffect, useState } from "react";
import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner } from "@/components/ui";
import { DashboardStats, RecentApplications, TopMatches, QuickActions } from "@/components/features/dashboard";
import { useAuth } from "@/hooks/useAuth";
import { useApi } from "@/hooks/useApi";
import { api } from "@/lib/api";
import type { ApplicationListResponse, MatchListResponse, ResumeResponse, Job } from "@/lib/types";

export default function CandidateDashboard() {
  const { user } = useAuth();
  const { data: apps, loading } = useApi<ApplicationListResponse>("/api/applications/my-applications?pageSize=5");
  const { data: matches } = useApi<MatchListResponse>("/api/matching/candidate/me/jobs?pageSize=5");
  const { data: resumes } = useApi<ResumeResponse[]>("/api/resumes/me");
  const [jobTitleMap, setJobTitleMap] = useState<Map<string, string>>(new Map());

  // Build job title map from applications + fetch missing titles for matches
  useEffect(() => {
    const map = new Map<string, string>();
    for (const app of apps?.items ?? []) {
      if (app.jobTitle) map.set(app.jobId, app.jobTitle);
    }

    const missingIds = (matches?.items ?? [])
      .map((m) => m.jobId)
      .filter((id) => !map.has(id));

    if (missingIds.length === 0) {
      setJobTitleMap(map);
      return;
    }

    Promise.all(
      [...new Set(missingIds)].map((id) =>
        api.get<Job>(`/api/jobs/${id}`).then((job) => map.set(job.jobId, job.title)).catch(() => {})
      )
    ).then(() => setJobTitleMap(new Map(map)));
  }, [apps, matches]);

  const stats = [
    { label: "Resumes", value: resumes?.length ?? 0, borderColor: "border-primary", href: "/resumes" },
    { label: "Applications", value: apps?.totalCount ?? 0, borderColor: "border-primary", href: "/applications" },
    { label: "Job Matches", value: matches?.totalCount ?? 0, borderColor: "border-primary", href: "/jobs" },
    {
      label: "Best Score",
      value: matches?.items?.[0] ? `${matches.items[0].overallScore}%` : "N/A",
      borderColor: "border-tertiary",
      href: "/skill-gap",
    },
  ];

  const greeting = user?.email ? `, ${user.email.split("@")[0]}` : "";

  return (
    <>
      <Navbar />
      <PageContainer>
        <section className="mb-10 space-y-2">
          <p className="text-primary font-semibold tracking-widest text-xs uppercase">Candidate Workspace</p>
          <h2 className="text-4xl md:text-5xl font-black text-on-surface tracking-tighter">
            Welcome back{greeting}!
          </h2>
        </section>

        <div className="space-y-10">
          <DashboardStats stats={stats} />

          <section className="grid grid-cols-1 lg:grid-cols-12 gap-10">
            <div className="lg:col-span-7">
              <RecentApplications applications={apps?.items ?? []} loading={loading} />
            </div>
            <div className="lg:col-span-5">
              <TopMatches matches={matches?.items ?? []} jobTitleMap={jobTitleMap} />
            </div>
          </section>

          <QuickActions />
        </div>
      </PageContainer>
    </>
  );
}
