"use client";

import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";
import { HeroSection, FeaturesSection, FeaturedJobs, CTASection } from "@/components/features/landing";
import { useApi } from "@/hooks/useApi";
import type { JobListResponse } from "@/lib/types";

export default function LandingPage() {
  const { data } = useApi<JobListResponse>("/api/jobs?pageSize=6");

  return (
    <>
      <Navbar />
      <HeroSection />
      <FeaturesSection />
      <FeaturedJobs jobs={data?.items ?? []} />
      <CTASection />
      <Footer />
    </>
  );
}
