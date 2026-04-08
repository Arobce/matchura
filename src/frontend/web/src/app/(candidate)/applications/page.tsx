"use client";

import { Navbar } from "@/components/layout/Navbar";
import { PageContainer } from "@/components/ui";
import { ApplicationList } from "@/components/features/applications";
import { useApi } from "@/hooks/useApi";
import type { ApplicationListResponse } from "@/lib/types";

export default function ApplicationsPage() {
  const { data: apps, loading } = useApi<ApplicationListResponse>("/api/applications/my-applications?pageSize=50");

  return (
    <>
      <Navbar />
      <PageContainer>
        <h1 className="text-3xl font-bold tracking-tight text-on-surface mb-8">My Applications</h1>
        <ApplicationList applications={apps?.items ?? []} loading={loading} />
      </PageContainer>
    </>
  );
}
