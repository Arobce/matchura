import { QuickActionCard } from "@/components/composed";
import { Upload, Search, BarChart3 } from "lucide-react";

export function QuickActions() {
  return (
    <section className="grid grid-cols-1 md:grid-cols-3 gap-6">
      <QuickActionCard
        icon={Upload}
        label="Upload Resume"
        description="Keep your profile updated with your latest achievements"
        href="/resumes"
      />
      <QuickActionCard
        icon={Search}
        label="Browse Jobs"
        description="Explore thousands of open positions curated for you"
        href="/jobs"
      />
      <QuickActionCard
        icon={BarChart3}
        label="Skill Gap Analysis"
        description="Identify key skills needed for your dream role"
        href="/skill-gap"
      />
    </section>
  );
}
