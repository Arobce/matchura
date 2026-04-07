"use client";

import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";
import { PageContainer, Spinner, Pagination } from "@/components/ui";
import { JobCardGrid, JobFilters } from "@/components/features/jobs";
import { useJobFilterStore } from "@/stores";
import { useApi } from "@/hooks/useApi";
import type { JobListResponse } from "@/lib/types";

export default function JobsPage() {
  const { getQueryString, page, setPage } = useJobFilterStore();
  const { data: jobs, loading } = useApi<JobListResponse>(`/api/jobs?${getQueryString()}`);

  return (
    <>
      <Navbar />
      <PageContainer>
        <h1 className="text-3xl font-bold tracking-tight text-on-surface mb-8">Browse Jobs</h1>
        <JobFilters />
        {loading ? (
          <Spinner size="lg" />
        ) : (
          <>
            {jobs && (
              <p className="text-on-surface-variant font-medium text-sm tracking-wide uppercase mb-8">
                {jobs.totalCount} job{jobs.totalCount !== 1 ? "s" : ""} found
              </p>
            )}
            <JobCardGrid jobs={jobs?.items ?? []} />
            {jobs && <Pagination page={page} totalPages={jobs.totalPages} onPageChange={setPage} />}
          </>
        )}
      </PageContainer>
      <Footer />
    </>
  );
}
