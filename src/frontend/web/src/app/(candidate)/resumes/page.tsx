"use client";

import { Navbar } from "@/components/layout/Navbar";
import { PageContainer } from "@/components/ui";
import { ResumeUploader, ResumeList } from "@/components/features/resumes";
import { useApi } from "@/hooks/useApi";
import type { ResumeResponse } from "@/lib/types";

export default function ResumesPage() {
  const { data: resumes, loading, refetch } = useApi<ResumeResponse[]>("/api/resumes/me");

  return (
    <>
      <Navbar />
      <PageContainer>
        <h1 className="text-3xl font-bold tracking-tight text-on-surface mb-8">My Resumes</h1>
        <ResumeUploader onUploadComplete={refetch} />
        <ResumeList resumes={resumes ?? []} loading={loading} />
      </PageContainer>
    </>
  );
}
