import { StatusBadge } from "@/components/composed";
import { Card, EmptyState } from "@/components/ui";
import type { Application } from "@/lib/types";
import { formatDate } from "@/lib/utils";
import Link from "next/link";
import { Briefcase } from "lucide-react";

interface RecentApplicationsProps {
  applications: Application[];
  loading?: boolean;
}

export function RecentApplications({ applications, loading }: RecentApplicationsProps) {
  return (
    <Card variant="low" padding="sm" className="rounded-2xl overflow-hidden">
      <div className="p-6 pb-2 flex justify-between items-center">
        <h3 className="text-xl font-bold text-on-surface tracking-tight">Recent Applications</h3>
        <Link href="/applications" className="text-sm text-primary font-bold hover:underline">
          View All
        </Link>
      </div>
      <div className="space-y-1 p-2">
        {applications.length > 0 ? (
          applications.map((app) => (
            <div
              key={app.applicationId}
              className="bg-surface-container-lowest p-5 rounded-xl flex justify-between items-center transition-all hover:translate-x-1"
            >
              <div className="flex flex-col">
                <Link
                  href={`/applications/${app.applicationId}`}
                  className="text-on-surface font-semibold text-lg hover:text-primary transition-colors"
                >
                  {app.jobTitle || `Job: ${app.jobId.slice(0, 8)}...`}
                </Link>
                <span className="text-on-surface-variant text-sm">
                  Applied {formatDate(app.appliedAt)}
                </span>
              </div>
              <StatusBadge status={app.status} />
            </div>
          ))
        ) : (
          <EmptyState
            icon={Briefcase}
            title="No applications yet"
            action={{ label: "Browse jobs", href: "/jobs" }}
          />
        )}
      </div>
    </Card>
  );
}
