import { ApplicationRow } from "./ApplicationRow";
import { EmptyState, Spinner } from "@/components/ui";
import type { Application } from "@/lib/types";
import { FileText } from "lucide-react";

interface ApplicationListProps {
  applications: Application[];
  loading?: boolean;
}

export function ApplicationList({ applications, loading }: ApplicationListProps) {
  if (loading) return <Spinner size="lg" />;

  if (applications.length === 0) {
    return (
      <EmptyState
        icon={FileText}
        title="No applications yet"
        description="Browse jobs to get started"
        action={{ label: "Browse jobs", href: "/jobs" }}
      />
    );
  }

  return (
    <div className="space-y-2">
      {applications.map((app) => (
        <ApplicationRow key={app.applicationId} application={app} />
      ))}
    </div>
  );
}
