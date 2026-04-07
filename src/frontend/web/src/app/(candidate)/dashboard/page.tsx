"use client";

import { Navbar } from "@/components/layout/Navbar";
import { PageContainer, Spinner } from "@/components/ui";
import { DashboardStats, RecentApplications, TopMatches, QuickActions } from "@/components/features/dashboard";
import { useAuth } from "@/hooks/useAuth";
import { useApi } from "@/hooks/useApi";
import type { ApplicationListResponse, MatchListResponse, ResumeResponse } from "@/lib/types";

export default function CandidateDashboard() {
  const { user } = useAuth();
  const { data: apps, loading } = useApi<ApplicationListResponse>("/api/applications/me?pageSize=5");
  const { data: matches } = useApi<MatchListResponse>("/api/matching/candidate/me/jobs?pageSize=5");
  const { data: resumes } = useApi<ResumeResponse[]>("/api/resumes/me");

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
              <TopMatches matches={matches?.items ?? []} />
            </div>
          </section>

          <QuickActions />
        </div>
      </PageContainer>
    </>
  );
}
